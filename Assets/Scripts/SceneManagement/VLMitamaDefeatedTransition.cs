using System.Collections;
using Fungus;
using UnityEngine;
using VLCNP.Control;
using VLCNP.Core;

namespace VLCNP.SceneManagement
{
    public class VLMitamaDefeatedTransition : MonoBehaviour
    {
        [SerializeField]
        TransitionEvent transitionEvent;

        [SerializeField]
        float slowTimeScale = 0.2f;

        [SerializeField]
        float whiteFadeDuration = 1f;

        [SerializeField]
        float bgmFadeOutDuration = 1f;

        bool isRunning;

        public void Execute(GameObject defeatedBoss)
        {
            if (isRunning)
                return;

            StartCoroutine(ExecuteRoutine());
        }

        IEnumerator ExecuteRoutine()
        {
            isRunning = true;

            PartyCongroller.FindInScene()?.SetTempInvincible(true);
            Time.timeScale = slowTimeScale;

            BGMWrapper bgmWrapper = FindObjectOfType<BGMWrapper>();
            bgmWrapper?.FadeOut(bgmFadeOutDuration);

            yield return FadeScreenToWhite();

            Time.timeScale = 1f;

            if (transitionEvent == null)
                transitionEvent = GetComponent<TransitionEvent>();

            if (transitionEvent == null)
            {
                Debug.LogError("VLMitamaDefeatedTransition requires a TransitionEvent.");
                isRunning = false;
                yield break;
            }

            transitionEvent.ExecuteTransition();
        }

        IEnumerator FadeScreenToWhite()
        {
            CameraManager cameraManager = FungusManager.Instance.CameraManager;
            cameraManager.ScreenFadeTexture = CameraManager.CreateColorTexture(Color.white, 32, 32);

            bool finished = false;
            cameraManager.Fade(
                1f,
                whiteFadeDuration,
                () => finished = true,
                LeanTweenType.easeInOutQuad
            );

            while (!finished)
            {
                yield return null;
            }
        }
    }
}
