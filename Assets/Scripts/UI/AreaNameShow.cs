using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using VLCNP.Core;
using VLCNP.SceneManagement;

namespace VLCNP.UI
{
    public class AreaNameShow : MonoBehaviour
    {
        PlayableDirector playableDirector;

        private void Awake() {
            playableDirector = GetComponent<PlayableDirector>();
        }

        public void Show(string areaName = "")
        {
            Text text = GetComponentInChildren<Text>();
            if (areaName != "")
            {
                text.text = areaName;
            }
            // バナー表示 = エリア到達として計測する(同一エリア内の部屋移動ではバナーを出さない)
            VLCNPAnalytics.RecordAreaEntered(text != null ? text.text : areaName);
            playableDirector.Play();
        }

    }
}
