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
        private ParticleSystem auraParticleSystem;

        private void Awake()
        {
            GameObject flagManagerObject = GameObject.FindWithTag("FlagManager");
            if (flagManagerObject != null)
            {
                flagManager = flagManagerObject.GetComponent<FlagManager>();
            }

            // 自分自身からParticleSystemを取得
            auraParticleSystem = GetComponent<ParticleSystem>();
            if (auraParticleSystem == null)
            {
                auraParticleSystem = GetComponentInChildren<ParticleSystem>();
            }
        }

        private void OnEnable()
        {
            if (flagManager != null)
            {
                flagManager.OnChangeFlag += OnFlagChanged;
            }
        }

        private void OnDisable()
        {
            if (flagManager != null)
            {
                flagManager.OnChangeFlag -= OnFlagChanged;
            }
        }

        private void Start()
        {
            // 初期状態を設定
            UpdateAuraState();
        }

        private void OnFlagChanged(Flag flag, bool value)
        {
            if (flag == Flag.NoEnemies)
            {
                UpdateAuraState();
            }
        }

        private void UpdateAuraState()
        {
            if (auraParticleSystem == null)
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
            UpdateAuraState();
        }
    }
}
