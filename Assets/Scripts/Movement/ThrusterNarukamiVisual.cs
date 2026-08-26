using System.Collections;
using UnityEngine;

namespace VLCNP.Movement
{
    /**
     * スラスター噴射中にプレイヤー頭上へ表示するナルカミの見た目制御。
     * Show/Hide でスプライトをフェードイン・フェードアウトする。
     */
    [RequireComponent(typeof(SpriteRenderer))]
    public class ThrusterNarukamiVisual : MonoBehaviour
    {
        [SerializeField, Min(0f)]
        float fadeSeconds = 0.15f;

        SpriteRenderer spriteRenderer;
        Coroutine fadeCoroutine;
        float targetAlpha = 0f;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            SetAlpha(0f);
        }

        public void Show()
        {
            FadeTo(1f);
        }

        public void Hide()
        {
            FadeTo(0f);
        }

        private void FadeTo(float alpha)
        {
            // すでに目標値でフェード中でもなければ何もしない(毎フレームのHide呼び出し対策)
            if (Mathf.Approximately(targetAlpha, alpha) && fadeCoroutine == null)
                return;

            targetAlpha = alpha;
            if (!isActiveAndEnabled)
            {
                SetAlpha(alpha);
                return;
            }
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(Fade(alpha));
        }

        private IEnumerator Fade(float to)
        {
            if (fadeSeconds <= 0f)
            {
                SetAlpha(to);
                yield break;
            }
            float from = spriteRenderer.color.a;
            float elapsed = 0f;
            while (elapsed < fadeSeconds)
            {
                elapsed += Time.deltaTime;
                SetAlpha(Mathf.Lerp(from, to, elapsed / fadeSeconds));
                yield return null;
            }
            SetAlpha(to);
            fadeCoroutine = null;
        }

        // 非アクティブ化(キャラ切替等)時はフェードを中断して最終状態を即時反映
        private void OnDisable()
        {
            SetAlpha(targetAlpha);
        }

        private void SetAlpha(float alpha)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}
