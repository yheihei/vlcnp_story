using System.Collections;
using Fungus;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using VLCNP.Actions;
using VLCNP.Control;
using VLCNP.Core;
using VLCNP.Movement;
using VLCNP.Saving;
using VLCNP.SceneManagement;
using VLCNP.UI;

namespace VLCNP.Movie
{
    public class LeeleeFukkatu : MonoBehaviour
    {
        PlayableDirector playableDirector;
        [SerializeField]
        Transform startPoint;
        [SerializeField]
        public Flowchart flowChart;
        [SerializeField]
        public AudioClip audioClip;
        [SerializeField]
        public float bgmVolume = 0.3f;
        [SerializeField]
        public float bgmPitch = 0.5f;
        [SerializeField]
        GameObject playerNPC;
        [SerializeField]
        PartyCongroller partyCongroller;

        FlagManager flagManager;

        Vector3 defaultLocalPosition;

        StoppableController stoppableController;
        Fader fader;

        private void Awake() {
            playableDirector = GetComponent<PlayableDirector>();
            flagManager = GameObject.FindWithTag("FlagManager").GetComponent<FlagManager>();
            stoppableController = GetComponent<StoppableController>();
            playerNPC.SetActive(false);
            fader = GameObject.FindWithTag("SceneFader").GetComponent<Fader>();
        }

        private void OnEnable() {
            playableDirector.played += DisableControl;
            playableDirector.stopped += EnableControl;
            playableDirector.stopped += OnStop;
        }

        private void OnDisable() {
            playableDirector.played -= DisableControl;
            playableDirector.stopped -= EnableControl;
        }

        void OnStop(PlayableDirector director) {
            StartCoroutine(Talk());
        }

        void DisableControl(PlayableDirector director) {
            GameObject player = GameObject.FindWithTag("Player");
            player.GetComponent<PlayerController>().enabled = false;
        }

        void EnableControl(PlayableDirector director) {
            GameObject player = GameObject.FindWithTag("Player");
            player.GetComponent<PlayerController>().enabled = true;
        }

        IEnumerator BGMPlay(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            AudioSource audioSource = GameObject.FindWithTag("BGM").GetComponent<AudioSource>();
            if (audioClip != null)
            {
                audioSource.clip = audioClip;
                audioSource.volume = bgmVolume;
                audioSource.pitch = bgmPitch;
                audioSource.Play();
            }
        }

        public void Execute()
        {
            StartCoroutine(ExecuteAsync());
        }

        private IEnumerator ExecuteAsync()
        {
            partyCongroller.SetVisibility(false);
            playerNPC.SetActive(true);
            yield return fader.FadeIn(0.5f);
            playableDirector.Play();
        }

        IEnumerator Talk() {
            stoppableController.StopAll();
            flowChart.ExecuteBlock("Message5");
            yield return new WaitUntil(() => flowChart.GetExecutingBlocks().Count == 0);
            stoppableController.StartAll();
        }

        private void StopAll()
        {
            foreach (MonoBehaviour obj in FindObjectsOfType<MonoBehaviour>())
            {
                IStoppable stoppable = obj.GetComponent<IStoppable>();
                if (stoppable == null) continue;
                stoppable.IsStopped = true;
            }
        }

        private void StartAll()
        {
            foreach (MonoBehaviour obj in FindObjectsOfType<MonoBehaviour>())
            {
                IStoppable stoppable = obj.transform.GetComponent<IStoppable>();
                if (stoppable == null) continue;
                stoppable.IsStopped = false;
            }
        }
    }
}
