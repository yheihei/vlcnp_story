using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Effects
{
    /**
     * 水面スプライトを上下に微かに揺らし、明るさをゆっくり周期変化させる環境演出。
     * 子の SpriteRenderer(位置と色)だけを動かし、当たり判定(WaterUnder 側)には触れない。
     * 会話中も揺れ続けるよう unscaledTime で進める。無効化したら元の位置と色に戻す。
     */
    public class WaterSurfaceSway : MonoBehaviour
    {
        [SerializeField, Header("上下の振れ幅(ワールド単位。1/32 で 1 ピクセル)")]
        private float amplitude = 1f / 32f;

        [SerializeField, Header("上下の周期(秒)")]
        private float period = 2.4f;

        [SerializeField, Header("明るさの振れ幅(0 で無効)"), Range(0f, 0.5f)]
        private float brightnessAmplitude = 0.06f;

        [SerializeField, Header("明るさの周期(秒)")]
        private float brightnessPeriod = 3.7f;

        [SerializeField, Header("隣り合う子ごとの位相のずれ(ラジアン)")]
        private float phaseStep = 1.1f;

        private struct Target
        {
            public Transform transform;
            public SpriteRenderer renderer;
            public Vector3 basePosition;
            public Color baseColor;
            public float phase;
        }

        private readonly List<Target> targets = new List<Target>();
        private float seed;

        private void Awake()
        {
            seed = Random.Range(0f, 100f);
            Collect();
        }

        private void Collect()
        {
            targets.Clear();
            var renderers = GetComponentsInChildren<SpriteRenderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer.transform == transform) continue;
                targets.Add(new Target
                {
                    transform = renderer.transform,
                    renderer = renderer,
                    basePosition = renderer.transform.localPosition,
                    baseColor = renderer.color,
                    phase = seed + i * phaseStep,
                });
            }
        }

        private void OnDisable()
        {
            foreach (var target in targets)
            {
                if (target.transform == null) continue;
                target.transform.localPosition = target.basePosition;
                if (target.renderer != null) target.renderer.color = target.baseColor;
            }
        }

        private void LateUpdate()
        {
            var time = Time.unscaledTime;
            var positionAngle = period > 0f ? time * (2f * Mathf.PI / period) : 0f;
            var brightnessAngle = brightnessPeriod > 0f ? time * (2f * Mathf.PI / brightnessPeriod) : 0f;
            foreach (var target in targets)
            {
                if (target.transform == null) continue;
                var position = target.basePosition;
                position.y += Mathf.Sin(positionAngle + target.phase) * amplitude;
                target.transform.localPosition = position;

                if (target.renderer == null || brightnessAmplitude <= 0f) continue;
                var factor = 1f + Mathf.Sin(brightnessAngle + target.phase * 0.7f) * brightnessAmplitude;
                var color = target.baseColor;
                color.r = Mathf.Clamp01(color.r * factor);
                color.g = Mathf.Clamp01(color.g * factor);
                color.b = Mathf.Clamp01(color.b * factor);
                target.renderer.color = color;
            }
        }
    }
}
