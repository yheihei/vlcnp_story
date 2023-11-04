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

        FlagManager flagManager;

        Vector3 defaultLocalPosition;

        StoppableController stoppableController;

        private void Awake() {
            playableDirector = GetComponent<PlayableDirector>();
            flagManager = GameObject.FindWithTag("FlagManager").GetComponent<FlagManager>();
            stoppableController = GetComponent<StoppableController>();
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
            // Playerの位置をstartPointに戻す
            player.transform.localPosition = defaultLocalPosition;
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

        public async void Execute()
        {
            // プレイヤーをイベント開始位置まで移動させる
            GameObject player = GameObject.FindWithTag("Player");
            Mover mover = player.GetComponent<Mover>();
            await mover.MoveToPosition(startPoint.position);
            defaultLocalPosition = player.transform.localPosition;
            playableDirector.Play();
        }

        IEnumerator Talk() {
            // StopAll();
            stoppableController.StopAll();
            flowChart.ExecuteBlock("Message5");
            yield return new WaitUntil(() => flowChart.GetExecutingBlocks().Count == 0);
            stoppableController.StartAll();
            // StartAll();
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
