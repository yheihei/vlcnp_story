using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using VLCNP.Control;
using VLCNP.Core;
using VLCNP.UI;

namespace VLCNP.SceneManagement
{
    /**
     * シーン遷移をする
     */
    public class TransitionEvent : MonoBehaviour
    {
        [SerializeField] int sceneToLoad = -1;

        // シーン遷移後に出現するTransitionSpawnPointの名前
        [SerializeField] string destinationSpawnPointName = "A";

        [SerializeField] float fadeOutTime = 1f;
        [SerializeField] float fadeWaitTime = 0.2f;
        [SerializeField] float fadeInTime = 1f;
        [SerializeField] string autoSaveFileName = "autoSave";
        [SerializeField] bool isAutoSave = true;
        [Header("移動先のシーンでリトライするか")]
        [SerializeField] bool isRetryOnDestination = false;
        [Header("移動先のシーンでエリア名を表示するか")]
        [SerializeField] bool isShowAreaName = false;

        private AudioSource BGM;
        private AreaBGM areaBGM;

        bool isTransitioning = false;

        public void ExecuteTransition()
        {
            StartCoroutine(Transition());
        }

        public IEnumerator Transition()
        {
            if (sceneToLoad < 0)
            {
                Debug.LogError("Scene to load not set");
                yield break;
            }
            DontDestroyOnLoad(gameObject);

            // StoppableControllerをタグから取得
            StoppableController stoppableController = GameObject.FindWithTag("StoppableController").GetComponent<StoppableController>();
            stoppableController?.StopAll();

            // SceneFaderタグでFaderを取得
            Fader fader = GameObject.FindWithTag("SceneFader").GetComponent<Fader>();
            yield return fader.FadeOut(fadeOutTime);

            // キャラたちの状態保存
            SavingWrapper savingWrapper = FindObjectOfType<SavingWrapper>();
            if (isAutoSave)
            {
                savingWrapper.Save(autoSaveFileName);
            }

            yield return new WaitForSeconds(fadeWaitTime/2);

            yield return SceneManager.LoadSceneAsync(sceneToLoad);
            print("scene load end: " + sceneToLoad);
            // キャラたちの状態復元
            // 遷移後 こちらのシーンでのSaving wrapperを再取得
            savingWrapper = FindObjectOfType<SavingWrapper>();
            savingWrapper.LoadOnlyState(autoSaveFileName);

            // BGMの変更があれば変更
            yield return ChangeBGM();

            TransitionSpawnPoint transitionSpawnPoint = GetTransitionSpawnPoint() ?? throw new System.Exception("Transition spawn point not found");
            print("transition spawn point found");
            UpdatePlayerPosition(transitionSpawnPoint);

            yield return new WaitForSeconds(fadeWaitTime/2);

            fader = GameObject.FindWithTag("SceneFader").GetComponent<Fader>();
            print("fade in start");
            yield return fader.FadeIn(fadeInTime);
            print("fade in end");

            if (isRetryOnDestination)
            {
                // 遷移先でリトライする場合は遷移後のシーンでオートセーブ
                savingWrapper.AutoSave();
            }

            if (isShowAreaName)
            {
                // 遷移先でエリア名を表示する場合は遷移後のシーンでエリア名表示
                AreaNameShow areaNameShow = GameObject.FindWithTag("AreaName").GetComponent<AreaNameShow>();
                if (areaNameShow) areaNameShow.Show();
            }

            Destroy(gameObject);
        }

        private IEnumerator ChangeBGM()
        {
            // 現在のBGMを取得
            BGM = GameObject.FindWithTag("BGM").GetComponent<AudioSource>();
            // エリアのBGMを取得
            areaBGM = GameObject.FindWithTag("AreaBGM").GetComponent<AreaBGM>();
            print("BGM.clip.name: " + BGM.clip.name);
            print("areaBGM.GetAudioClip().name: " + areaBGM.GetAudioClip().name);
            if (BGM.clip.name == areaBGM.GetAudioClip().name)
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
            print("BGM.clip.name: " + BGM.clip.name);
            print("BGM.volume: " + BGM.volume);
            print("BGM.pitch: " + BGM.pitch);
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
                player.GetComponent<Movement.Mover>().IsLeft = transitionSpawnPoint.isPlayerDirectionLeft;
            }
        }

        private TransitionSpawnPoint GetTransitionSpawnPoint()
        {
            foreach (TransitionSpawnPoint spawnPoint in FindObjectsOfType<TransitionSpawnPoint>())
            {
                if (spawnPoint.spawnPointName != destinationSpawnPointName) continue;
                return spawnPoint;
            }
            return null;
        }
    }    
}
