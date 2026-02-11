using UnityEngine;
using VLCNP.Core;

namespace RPG.Stats
{
    [RequireComponent(typeof(Experience))]
    public class EnemyScaler : MonoBehaviour
    {
        [SerializeField] private ScalableEnemyData scalingData;
        [SerializeField] private bool debugMode = false;

        private Experience experience;
        private FlagManager flagManager;
        private bool isInitialized = false;

        private void Awake()
        {
            experience = GetComponent<Experience>();
            if (experience == null)
            {
                Debug.LogError($"EnemyScaler on {gameObject.name}: Experience component is required but not found.");
            }
        }

        private void Start()
        {
            InitializeScaling();
        }

        private void InitializeScaling()
        {
            if (isInitialized) return;

            flagManager = FindObjectOfType<FlagManager>();
            if (flagManager == null)
            {
                Debug.LogError($"EnemyScaler on {gameObject.name}: FlagManager not found in scene.");
                return;
            }

            if (scalingData == null)
            {
                if (debugMode)
                {
                    Debug.LogWarning($"EnemyScaler on {gameObject.name}: No scaling data assigned. Enemy will not be scaled.");
                }
                return;
            }

            ApplyScaling();
            
            // フラグ変更時の再評価を登録
            flagManager.OnChangeFlag += OnFlagChanged;
            
            isInitialized = true;

            if (debugMode)
            {
                int flagCount = CountActivatedFlags();
                float currentExperience = experience.GetCurrentExperience();
                Debug.Log($"EnemyScaler on {gameObject.name}: Initialized with {flagCount} flags, {currentExperience} experience");
            }
        }

        private void OnDestroy()
        {
            if (flagManager != null)
            {
                flagManager.OnChangeFlag -= OnFlagChanged;
            }
        }

        private void OnFlagChanged(Flag flag, bool isActive)
        {
            if (!isInitialized || scalingData == null) return;

            // 監視対象フラグのみに反応
            Flag[] watchedFlags = scalingData.GetWatchedFlags();
            if (watchedFlags == null) return;

            bool isWatchedFlag = false;
            for (int i = 0; i < watchedFlags.Length; i++)
            {
                if (watchedFlags[i] == flag)
                {
                    isWatchedFlag = true;
                    break;
                }
            }

            if (isWatchedFlag)
            {
                ApplyScaling();
                
                if (debugMode)
                {
                    int flagCount = CountActivatedFlags();
                    float currentExperience = experience.GetCurrentExperience();
                    Debug.Log($"EnemyScaler on {gameObject.name}: Flag {flag} changed to {isActive}. New flag count: {flagCount}, experience: {currentExperience}");
                }
            }
        }

        private void ApplyScaling()
        {
            if (experience == null || scalingData == null) return;

            int flagCount = CountActivatedFlags();
            float experienceToSet = scalingData.GetExperienceForFlagCount(flagCount);
            
            // SetExperiencePointsIfGreater を使用して、現在の経験値より高い場合のみ設定
            experience.SetExperiencePointsIfGreater(experienceToSet);
        }

        private int CountActivatedFlags()
        {
            if (flagManager == null || scalingData == null) return 0;

            Flag[] watchedFlags = scalingData.GetWatchedFlags();
            if (watchedFlags == null) return 0;

            int count = 0;
            for (int i = 0; i < watchedFlags.Length; i++)
            {
                if (flagManager.GetFlag(watchedFlags[i]))
                {
                    count++;
                }
            }

            return count;
        }

        // エディター用のデバッグ情報表示
        public void LogCurrentStatus()
        {
            if (scalingData == null)
            {
                Debug.Log($"EnemyScaler on {gameObject.name}: No scaling data assigned.");
                return;
            }

            int flagCount = CountActivatedFlags();
            float experienceForFlags = scalingData.GetExperienceForFlagCount(flagCount);
            float currentExperience = experience != null ? experience.GetCurrentExperience() : 0f;
            
            Debug.Log($"EnemyScaler on {gameObject.name}:\n" +
                     $"  Active flags: {flagCount}/{scalingData.GetMaxFlagCount()}\n" +
                     $"  Experience for flags: {experienceForFlags}\n" +
                     $"  Current experience: {currentExperience}");
        }

        // Unity Inspector での確認用
        private void OnValidate()
        {
            if (scalingData == null && debugMode)
            {
                Debug.LogWarning($"EnemyScaler on {gameObject.name}: Scaling data is not assigned.");
            }
        }
    }
}