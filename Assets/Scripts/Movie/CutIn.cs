using System.Collections;
using UnityEngine;

namespace VLCNP.Movie
{
    /**
     * カットイン。顔グラと帯をまとめた content を右から中央へ走らせ、一拍止めて左へ抜ける。
     * Fungus の InvokeMethod から Play() を呼ぶ。content は再生中だけ表示する。
     */
    public class CutIn : MonoBehaviour
    {
        [SerializeField]
        RectTransform content = null;

        [SerializeField]
        float slideInDuration = 0.15f;

        [SerializeField]
        float holdDuration = 0.6f;

        [SerializeField]
        float slideOutDuration = 0.2f;

        // 画面外とみなす横方向の移動量(キャンバス座標)
        [SerializeField]
        float travelDistance = 2600f;

        [SerializeField]
        AudioClip se = null;

        [SerializeField]
        float seVolume = 0.7f;

        void Awake()
        {
            if (content != null) content.gameObject.SetActive(false);
        }

        public void Play()
        {
            if (content == null)
            {
                Debug.LogWarning("[CutIn] content が設定されていません。");
                return;
            }
            StopAllCoroutines();
            StartCoroutine(PlayCoroutine());
        }

        IEnumerator PlayCoroutine()
        {
            content.gameObject.SetActive(true);
            if (se != null)
            {
                Vector3 at = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
                AudioSource.PlayClipAtPoint(se, at, seVolume);
            }
            yield return Slide(new Vector2(travelDistance, 0), Vector2.zero, slideInDuration, easeOut: true);
            yield return new WaitForSeconds(holdDuration);
            yield return Slide(Vector2.zero, new Vector2(-travelDistance, 0), slideOutDuration, easeOut: false);
            content.gameObject.SetActive(false);
        }

        IEnumerator Slide(Vector2 from, Vector2 to, float duration, bool easeOut)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / duration);
                // 入りは減速、抜けは加速
                k = easeOut ? 1 - (1 - k) * (1 - k) * (1 - k) : k * k * k;
                content.anchoredPosition = Vector2.LerpUnclamped(from, to, k);
                yield return null;
            }
            content.anchoredPosition = to;
        }
    }
}
