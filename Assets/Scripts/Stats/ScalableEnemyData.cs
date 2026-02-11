using UnityEngine;
using VLCNP.Core;

namespace RPG.Stats
{
    [CreateAssetMenu(fileName = "ScalableEnemyData", menuName = "RPG/Enemy Scaling Data")]
    public class ScalableEnemyData : ScriptableObject
    {
        [SerializeField] private Flag[] watchedFlags;
        [SerializeField] private float[] experiencePerFlagCount;

        public Flag[] GetWatchedFlags()
        {
            return watchedFlags;
        }

        public float GetExperienceForFlagCount(int flagCount)
        {
            if (experiencePerFlagCount == null || experiencePerFlagCount.Length == 0)
            {
                return 0f;
            }

            int index = Mathf.Clamp(flagCount, 0, experiencePerFlagCount.Length - 1);
            return experiencePerFlagCount[index];
        }

        public int GetMaxFlagCount()
        {
            return watchedFlags != null ? watchedFlags.Length : 0;
        }

        private void OnValidate()
        {
            if (watchedFlags != null && experiencePerFlagCount != null)
            {
                if (experiencePerFlagCount.Length < watchedFlags.Length + 1)
                {
                    Debug.LogWarning($"ScalableEnemyData '{name}': Experience array should have at least {watchedFlags.Length + 1} elements (0 flags to {watchedFlags.Length} flags)");
                }
            }
        }
    }
}