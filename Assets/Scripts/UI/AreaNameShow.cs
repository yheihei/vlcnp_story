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

        public void Show(string areaName = null)
        {
            if (areaName != null)
            {
                Text text = GetComponentInChildren<Text>();
                text.text = areaName;
            }
            playableDirector.Play();
            // StartCoroutine(ShowAreaNameCoroutine());
        }

        // IEnumerator ShowAreaNameCoroutine()
        // {
        //     jingle.PlayOneShot(jingle.clip);
        //     Fader fader = GetComponent<Fader>();
        //     yield return fader.FadeIn(1f);
        //     yield return new WaitForSeconds(3f);
        //     yield return fader.FadeOut(1f);
        // }

    }
}
