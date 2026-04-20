using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Effects
{
    public static class ParticleEffectFadeOut
    {
        struct MaterialColorState
        {
            public Material material;
            public string propertyName;
            public Color initialColor;
        }

        static readonly string[] ColorPropertyNames =
        {
            "_BaseColor",
            "_Color",
            "_TintColor",
        };

        public static IEnumerator FadeOutAndDestroy(GameObject effect, float duration)
        {
            if (effect == null)
                yield break;

            StopParticleEmission(effect);

            if (duration <= 0f)
            {
                Object.Destroy(effect);
                yield break;
            }

            List<MaterialColorState> colorStates = CaptureMaterialColors(effect);
            float elapsed = 0f;

            while (elapsed < duration && effect != null)
            {
                elapsed += Time.deltaTime;
                float alphaRate = 1f - Mathf.Clamp01(elapsed / duration);
                ApplyAlphaRate(colorStates, alphaRate);
                yield return null;
            }

            if (effect != null)
                Object.Destroy(effect);
        }

        static void StopParticleEmission(GameObject effect)
        {
            ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                if (particleSystems[i] != null)
                    particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        static List<MaterialColorState> CaptureMaterialColors(GameObject effect)
        {
            List<MaterialColorState> colorStates = new List<MaterialColorState>();
            Renderer[] renderers = effect.GetComponentsInChildren<Renderer>(true);

            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                if (renderer == null)
                    continue;

                Material[] materials = renderer.materials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    Material material = materials[materialIndex];
                    if (material == null)
                        continue;

                    for (int propertyIndex = 0; propertyIndex < ColorPropertyNames.Length; propertyIndex++)
                    {
                        string propertyName = ColorPropertyNames[propertyIndex];
                        if (!material.HasProperty(propertyName))
                            continue;

                        colorStates.Add(new MaterialColorState
                        {
                            material = material,
                            propertyName = propertyName,
                            initialColor = material.GetColor(propertyName),
                        });
                    }
                }
            }

            return colorStates;
        }

        static void ApplyAlphaRate(List<MaterialColorState> colorStates, float alphaRate)
        {
            for (int i = 0; i < colorStates.Count; i++)
            {
                MaterialColorState colorState = colorStates[i];
                if (colorState.material == null)
                    continue;

                Color color = colorState.initialColor;
                color.a *= alphaRate;
                colorState.material.SetColor(colorState.propertyName, color);
            }
        }
    }
}
