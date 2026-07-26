using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VLCNP.Core;
using VLCNP.UI;

namespace VLCNP.SceneManagement
{
    /**
     * シーン遷移をする
     */
    public class TransitionEvent : MonoBehaviour
    {
        public enum TransitionState
        {
            Idle,
            Transitioning,
            Completed,
        }

        private static int activeTransitionCount = 0;

        public static bool IsAnyTransitionRunning => activeTransitionCount > 0;
        public static TransitionState SharedState { get; private set; } = TransitionState.Idle;

        [SerializeField]
        int sceneToLoad = -1;

        // シーン遷移後に出現するTransitionSpawnPointの名前
        [SerializeField]
        string destinationSpawnPointName = "A";

        [SerializeField]
        float fadeOutTime = 1f;

        [SerializeField]
        float fadeWaitTime = 0.2f;

        [SerializeField]
        float fadeInTime = 1f;

        [SerializeField]
        string autoSaveFileName = "autoSave";

        [SerializeField]
        bool isAutoSave = true;

        [Header("移動先のシーンでリトライするか")]
        [SerializeField]
        bool isRetryOnDestination = false;

        [Header("移動先のシーンでエリア名を表示するか")]
        [SerializeField]
        bool isShowAreaName = false;

        [Header("遷移開始時にプレイヤーを固定するか（フェードアウト中の落下を防ぐ）")]
        [SerializeField]
        bool isFreezePlayerOnStart = true;

        private AudioSource BGM;
        private AreaBGM areaBGM;

        bool isTransitioning = false;
        bool isRegisteredSharedState = false;

        private class FrozenPlayer
        {
            public Rigidbody2D Rigidbody;
            public RigidbodyConstraints2D Constraints;
        }

        private readonly List<FrozenPlayer> frozenPlayers = new List<FrozenPlayer>();

        public void ExecuteTransition()
        {
            if (isTransitioning)
            {
                Debug.LogWarning("TransitionEvent is already running.");
                return;
            }
            isTransitioning = true;
            StartCoroutine(Transition());
        }

        public IEnumerator Transition()
        {
            if (sceneToLoad < 0)
            {
                isTransitioning = false;
                Debug.LogError("Scene to load not set");
                yield break;
            }
            RegisterSharedState();
            try
            {
                DontDestroyOnLoad(gameObject);

                // フェードアウト中にプレイヤーが落下・移動しないよう固定する
                FreezePlayers();

                // SceneFaderタグでFaderを取得
                Fader fader = GameObject.FindWithTag("SceneFader").GetComponent<Fader>();
                yield return fader.FadeOut(fadeOutTime);

                // キャラたちの状態保存
                SavingWrapper savingWrapper = FindObjectOfType<SavingWrapper>();
                if (isAutoSave)
                {
                    savingWrapper.Save(autoSaveFileName);
                }

                yield return new WaitForSeconds(fadeWaitTime / 2);

                yield return SceneManager.LoadSceneAsync(sceneToLoad);
                print("scene load end: " + sceneToLoad);
                // キャラたちの状態復元
                // 遷移後 こちらのシーンでのSaving wrapperを再取得
                savingWrapper = FindObjectOfType<SavingWrapper>();
                savingWrapper.LoadOnlyState(autoSaveFileName);

                // BGMの変更があれば変更
                yield return ChangeBGM();

                TransitionSpawnPoint transitionSpawnPoint =
                    GetTransitionSpawnPoint()
                    ?? throw new System.Exception("Transition spawn point not found");
                print("transition spawn point found");
                UpdatePlayerPosition(transitionSpawnPoint);

                // 遷移前のプレイヤーが残っている場合に備えて固定を解除する
                UnfreezePlayers();

                yield return new WaitForSeconds(fadeWaitTime / 2);

                // 1フレーム待つ この間に自動起動イベント等を起こさせる
                yield return null;

                // 自動起動イベントが起きた後にフェードインする
                fader = GameObject.FindWithTag("SceneFader").GetComponent<Fader>();
                print("fade in start");
                yield return fader.FadeIn(fadeInTime);
                print("fade in end");
                SharedState = TransitionState.Completed;

                if (isRetryOnDestination)
                {
                    // 遷移先でリトライする場合は遷移後のシーンでオートセーブ
                    savingWrapper.AutoSave();
                }

                if (isShowAreaName)
                {
                    // 遷移先でエリア名を表示する場合は遷移後のシーンでエリア名表示
                    AreaNameShow areaNameShow = GameObject
                        .FindWithTag("AreaName")
                        .GetComponent<AreaNameShow>();
                    if (areaNameShow)
                        areaNameShow.Show();
                }

                Destroy(gameObject);
            }
            finally
            {
                UnfreezePlayers();
                UnregisterSharedState();
            }
        }

        private void OnDestroy()
        {
            UnfreezePlayers();
            UnregisterSharedState();
        }

        /// <summary>
        /// 遷移開始時にPlayerタグのオブジェクトを物理的に固定する。
        /// フェードアウト中に壁キック中のプレイヤーが落下して見えるのを防ぐ。
        /// </summary>
        private void FreezePlayers()
        {
            if (!isFreezePlayerOnStart)
                return;
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (player == null)
                    continue;
                // 歩行アニメーションが流れ続けないように止める
                var mover = player.GetComponent<Movement.Mover>();
                if (mover != null)
                    mover.Stop();

                Rigidbody2D rbody = player.GetComponent<Rigidbody2D>();
                if (rbody == null)
                    continue;
                if (frozenPlayers.Exists(frozen => frozen.Rigidbody == rbody))
                    continue;
                frozenPlayers.Add(
                    new FrozenPlayer { Rigidbody = rbody, Constraints = rbody.constraints }
                );
                rbody.velocity = Vector2.zero;
                rbody.angularVelocity = 0f;
                rbody.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }

        private void UnfreezePlayers()
        {
            if (frozenPlayers.Count == 0)
                return;
            foreach (FrozenPlayer frozen in frozenPlayers)
            {
                // シーン遷移で元のプレイヤーごと破棄されている場合は何もしない
                if (frozen.Rigidbody == null)
                    continue;
                frozen.Rigidbody.constraints = frozen.Constraints;
                frozen.Rigidbody.velocity = Vector2.zero;
                frozen.Rigidbody.angularVelocity = 0f;
            }
            frozenPlayers.Clear();
        }

        private void RegisterSharedState()
        {
            if (isRegisteredSharedState)
                return;
            activeTransitionCount++;
            SharedState = TransitionState.Transitioning;
            isRegisteredSharedState = true;
        }

        private void UnregisterSharedState()
        {
            if (!isRegisteredSharedState)
                return;

            activeTransitionCount = Mathf.Max(0, activeTransitionCount - 1);
            isRegisteredSharedState = false;
            isTransitioning = false;

            if (activeTransitionCount == 0)
            {
                SharedState = TransitionState.Idle;
            }
        }

        private IEnumerator ChangeBGM()
        {
            // 現在のBGMを取得
            BGM = GameObject.FindWithTag("BGM").GetComponent<AudioSource>();
            // エリアのBGMを取得
            areaBGM = GameObject.FindWithTag("AreaBGM").GetComponent<AreaBGM>();
            if (areaBGM.GetAudioClip() == null)
            {
                print("エリアBGMが設定されていません");
                BGM.Stop();
                BGM.clip = areaBGM.GetAudioClip();
                BGM.volume = areaBGM.GetVolume();
                BGM.pitch = areaBGM.GetPitch();
                BGM.Play();
                yield break;
            }
            if (
                BGM.clip != null
                && areaBGM.GetAudioClip() != null
                && BGM.clip.name == areaBGM.GetAudioClip().name
            )
            {
                print("クリップの変更なし");
                yield break;
            }
            print("クリップの変更");
            // clipの変更があれば変更
            yield return BGMFadeRoutine(0, fadeWaitTime);
            BGM.Stop();
            BGM.clip = areaBGM.GetAudioClip();
            BGM.volume = areaBGM.GetVolume();
            BGM.pitch = areaBGM.GetPitch();
            BGM.Play();
        }

        private IEnumerator BGMFadeRoutine(float targetVolume, float time)
        {
            while (!Mathf.Approximately(BGM.volume, targetVolume))
            {
                BGM.volume = Mathf.MoveTowards(BGM.volume, targetVolume, Time.deltaTime / time);
                yield return null;
            }
        }

        private void UpdatePlayerPosition(TransitionSpawnPoint transitionSpawnPoint)
        {
            print("UpdatePlayerPosition");
            // Playerタグ全てをspawnPointの位置に移動
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                player.transform.position = transitionSpawnPoint.transform.position;
                // 向きを変える
                var mover = player.GetComponent<Movement.Mover>();
                if (mover != null)
                {
                    mover.IsLeft = transitionSpawnPoint.isPlayerDirectionLeft;
                }
            }
        }

        private TransitionSpawnPoint GetTransitionSpawnPoint()
        {
            foreach (TransitionSpawnPoint spawnPoint in FindObjectsOfType<TransitionSpawnPoint>())
            {
                if (spawnPoint.spawnPointName != destinationSpawnPointName)
                    continue;
                return spawnPoint;
            }
            return null;
        }
    }
}
