using System.Collections;
using UnityEngine;

namespace VLCNP.Movie
{
    public class FadeInSprite : MonoBehaviour
    {
        public float fadeDuration = 0.5f; // フェードインにかかる時間
        private SpriteRenderer spriteRenderer;
        private Color originalColor;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalColor = spriteRenderer.color;
            originalColor.a = 0f;
            spriteRenderer.color = originalColor;
        }

        void OnEnable()
        {
            StartCoroutine(FadeIn());
        }

        private IEnumerator FadeIn()
        {
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                Color newColor = originalColor;
                newColor.a = alpha;
                spriteRenderer.color = newColor;
                yield return null;
            }

            // 最後に完全にフェードインした状態にする
            originalColor.a = 1f;
            spriteRenderer.color = originalColor;
        }
    }
}