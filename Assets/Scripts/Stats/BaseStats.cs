using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Attributes;

namespace VLCNP.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [SerializeField] StatClass statClass;
        [SerializeField] Progression progression = null;
        int currentLevel = 1;
        int currentHealthLevel = 1;
        Experience experience;
        public event Action<int> OnChangeLevel;
        Health health;

        private void Awake() {
            experience = GetComponent<Experience>();
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            if (experience != null)
            {
                experience.onExperienceGained += UpdateLevel;
                experience.onExperienceLost += UpdateLevel;
            }
        }

        private void OnDisable() {
            if (experience != null)
            {
                experience.onExperienceGained -= UpdateLevel;
                experience.onExperienceLost -= UpdateLevel;
            }
        }

        private void UpdateLevel() {
            int newLevel = CalculateLevel();
            currentLevel = newLevel;
            OnChangeLevel(currentLevel);
        }

        private int CalculateLevel()
        {
            Experience experience = GetComponent<Experience>();
            if (experience == null) return 1;
            float currentXP = experience.GetExperiencePoints();
            int penultimateLevel = progression.GetLevels(Stat.ExperienceToLevelUp, statClass);
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

        public void IncrementHealthLevel()
        {
            currentHealthLevel++;
            health.SetHealthPoints(GetStat(Stat.Health));
        }

        public int GetCurrentHealthLevel()
        {
            return currentHealthLevel;
        }

        public void SetCurrentHealthLevel(int level)
        {
            currentHealthLevel = level;
        }

        public int GetMaxLevel()
        {
            return progression.GetLevels(Stat.ExperienceToLevelUp, statClass) + 1;
        }

        public int GetLevel()
        {
            return currentLevel;
        }

        // 次のレベルまでの経験値を取得
        public float GetExperienceToNextLevel()
        {
            if (isReachedMaxLevel())
            {
                return 0;
            }
            return progression.GetStat(Stat.ExperienceToLevelUp, statClass, currentLevel);
        }

        // いまのレベルまでの経験値を取得
        public float GetExperienceToCurrentLevel()
        {
            if (currentLevel == 1)
            {
                return 0;
            }
            return progression.GetStat(Stat.ExperienceToLevelUp, statClass, currentLevel - 1);
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
            if (stat == Stat.Health)
            {
                return progression.GetStat(Stat.Health, statClass, currentHealthLevel);
            }
            return progression.GetStat(stat, statClass, currentLevel);
        }
    }
}
