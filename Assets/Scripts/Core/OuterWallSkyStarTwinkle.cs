using UnityEngine;

namespace VLCNP.Core
{
    /**
     * 夜空の星(OuterWallSky/Stars)の明るさをゆっくり周期変化させ、明滅しているように見せる環境演出。
     * ゲームの進行には影響しない。会話中も動き続けるよう unscaledTime で進める。
     */
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class OuterWallSkyStarTwinkle : MonoBehaviour
    {
        [SerializeField, Header("最も暗いときの明るさ(基準色に対する割合)"), Range(0f, 1f)]
        private float minBrightness = 0.7f;

        [SerializeField, Header("主周期(秒)")]
        private float period = 6f;

        [SerializeField, Header("重ねる短い周期(秒)。0 以下で無効")]
        private float secondaryPeriod = 2.3f;

        [SerializeField, Header("短い周期の割合"), Range(0f, 1f)]
        private float secondaryWeight = 0.25f;

        private SpriteRenderer spriteRenderer;
        private Color baseColor;
        private float phase;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            baseColor = spriteRenderer.color;
            phase = Random.Range(0f, Mathf.PI * 2f);
        }

        private void OnDisable()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = baseColor;
            }
        }

        private void Update()
        {
            if (period <= 0f) return;

            float t = Time.unscaledTime;
            float wave = (Mathf.Sin(Mathf.PI * 2f * t / period + phase) + 1f) * 0.5f;
            if (secondaryPeriod > 0f)
            {
                float secondary = (Mathf.Sin(Mathf.PI * 2f * t / secondaryPeriod + phase * 1.7f) + 1f) * 0.5f;
                wave = Mathf.Lerp(wave, secondary, secondaryWeight);
            }

            float brightness = Mathf.Lerp(minBrightness, 1f, wave);
            spriteRenderer.color = new Color(
                baseColor.r * brightness,
                baseColor.g * brightness,
                baseColor.b * brightness,
                baseColor.a);
        }
    }
}
