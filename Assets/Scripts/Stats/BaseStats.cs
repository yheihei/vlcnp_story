using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [SerializeField] CharacterClass characterClass;
        [SerializeField] CharacterStats characterStats = null;
        int currentLevel = 1;

        public float GetStat(Stat stat)
        {
            return GetBaseStat(Stat.Health);
        }

        private float GetBaseStat(Stat stat)
        {
            return characterStats.GetStat(stat, characterClass, currentLevel);
        }
    }
}
