using System;
using UnityEngine;

namespace VLCNP.Stats
{
    public class Experience : MonoBehaviour
    {
        [SerializeField] float experiencePoints = 0;
        // public delegate void ExperienceGainedDelegate();
        public event Action onExperienceGained;
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
    }
}
