using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VLCNP.Saving;

namespace VLCNP.Stats
{
    public class HealthLevel : MonoBehaviour
    {
        [SerializeField, Min(1)] int currentLevel = 1;
        BaseStats baseStats;

        void Awake()
        {
            baseStats = GetComponent<BaseStats>();
        }

        public int GetCurrentLevel()
        {
            return currentLevel;
        }

        public void SetLevel(int level)
        {
            currentLevel = level;
            baseStats.SetCurrentHealthLevel(level);
        }
    }
}
