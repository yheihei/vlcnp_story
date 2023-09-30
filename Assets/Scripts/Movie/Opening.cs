using System.Collections;
using Fungus;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using VLCNP.Actions;
using VLCNP.Control;
using VLCNP.Core;
using VLCNP.Saving;

namespace VLCNP.Movie
{
    public class Opening : MonoBehaviour, IJsonSaveable
    {
        PlayableDirector playableDirector;
        Transform startPoint;
        GameObject tutorial;
        [SerializeField]
        public Flowchart flowChart;

        bool isDone = false;

        private void Awake() {
            playableDirector = GetComponent<PlayableDirector>();
            tutorial = GameObject.Find("Tutorial");
        }

        private void OnEnable() {
            playableDirector.played += DisableControl;
            playableDirector.stopped += EnableControl;
        }

        private void OnDisable() {
            playableDirector.played -= DisableControl;
            playableDirector.stopped -= EnableControl;
        }

        void DisableControl(PlayableDirector director) {
            GameObject player = GameObject.FindWithTag("Player");
            player.GetComponent<PlayerController>().enabled = false;
            // チュートリアル取得して、disableにする
            tutorial.SetActive(false);
        }

        void EnableControl(PlayableDirector director) {
            GameObject player = GameObject.FindWithTag("Player");
            // Akim、はてなマーク
            StartCoroutine(Talk());
            player.GetComponent<PlayerController>().enabled = true;
            // プレイヤーを開始位置に移動
            player.transform.position = startPoint.position;
        }

        void Start()
        {
            startPoint = GameObject.Find("StartPoint").transform;
            if (isDone) return;
            playableDirector.Play();
            isDone = true;
        }

        IEnumerator Talk() {
            StopAll();
            flowChart.ExecuteBlock("AkimQuestion");
            yield return new WaitUntil(() => flowChart.GetExecutingBlocks().Count == 0);
            StartAll();
            // チュートリアルを有効にする
            tutorial.SetActive(true);
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

        public JToken CaptureAsJToken()
        {
            return isDone;
        }

        public void RestoreFromJToken(JToken state)
        {
            isDone = (bool) state;
        }
    }
}
