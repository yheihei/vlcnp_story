using System.Collections;
using System.Collections.Generic;
using Fungus;
using UnityEngine;
using UnityEngine.SceneManagement;
using VLCNP.Attributes;
using VLCNP.SceneManagement;

namespace VLCNP.Core
{
    public class GameOver : MonoBehaviour
    {
        Health playerHealth;
        [SerializeField] string autoSaveFileName = "autoSave";
        [SerializeField]
        public Flowchart flowChart;
        [SerializeField] float fadeOutTime = 3f;
        [SerializeField] float fadeWaitTime = 3f;
        [SerializeField] float fadeInTime = 1f;

        private AudioSource BGM;
        private AreaBGM areaBGM;

        void Start()
        {
            // プレイヤーのHealthを取得する
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
            }
        }

        public void Execute()
        {
            flowChart.ExecuteBlock("GameOver");
        }

        public void Retry()
        {
            StartCoroutine(RestartSchene());
        }

        public IEnumerator RestartSchene()
        {
            print("RestoreSchene");
            DontDestroyOnLoad(gameObject);
            Fader fader = GameObject.FindWithTag("SceneFader").GetComponent<Fader>();
            yield return fader.FadeOut(fadeOutTime);
            yield return new WaitForSeconds(fadeWaitTime);
            SavingWrapper wrapper = FindObjectOfType<SavingWrapper>();
            yield return wrapper.Load(autoSaveFileName);
            yield return ChangeBGM();
            yield return fader.FadeIn(fadeInTime);
            Destroy(gameObject);
        }

        public void BackToTitle()
        {
            StartCoroutine(BackToTitleRoutine());
        }

        public IEnumerator BackToTitleRoutine()
        {
            print("BackToTitle");
            DontDestroyOnLoad(gameObject);
            Fader fader = GameObject.FindWithTag("SceneFader").GetComponent<Fader>();
            yield return fader.FadeOut(fadeOutTime);
            yield return new WaitForSeconds(fadeWaitTime);
            // Schene0をロード
            yield return SceneManager.LoadSceneAsync(0);

            yield return ChangeBGM();
            yield return fader.FadeIn(fadeInTime);
            Destroy(gameObject);
        }

        // public void SetPlayerHealth(Health newPlayerHealth)
        // {
        //     if (playerHealth == null) return;
        //     playerHealth.onDie -= PlayerDie;
        //     playerHealth = newPlayerHealth;
        //     playerHealth.onDie += PlayerDie;
        // }

        private IEnumerator ChangeBGM()
        {
            // 現在のBGMを取得
            BGM = GameObject.FindWithTag("BGM").GetComponent<AudioSource>();
            // エリアのBGMを取得
            areaBGM = GameObject.FindWithTag("AreaBGM").GetComponent<AreaBGM>();
            // areaBGMがなければBGMをStop
            if (areaBGM.GetAudioClip() == null)
            {
                yield return BGMFadeRoutine(0, fadeWaitTime);
                BGM.Stop();
                yield break;
            }
            yield return BGMFadeRoutine(0, fadeWaitTime);
            BGM.Stop();
            BGM.clip = areaBGM.GetAudioClip();
            BGM.volume = areaBGM.GetVolume();
            BGM.pitch = areaBGM.GetPitch();
            print("Play");
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
    }    
}
