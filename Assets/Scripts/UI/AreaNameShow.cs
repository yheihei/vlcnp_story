using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
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
            if (areaName != "")
            {
                Text text = GetComponentInChildren<Text>();
                text.text = areaName;
            }
            playableDirector.Play();
        }

    }
}
