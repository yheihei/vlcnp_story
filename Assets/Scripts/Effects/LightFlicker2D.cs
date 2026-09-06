using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VLCNP.Effects
{
    /**
     * 松明・ランプ用の Point Light 2D の強さをパーリンノイズでゆらす環境演出。
     * ゲームの進行には影響しない。会話中も揺れ続けるよう unscaledTime で進める。
     */
    [RequireComponent(typeof(Light2D))]
    public class LightFlicker2D : MonoBehaviour
    {
        [SerializeField, Header("ゆらぎの幅(基準強さに対する割合)"), Range(0f, 1f)]
        private float amplitude = 0.18f;

        [SerializeField, Header("ゆらぎの速さ")]
        private float speed = 5f;

        private Light2D light2D;
        private float baseIntensity;
        private float seed;

        private void Awake()
        {
            light2D = GetComponent<Light2D>();
            baseIntensity = light2D.intensity;
            seed = Random.Range(0f, 1000f);
        }

        private void OnEnable()
        {
            if (light2D != null)
            {
                light2D.intensity = baseIntensity;
            }
        }

        private void OnDisable()
        {
            if (light2D != null)
            {
                light2D.intensity = baseIntensity;
            }
        }

        private void Update()
        {
            float t = Time.unscaledTime * speed;
            // 2 つの周期を重ねて、単調な明滅に見えないようにする
            float noise = Mathf.PerlinNoise(seed + t, seed) * 0.7f + Mathf.PerlinNoise(seed, seed + t * 2.3f) * 0.3f;
            float factor = 1f + (noise - 0.5f) * 2f * amplitude;
            light2D.intensity = baseIntensity * factor;
        }
    }
}
