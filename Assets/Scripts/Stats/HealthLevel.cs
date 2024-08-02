using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VLCNP.Saving;

namespace VLCNP.Stats
{
    public class HealthLevel : MonoBehaviour, IJsonSaveable
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
 
        public JToken CaptureAsJToken()
        {
            return JToken.FromObject(currentLevel);
        }

        public void RestoreFromJToken(JToken state)
        {
            // BaseStatsのLevelに引き継ぎ
            currentLevel = state.ToObject<int>();
            baseStats.SetCurrentHealthLevel(currentLevel);
        }
    }
}
