using System;
using System.Collections;
using Unity.VisualScripting;
using VLCNP.Control;
using UnityEngine;
using UnityEngine.SceneManagement;
using VLCNP.Core;
using UnityEngine.Events;

namespace VLCNP.SceneManagement
{
    // TODO: TransitionEventの方を使うこと
    public class Portal : MonoBehaviour
    {
        enum DestinationIdentifier
        {
            A, B, C, D, E
        }

        [SerializeField] int sceneToLoad = -1;
        [SerializeField] Transform spawnPoint;
        [SerializeField] bool isPlayerDirectionLeft;
        [SerializeField] DestinationIdentifier destination;
        [SerializeField] float fadeOutTime = 1f;
        [SerializeField] float fadeWaitTime = 0.2f;
        [SerializeField] float fadeInTime = 1f;
        [SerializeField] string autoSaveFileName = "autoSave";
        [SerializeField] bool isAutoSave = true;
        [SerializeField] GameObject[] dontDestroyOnLoadObjects;

        private AudioSource BGM;
        private AreaBGM areaBGM;

        bool isTransitioning = false;

        // private void OnTriggerEnter2D(Collider2D other)
        // {
        //     if (isTransitioning) return;
        //     if (other.gameObject.tag == "Player")
        //     {
        //         print("Portal");
        //         isTransitioning = true;
        //         StartCoroutine(Transition());
        //     }
        // }

        public void TransitionEvent()
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
            foreach (GameObject obj in dontDestroyOnLoadObjects)
            {
                DontDestroyOnLoad(obj);
            }
            DisableControl();

            // SceneFaderタグでFaderを取得
            Fader fader = GameObject.FindWithTag("SceneFader").GetComponent<Fader>();
            yield return fader.FadeOut(fadeOutTime);

            // キャラたちの状態保存
            SavingWrapper savingWrapper = FindObjectOfType<SavingWrapper>();
            if (isAutoSave)
            {
                savingWrapper.Save(autoSaveFileName);
            }

            yield return SceneManager.LoadSceneAsync(sceneToLoad);

            // 遷移後 こちらのシーンでのSaving wrapperを再取得
            savingWrapper = FindObjectOfType<SavingWrapper>();
            // StoppableControllerをタグから取得
            StoppableController stoppableController = GameObject.FindWithTag("StoppableController").GetComponent<StoppableController>();
            stoppableController?.StopAll();
            // BGMの変更があれば変更
            StartCoroutine(ChangeBGM());
            // キャラたちの状態復元
            savingWrapper.LoadOnlyState(autoSaveFileName);

            Portal otherPortal = GetOtherPortal();
            UpdatePlayerPosition(otherPortal);

            DisableControl();

            yield return new WaitForSeconds(fadeWaitTime);
            yield return fader.FadeIn(fadeInTime);

            EnableControl();
            stoppableController?.StartAll();

            Destroy(gameObject);
            foreach (GameObject obj in dontDestroyOnLoadObjects)
            {
                Destroy(obj);
            }
        }

        private IEnumerator ChangeBGM()
        {
            // 現在のBGMを取得
            BGM = GameObject.FindWithTag("BGM").GetComponent<AudioSource>();
            // エリアのBGMを取得
            areaBGM = GameObject.FindWithTag("AreaBGM").GetComponent<AreaBGM>();
            print($"BGM: {BGM.clip.name}, areaBGM: {areaBGM}");
            // areaBGMがなければBGMをStop
            if (areaBGM.GetAudioClip() == null)
            {
                yield return StartCoroutine(BGMFadeRoutine(0, fadeWaitTime));
                BGM.Stop();
                yield break;
            }
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

        private void UpdatePlayerPosition(Portal otherPortal)
        {
            print("UpdatePlayerPosition");
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = otherPortal.spawnPoint.position;
            // 向きを変える
            player.GetComponent<Movement.Mover>().IsLeft = otherPortal.isPlayerDirectionLeft;
        }

        void DisableControl()
        {
            GameObject player = GameObject.FindWithTag("Player");
            player.GetComponent<PlayerController>().enabled = false;
            player.GetComponent<Movement.Mover>().Stop();
        }

        void EnableControl()
        {
            GameObject player = GameObject.FindWithTag("Player");
            player.GetComponent<PlayerController>().enabled = true;
        }

        private Portal GetOtherPortal()
        {
            foreach (Portal portal in FindObjectsOfType<Portal>())
            {
                if (portal == this) continue;
                if (portal.destination != destination) continue;
                return portal;
            }
            return null;
        }
    }    
}
