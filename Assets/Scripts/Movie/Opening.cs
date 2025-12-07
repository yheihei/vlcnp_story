using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Fungus;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using VLCNP.Actions;
using VLCNP.Control;
using VLCNP.Core;
using VLCNP.Saving;
using VLCNP.UI;

namespace VLCNP.Movie
{
    public class Opening : MonoBehaviour
    {
        PlayableDirector playableDirector;
        Transform startPoint;
        GameObject tutorial;
        [SerializeField]
        public Flowchart flowChart;
        [SerializeField]
        public AudioClip audioClip;
        [SerializeField]
        public float bgmVolume = 0.3f;
        [SerializeField]
        public float bgmPitch = 0.5f;

        // bool isDone = false;
        FlagManager flagManager;

        private void Awake() {
            playableDirector = GetComponent<PlayableDirector>();
            tutorial = GameObject.Find("Tutorial");
            flagManager = GameObject.FindWithTag("FlagManager").GetComponent<FlagManager>();
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
            if (flagManager.GetFlag(Flag.OpeningDone)) return;
            StopAll();
            GameObject player = GameObject.FindWithTag("Player");
            player.GetComponent<PlayerController>().enabled = false;
            Debug.Log($"Opening.DisableControl: disabled PlayerController at t={Time.time:F3}");
            // チュートリアル取得して、disableにする
            tutorial.SetActive(false);
        }

        void EnableControl(PlayableDirector director) {
            if (flagManager.GetFlag(Flag.OpeningDone)) return;
            StartAll();
            GameObject player = GameObject.FindWithTag("Player");
            // Akim、はてなマーク
            // StartCoroutine(Talk());
            player.GetComponent<PlayerController>().enabled = true;
            Debug.Log($"Opening.EnableControl: enabled PlayerController at t={Time.time:F3}");
            // プレイヤーを開始位置に移動
            player.transform.position = startPoint.position;
            // CMCameraを取得して、プレイヤーを追従させる
            GameObject.FindWithTag("CMCamera").GetComponent<CinemachineVirtualCamera>().Follow = player.transform;
            // AreaNameをタグで取得し、表示
            AreaNameShow areaNameShow = GameObject.FindWithTag("AreaName").GetComponent<AreaNameShow>();
            areaNameShow.Show();
            // チュートリアルを有効にする
            tutorial.SetActive(true);
            StartCoroutine(BGMPlay(4f));
            flagManager.SetFlag(Flag.OpeningDone, true);
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

        void Start()
        {
            startPoint = GameObject.Find("StartPoint").transform;
            if (flagManager.GetFlag(Flag.OpeningDone)) return;
            playableDirector.Play();
        }

        IEnumerator Talk() {
            StopAll();
            flowChart.ExecuteBlock("AkimQuestion");
            yield return new WaitUntil(() => flowChart.GetExecutingBlocks().Count == 0);
            StartAll();
        }

        private void StopAll()
        {
            float now = Time.time;
            int stoppedCount = 0;
            var seen = new HashSet<IStoppable>();
            // 非アクティブ／子オブジェクトも含めてユニークな IStoppable を停止
            foreach (MonoBehaviour obj in FindObjectsOfType<MonoBehaviour>(true))
            {
                foreach (IStoppable stoppable in obj.GetComponentsInChildren<IStoppable>(true))
                {
                    if (!seen.Add(stoppable)) continue;
                    stoppable.IsStopped = true;
                    stoppedCount++;
                }
            }
            Debug.Log($"Opening.StopAll: stopped {stoppedCount} unique IStoppable components at t={now:F3}");
        }

        private void StartAll()
        {
            float now = Time.time;
            int restartedCount = 0;
            var seen = new HashSet<IStoppable>();
            // 非アクティブ／子オブジェクトも含めてユニークな IStoppable を再開
            foreach (MonoBehaviour obj in FindObjectsOfType<MonoBehaviour>(true))
            {
                foreach (IStoppable stoppable in obj.GetComponentsInChildren<IStoppable>(true))
                {
                    if (!seen.Add(stoppable)) continue;
                    stoppable.IsStopped = false;
                    restartedCount++;
                }
            }
            Debug.Log($"Opening.StartAll: resumed {restartedCount} unique IStoppable components at t={now:F3}");
        }

        // public JToken CaptureAsJToken()
        // {
        //     return JToken.FromObject(isDone);
        // }

        // public void RestoreFromJToken(JToken state)
        // {
        //     isDone = state.ToObject<bool>();
        // }
    }
}
