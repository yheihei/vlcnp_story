using System;
using UnityEngine;

namespace VLCNP.Stats
{
    public class Experience : MonoBehaviour
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
            print($"Lose {loseExperiencePoint} experience");
            experiencePoints = Mathf.Max(experiencePoints - loseExperiencePoint, 0);
            onExperienceLost();
        }
    }
}
