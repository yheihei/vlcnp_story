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
        // public delegate void ExperienceGainedDelegate();
        public event Action onExperienceGained;
        public event Action onExperienceLost;
        BaseStats baseStats;

        private void Awake() {
            baseStats = GetComponent<BaseStats>();
        }

        public float GetExperiencePoints()
        {
            return experiencePoints;
        }

        public void GainExperience(float experience)
        {
            if (baseStats.isReachedMaxLevel()) return;
            experiencePoints += experience;
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
            experiencePoints = experience;
            // onExperienceGained();  // レベルの再計算
            // 通知対象があれば通知する
            if (onExperienceGained != null) onExperienceGained();
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
