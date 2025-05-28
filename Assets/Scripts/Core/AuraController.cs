using UnityEngine;
using VLCNP.Stats;

namespace VLCNP.Core
{
    /// <summary>
    /// オーラエフェクトのフラグベース制御を担当するクラス
    /// 「敵がいない」フラグの状態に応じてパーティクルシステムを制御する
    /// </summary>
    public class AuraController : MonoBehaviour
    {
        private FlagManager flagManager;
        private BaseStats baseStats;
        private ChangeSpriteAnimationOnLevelUp spriteChanger;
        private ParticleSystem auraParticleSystem;
        
        private bool isAuraActiveByLevel = false;
        
        private void Awake()
        {
            flagManager = FindObjectOfType<FlagManager>();
            baseStats = GetComponent<BaseStats>();
            spriteChanger = GetComponent<ChangeSpriteAnimationOnLevelUp>();
        }
        
        private void OnEnable()
        {
            if (flagManager != null)
            {
                flagManager.OnChangeFlag += OnFlagChanged;
            }
            
            if (baseStats != null)
            {
                baseStats.OnChangeLevel += OnLevelChanged;
            }
        }
        
        private void OnDisable()
        {
            if (flagManager != null)
            {
                flagManager.OnChangeFlag -= OnFlagChanged;
            }
            
            if (baseStats != null)
            {
                baseStats.OnChangeLevel -= OnLevelChanged;
            }
        }
        
        private void Start()
        {
            // 初期状態を設定
            UpdateAuraState();
        }
        
        private void OnFlagChanged(Flag flag)
        {
            if (flag == Flag.NoEnemies)
            {
                UpdateAuraState();
            }
        }
        
        private void OnLevelChanged(int newLevel)
        {
            // レベル3の場合のみオーラが有効
            isAuraActiveByLevel = (newLevel >= 3);
            
            // パーティクルシステムの参照を更新
            UpdateParticleSystemReference();
            
            UpdateAuraState();
        }
        
        private void UpdateParticleSystemReference()
        {
            // オーラオブジェクトからパーティクルシステムを取得
            Transform leg = transform.Find("Leg");
            if (leg != null)
            {
                // オーラエフェクトを子オブジェクトから検索
                foreach (Transform child in leg)
                {
                    ParticleSystem ps = child.GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        auraParticleSystem = ps;
                        break;
                    }
                }
            }
        }
        
        private void UpdateAuraState()
        {
            if (!isAuraActiveByLevel || auraParticleSystem == null)
            {
                return;
            }
            
            bool noEnemiesFlag = flagManager != null && flagManager.GetFlag(Flag.NoEnemies);
            
            if (noEnemiesFlag)
            {
                // 敵がいない場合はパーティクルシステムを停止
                if (auraParticleSystem.isPlaying)
                {
                    auraParticleSystem.Stop();
                }
            }
            else
            {
                // 敵がいる場合はパーティクルシステムを再生
                if (!auraParticleSystem.isPlaying)
                {
                    auraParticleSystem.Play();
                }
            }
        }
        
        /// <summary>
        /// 外部からオーラの状態を強制更新する場合に使用
        /// </summary>
        public void ForceUpdateAuraState()
        {
            UpdateParticleSystemReference();
            UpdateAuraState();
        }
    }
}