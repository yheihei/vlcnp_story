using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using VLCNP.SceneManagement;

namespace VLCNP.SceneManagement
{
    [RequireComponent(typeof(ChangeAreaBGM))]
    public class ContinueGame : MonoBehaviour
    {
        [SerializeField] float fadeOutTime = 1f;
        [SerializeField] float fadeWaitTime = 3f;
        [SerializeField] float fadeInTime = 1f;

        public void Execute()
        {
            DontDestroyOnLoad(gameObject);
            StartCoroutine(GameStart());
        }

        public IEnumerator GameStart()
        {
            // SceneFaderタグでFaderを取得
            Fader fader = GameObject.FindWithTag("SceneFader").GetComponent<Fader>();
            yield return fader.FadeOut(fadeOutTime);

            // fadeWaitTime分待つ
            yield return new WaitForSeconds(fadeWaitTime);

            // SavingWrapperを取得し、save.jsonをロード
            SavingWrapper savingWrapper = FindObjectOfType<SavingWrapper>();
            yield return savingWrapper.Load();

            ChangeAreaBGM changeBGM = GetComponent<ChangeAreaBGM>();
            changeBGM.Execute();

            // Faderを再取得してフェードイン
            fader = GameObject.FindWithTag("SceneFader").GetComponent<Fader>();
            yield return fader.FadeIn(fadeInTime);
            Destroy(gameObject);
        }
    }    
}
