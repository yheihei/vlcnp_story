using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [SerializeField] StatClass statClass;
        [SerializeField] Progression progression = null;
        int currentLevel = 1;
        Experience experience;

        private void Awake() {
            experience = GetComponent<Experience>();
        }

        private void OnEnable()
        {
            if (experience != null)
            {
                experience.onExperienceGained += UpdateLevel;
            }
        }

        private void OnDisable() {
            if (experience != null)
            {
                experience.onExperienceGained -= UpdateLevel;
            }
        }

        private void UpdateLevel() {
            int newLevel = CalculateLevel();
            if (newLevel > currentLevel)
            {
                currentLevel = newLevel;
                print($"Levelled Up! {currentLevel}");
                // if (levelUpEffect != null)
                // {
                //     LevelUpEffect();
                //     onLevelUp();
                // }
            }
        }

        private int CalculateLevel()
        {
            Experience experience = GetComponent<Experience>();
            if (experience == null) return 1;
            float currentXP = experience.GetExperiencePoints();
            print($"currentXP: {currentXP}");
            int penultimateLevel = progression.GetLevels(Stat.ExperienceToLevelUp, statClass);
            print($"penultimateLevel: {penultimateLevel}");
            for (int level = 1; level <= penultimateLevel; level++)
            {
                float XPToLevelUp = progression.GetStat(Stat.ExperienceToLevelUp, statClass, level);
                if (XPToLevelUp > currentXP)
                {
                    return level;
                }
            }

            return penultimateLevel + 1;
        }

        public int GetMaxLevel()
        {
            return progression.GetLevels(Stat.ExperienceToLevelUp, statClass) + 1;
        }

        public int GetLevel()
        {
            return currentLevel;
        }

        public bool isReachedMaxLevel()
        {
            return currentLevel >= GetMaxLevel();
        }

        public float GetStat(Stat stat)
        {
            return GetBaseStat(stat);
        }

        private float GetBaseStat(Stat stat)
        {
            return progression.GetStat(stat, statClass, currentLevel);
        }
    }
}
