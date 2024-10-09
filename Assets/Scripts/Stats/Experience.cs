using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VLCNP.Saving;

namespace VLCNP.Stats
{
    public class Experience : MonoBehaviour, IJsonSaveable
    {
        [SerializeField] float experiencePoints = 0;
        [SerializeField] float loseExperienceModifier = 2f;
        [SerializeField] float SEVolume = 0.3f;
        AudioSource audioSource;
        [SerializeField] AudioClip getSeSound = null;
        // public delegate void ExperienceGainedDelegate();
        public event Action onExperienceGained;
        public event Action onExperienceLost;
        BaseStats baseStats;

        private void Awake() {
            baseStats = GetComponent<BaseStats>();
            audioSource = GetComponent<AudioSource>();
        }

        public float GetExperiencePoints()
        {
            return experiencePoints;
        }

        public void GainExperience(float experience)
        {
            if (baseStats.isReachedMaxLevel()) return;
            experiencePoints += experience;
            if (audioSource != null && getSeSound != null)
            {
                audioSource.PlayOneShot(getSeSound, SEVolume);
            }
            onExperienceGained();
        }

        public void LoseExperience(float experience)
        {
            float loseExperiencePoint = experience * loseExperienceModifier;
            experiencePoints = Mathf.Max(experiencePoints - loseExperiencePoint, 0);
            onExperienceLost();
        }

        public void SetExperiencePoints(float experience)
        {
            float beforeExperience = experiencePoints;
            experiencePoints = experience;
            if (beforeExperience < experiencePoints)
            {
                onExperienceGained?.Invoke();
            }
            else if (beforeExperience > experiencePoints)
            {
                onExperienceLost?.Invoke();
            }
        }

        // 経験値を指定の値にセットするが すでにその経験値を超えている場合は何もしない
        public void SetExperiencePointsIfGreater(float experience)
        {
            if (experiencePoints < experience)
            {
                SetExperiencePoints(experience);
            }
        }

        public JToken CaptureAsJToken()
        {
            return JToken.FromObject(experiencePoints);
        }

        public void RestoreFromJToken(JToken state)
        {
            experiencePoints = state.ToObject<float>();
            if (onExperienceGained != null) onExperienceGained();  // レベルの再計算
        }
    }
}
