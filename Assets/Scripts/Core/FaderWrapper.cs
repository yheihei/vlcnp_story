using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.SceneManagement;

namespace VLCNP.Core
{
    public class FaderWrapper : MonoBehaviour
    {
        Fader fader = null;

        void Start()
        {
            // SceneFaderのtagを持つオブジェクトを探して、そのFaderを取得する
            fader = GameObject.FindWithTag("SceneFader")?.GetComponent<Fader>();
        }

        public void FadeOut(float time)
        {
            print("FadeOut");
            print(fader);
            if (fader == null) return;
            StartCoroutine(FadeOutRoutine(time));
        }

        IEnumerator FadeOutRoutine(float time)
        {
            Debug.Log($"Fade out started. time:{time}");
            yield return fader.FadeOut(time);
            Debug.Log("Fade out completed.");
        }

        public void FadeIn(float time)
        {
            print("FadeIn");
            print(fader);
            if (fader == null) return;
            StartCoroutine(FadeInRoutine(time));
        }

        IEnumerator FadeInRoutine(float time)
        {
            yield return fader.FadeIn(time);
        }
    }
}
