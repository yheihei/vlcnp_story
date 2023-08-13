using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [SerializeField] StatClass statClass;
        [SerializeField] Progression characterStats = null;
        int currentLevel = 1;

        public float GetStat(Stat stat)
        {
            return GetBaseStat(stat);
        }

        private float GetBaseStat(Stat stat)
        {
            return characterStats.GetStat(stat, statClass, currentLevel);
        }
    }
}
