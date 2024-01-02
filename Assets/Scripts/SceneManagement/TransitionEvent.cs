using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using VLCNP.Control;
using VLCNP.Core;

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

            yield return new WaitForSeconds(fadeWaitTime);

            yield return SceneManager.LoadSceneAsync(sceneToLoad);
            print("scene load end");

            // BGMの変更があれば変更
            StartCoroutine(ChangeBGM());
            // キャラたちの状態復元
            // 遷移後 こちらのシーンでのSaving wrapperを再取得
            savingWrapper = FindObjectOfType<SavingWrapper>();
            savingWrapper.LoadOnlyState(autoSaveFileName);

            TransitionSpawnPoint transitionSpawnPoint = GetTransitionSpawnPoint() ?? throw new System.Exception("Transition spawn point not found");
            print("transition spawn point found");
            UpdatePlayerPosition(transitionSpawnPoint);

            fader = GameObject.FindWithTag("SceneFader").GetComponent<Fader>();
            print("fade in start");
            yield return fader.FadeIn(fadeInTime);
            print("fade in end");

            Destroy(gameObject);
        }

        private IEnumerator ChangeBGM()
        {
            // 現在のBGMを取得
            BGM = GameObject.FindWithTag("BGM").GetComponent<AudioSource>();
            // エリアのBGMを取得
            areaBGM = GameObject.FindWithTag("AreaBGM").GetComponent<AreaBGM>();
            if (BGM.clip.name == areaBGM.GetAudioClip().name)
            {
                yield break;
            }
            // clipの変更があれば変更
            yield return StartCoroutine(BGMFadeRoutine(0, fadeWaitTime));
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
            print("UpdatePlayerPosition@@@@@@@");
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = transitionSpawnPoint.transform.position;
            // 向きを変える
            player.GetComponent<Movement.Mover>().IsLeft = transitionSpawnPoint.isPlayerDirectionLeft;
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
