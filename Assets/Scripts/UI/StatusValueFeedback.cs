using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace VLCNP.UI
{
    /** 数値が変化した瞬間だけ、HUDに短い色の変化と小さな動きを付ける。 */
    public class StatusValueFeedback : MonoBehaviour
    {
        [SerializeField] private Graphic graphic;
        [SerializeField] private RectTransform motionRoot;
        [SerializeField] private Color increaseColor = new Color(0.75f, 0.9f, 0.65f);
        [SerializeField] private Color decreaseColor = new Color(1f, 0.55f, 0.5f);
        [SerializeField, Min(0.01f)] private float duration = 0.18f;
        [SerializeField, Min(0f)] private float movement = 4f;

        private Coroutine routine;
        private Color restingColor;
        private Vector2 restingPosition;
        private bool hasRestingState;

        public void Play(bool increased)
        {
            if (!isActiveAndEnabled || graphic == null)
                return;

            Cancel();
            restingColor = graphic.color;
            if (motionRoot != null)
                restingPosition = motionRoot.anchoredPosition;
            hasRestingState = true;
            routine = StartCoroutine(Pulse(increased ? increaseColor : decreaseColor));
        }

        public void Cancel()
        {
            if (routine != null)
                StopCoroutine(routine);
            routine = null;
            Restore();
        }

        private IEnumerator Pulse(Color flashColor)
        {
            float elapsed = 0;
            float pulseDuration = Mathf.Max(0.01f, duration);
            while (elapsed < pulseDuration)
            {
                float progress = elapsed / pulseDuration;
                graphic.color = Color.Lerp(flashColor, restingColor, progress);
                if (motionRoot != null)
                {
                    // 拡大・縮小せず、整数の座標差だけで動かしてドットの形を保つ。
                    float offset = Mathf.Round(movement * Mathf.Sin(progress * Mathf.PI));
                    motionRoot.anchoredPosition = restingPosition + Vector2.up * offset;
                }
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            Restore();
            routine = null;
        }

        private void Restore()
        {
            if (!hasRestingState)
                return;
            if (graphic != null)
                graphic.color = restingColor;
            if (motionRoot != null)
                motionRoot.anchoredPosition = restingPosition;
            hasRestingState = false;
        }

        private void OnDisable()
        {
            Cancel();
        }
    }
}
