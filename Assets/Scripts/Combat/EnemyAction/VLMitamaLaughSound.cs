using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    static class VLMitamaLaughSound
    {
        static AudioClip generatedLaughClip = null;

        public static void Play(AudioClip clip, Vector3 position, float volume)
        {
            AudioClip sound = clip != null ? clip : GetGeneratedLaughClip();
            if (sound != null)
                AudioSource.PlayClipAtPoint(sound, position, volume);
        }

        static AudioClip GetGeneratedLaughClip()
        {
            if (generatedLaughClip != null)
                return generatedLaughClip;

            const int frequency = 44100;
            const float duration = 0.35f;
            int sampleCount = Mathf.CeilToInt(frequency * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / frequency;
                float envelope = Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI);
                float wave =
                    Mathf.Sin(2f * Mathf.PI * 720f * t)
                    + 0.45f * Mathf.Sin(2f * Mathf.PI * 980f * t);
                samples[i] = wave * envelope * 0.18f;
            }

            generatedLaughClip = AudioClip.Create(
                "VLMitamaBoss_TempLaugh",
                sampleCount,
                1,
                frequency,
                false
            );
            generatedLaughClip.SetData(samples, 0);
            return generatedLaughClip;
        }
    }
}
