using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using VLCNP.SceneManagement;

namespace VLCNP.SceneManagement
{
    [RequireComponent(typeof(ChangeAreaBGM))]
    public class NewGame : MonoBehaviour
    {
        [SerializeField] float fadeOutTime = 1f;
        [SerializeField] float fadeWaitTime = 3f;
        [SerializeField] float fadeInTime = 5f;

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

            // Schene1をロード
            yield return SceneManager.LoadSceneAsync(1);

            ChangeAreaBGM changeBGM = GetComponent<ChangeAreaBGM>();
            changeBGM.Execute();

            // Faderを再取得してフェードイン
            fader = GameObject.FindWithTag("SceneFader").GetComponent<Fader>();
            yield return fader.FadeIn(fadeInTime);
            Destroy(gameObject);
        }
    }    
}
