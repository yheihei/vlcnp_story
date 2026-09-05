using System.Collections;
using UnityEngine;

namespace VLCNP.Movie
{
    /**
     * カットイン。顔グラと帯をまとめた content を右から中央へ走らせ、一拍止めて左へ抜ける。
     * 入りは中央を少し通り過ぎてから戻す(オーバーシュート)。戻りの時間はホールドから差し引くので、
     * 抜けの開始時刻は slideInDuration + holdDuration のまま変わらない。
     * アルファのフェードは使わない(必殺の一撃の鋭さを優先)。
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

        // 入りで中央を通り過ぎる量と、中央へ戻す時間。戻しはホールドの時間内で行う
        [SerializeField]
        float overshootDistance = 50f;

        [SerializeField]
        float settleDuration = 0.08f;

        // 画面外とみなす横方向の移動量(キャンバス座標)。
        // 帯(幅3400、-7度回転)の半幅は約1720なので、画面半幅960を足した2680以上ないと端がポップイン/アウトする
        [SerializeField]
        float travelDistance = 3000f;

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
            Vector2 overshoot = new Vector2(-overshootDistance, 0);
            yield return Slide(new Vector2(travelDistance, 0), overshoot, slideInDuration, easeOut: true);
            yield return Slide(overshoot, Vector2.zero, settleDuration, easeOut: true);
            yield return new WaitForSeconds(Mathf.Max(0, holdDuration - settleDuration));
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
