using System.Collections;
using UnityEngine;

namespace VLCNP.Movie
{
    public class ManualFadeObject : MonoBehaviour
    {
        public void FadeIn(float duration = 0.5f)
        {
            StartCoroutine(FadeInCoroutine(duration));
        }

        private IEnumerator FadeInCoroutine(float duration)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            Color color = spriteRenderer.color;
            float startAlpha = color.a;
            float endAlpha = 1f;

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float normalizedTime = t / duration;
                color.a = Mathf.Lerp(startAlpha, endAlpha, normalizedTime);
                spriteRenderer.color = color;
                yield return null;
            }

            color.a = endAlpha;
            spriteRenderer.color = color;
        }

        public void FadeOut(float duration = 0.5f)
        {
            StartCoroutine(FadeOutCoroutine(duration));
        }

        private IEnumerator FadeOutCoroutine(float duration)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            Color color = spriteRenderer.color;
            float startAlpha = color.a;
            float endAlpha = 0f;

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float normalizedTime = t / duration;
                color.a = Mathf.Lerp(startAlpha, endAlpha, normalizedTime);
                spriteRenderer.color = color;
                yield return null;
            }

            color.a = endAlpha;
            spriteRenderer.color = color;
        }
    }
}
