using System.Collections;
using UnityEngine;

namespace VLCNP.Movie
{
    /**
     * カットイン。顔と帯は定位置に保ち、中央から上下へ開き、上下から中央へ閉じる。
     * 顔は帯とは別のマスクで開閉し、全開時には耳が帯の上にはみ出す。
     * Fungus の InvokeMethod から Play() を呼ぶ。content は再生中だけ表示する。
     */
    public class CutIn : MonoBehaviour
    {
        [SerializeField]
        RectTransform content = null;

        [SerializeField]
        RectTransform bandWipe = null;

        [SerializeField]
        RectTransform faceWipe = null;

        [SerializeField]
        RectTransform face = null;

        [SerializeField]
        RectTransform lineTop = null;

        [SerializeField]
        RectTransform lineBottom = null;

        [SerializeField, Min(0)]
        float wipeInDuration = 0.14f;

        [SerializeField, Min(0)]
        float holdDuration = 0.69f;

        [SerializeField, Min(0)]
        float wipeOutDuration = 0.12f;

        [SerializeField]
        AudioClip se = null;

        [SerializeField]
        float seVolume = 0.7f;

        float bandHeight;
        float faceMaskHeight;
        Vector2 faceMaskOffset;
        Vector2 faceOffset;
        Vector2 facePosition;
        Vector2 topPosition;
        Vector2 bottomPosition;

        void Awake()
        {
            if (bandWipe != null) bandHeight = bandWipe.sizeDelta.y;
            if (faceWipe != null && bandWipe != null)
            {
                faceMaskHeight = faceWipe.sizeDelta.y;
                faceMaskOffset = faceWipe.anchoredPosition - bandWipe.anchoredPosition;
                faceOffset = Quaternion.Inverse(faceWipe.localRotation) * (Vector3)faceMaskOffset;
            }
            if (face != null) facePosition = face.anchoredPosition;
            if (lineTop != null) topPosition = lineTop.anchoredPosition;
            if (lineBottom != null) bottomPosition = lineBottom.anchoredPosition;
            if (content != null) content.gameObject.SetActive(false);
        }

        public void Play()
        {
            if (content == null || bandWipe == null || faceWipe == null || face == null
                || lineTop == null || lineBottom == null)
            {
                Debug.LogWarning("[CutIn] ワイプの参照が設定されていません。");
                return;
            }
            StopAllCoroutines();
            StartCoroutine(PlayCoroutine());
        }

        IEnumerator PlayCoroutine()
        {
            SetReveal(0);
            content.gameObject.SetActive(true);
            if (se != null)
            {
                Vector3 at = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
                AudioSource.PlayClipAtPoint(se, at, seVolume);
            }
            // フェーズごとに待機を積み上げず、着弾に合わせた全体の時間で進める。
            float openEnd = Mathf.Max(0, wipeInDuration);
            float closeStart = openEnd + Mathf.Max(0, holdDuration);
            float end = closeStart + Mathf.Max(0, wipeOutDuration);
            for (float t = 0; t < end; t += Time.deltaTime)
            {
                float reveal = 1;
                if (t < openEnd)
                {
                    float k = t / openEnd;
                    reveal = 1 - Mathf.Pow(1 - k, 3);
                }
                else if (t >= closeStart)
                {
                    float k = (t - closeStart) / (end - closeStart);
                    reveal = 1 - k * k * k;
                }
                SetReveal(reveal);
                yield return null;
            }
            SetReveal(0);
            content.gameObject.SetActive(false);
        }

        void SetReveal(float progress)
        {
            progress = Mathf.Clamp01(progress);
            bandWipe.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bandHeight * progress);
            faceWipe.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, faceMaskHeight * progress);
            // 顔マスクの下端を帯の下端に揃え、上側だけ耳の分まで広げる。
            faceWipe.anchoredPosition = bandWipe.anchoredPosition + faceMaskOffset * progress;
            // マスクは斜めなので、親座標での移動量を顔の座標へ戻して相殺する。
            face.anchoredPosition = facePosition + faceOffset * (1 - progress);
            // ラインは太さを保ったままワイプの境界に沿わせる。
            float hiddenHalf = bandHeight * (1 - progress) * 0.5f;
            lineTop.anchoredPosition = topPosition - Vector2.up * hiddenHalf;
            lineBottom.anchoredPosition = bottomPosition + Vector2.up * hiddenHalf;
        }

        void OnDisable()
        {
            StopAllCoroutines();
            if (content != null) content.gameObject.SetActive(false);
        }
    }
}
