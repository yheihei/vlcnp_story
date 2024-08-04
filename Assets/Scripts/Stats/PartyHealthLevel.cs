using Newtonsoft.Json.Linq;
using UnityEngine;
using VLCNP.Saving;

namespace VLCNP.Stats
{
    public class PartyHealthLevel : MonoBehaviour, IJsonSaveable
    {
        [SerializeField, Min(1)] int currentLevel = 1;

        public int GetCurrentLevel()
        {
            return currentLevel;
        }

        public void SetLevel(int level, BaseStats currentPlayerBaseStats)
        {
            currentLevel = level;
            currentPlayerBaseStats.SetCurrentHealthLevel(level);
        }
 
        public JToken CaptureAsJToken()
        {
            return JToken.FromObject(currentLevel);
        }

        public void RestoreFromJToken(JToken state)
        {
            // BaseStatsのLevelに引き継ぎ
            currentLevel = state.ToObject<int>();
        }
    }
}
