using UnityEngine;
using VLCNP.Core;

namespace VLCNP.Stats
{
    public class FlagBasedEnemyStrengthening : MonoBehaviour
    {
        [SerializeField] Flag[] strengtheningFlags = null;
        [SerializeField] float experiencePerFlag = 1f;
        
        private FlagManager flagManager;
        private Experience experience;
        private int lastFlagCount = 0;

        private void Awake()
        {
            experience = GetComponent<Experience>();
            if (experience == null)
            {
                Debug.LogWarning($"FlagBasedEnemyStrengthening on {gameObject.name} requires an Experience component but none was found.");
            }
        }

        private void Start()
        {
            flagManager = FindObjectOfType<FlagManager>();
            if (flagManager == null)
            {
                Debug.LogError("FlagManager not found in scene. FlagBasedEnemyStrengthening will not work.");
                return;
            }
            
            InitializeEnemyStrength();
            flagManager.OnChangeFlag += OnFlagChanged;
        }

        private void OnDestroy()
        {
            if (flagManager != null)
            {
                flagManager.OnChangeFlag -= OnFlagChanged;
            }
        }

        private void InitializeEnemyStrength()
        {
            int currentFlagCount = GetActiveFlagCount();
            if (currentFlagCount > 0 && experience != null)
            {
                float requiredExperience = currentFlagCount * experiencePerFlag;
                experience.SetExperiencePointsIfGreater(requiredExperience);
            }
            lastFlagCount = currentFlagCount;
        }

        private void OnFlagChanged(Flag flag, bool isActive)
        {
            if (!isActive || experience == null) return;
            
            if (System.Array.IndexOf(strengtheningFlags, flag) != -1)
            {
                int currentFlagCount = GetActiveFlagCount();
                if (currentFlagCount > lastFlagCount)
                {
                    float requiredExperience = currentFlagCount * experiencePerFlag;
                    experience.SetExperiencePointsIfGreater(requiredExperience);
                    lastFlagCount = currentFlagCount;
                }
            }
        }

        private int GetActiveFlagCount()
        {
            if (flagManager == null || strengtheningFlags == null) return 0;
            
            int count = 0;
            foreach (Flag flag in strengtheningFlags)
            {
                if (flagManager.GetFlag(flag))
                {
                    count++;
                }
            }
            return count;
        }

        public Flag[] GetStrengtheningFlags()
        {
            return strengtheningFlags;
        }

        public float GetExperiencePerFlag()
        {
            return experiencePerFlag;
        }

        public int GetCurrentActiveFlagCount()
        {
            return GetActiveFlagCount();
        }
    }
}