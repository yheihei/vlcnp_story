using UnityEngine;
using VLCNP.Core;

public class AuraController : MonoBehaviour
{
    private ParticleSystem auraParticleSystem;
    private FlagManager flagManager;
    private bool isInitialized = false;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        // ParticleSystemコンポーネントを取得
        auraParticleSystem = GetComponent<ParticleSystem>();
        if (auraParticleSystem == null)
        {
            Debug.LogError("AuraController: ParticleSystem component not found!");
            return;
        }

        // FlagManagerを探して取得
        flagManager = FindObjectOfType<FlagManager>();
        if (flagManager == null)
        {
            Debug.LogError("AuraController: FlagManager not found!");
            return;
        }

        // フラグ変更イベントを監視
        flagManager.OnChangeFlag += OnFlagChanged;

        // 初期状態をチェック
        UpdateAuraVisibility();

        isInitialized = true;
    }

    private void OnDestroy()
    {
        // イベントの購読を解除
        if (flagManager != null)
        {
            flagManager.OnChangeFlag -= OnFlagChanged;
        }
    }

    private void OnFlagChanged(Flag flag, bool value)
    {
        // NoEnemiesフラグの変更のみを監視
        if (flag == Flag.NoEnemies)
        {
            UpdateAuraVisibility();
        }
    }

    private void UpdateAuraVisibility()
    {
        if (!isInitialized || auraParticleSystem == null || flagManager == null)
        {
            return;
        }

        bool noEnemies = flagManager.GetFlag(Flag.NoEnemies);
        
        if (noEnemies)
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

    // 外部からオーラの可視性を強制更新するためのメソッド
    public void ForceUpdateVisibility()
    {
        if (isInitialized)
        {
            UpdateAuraVisibility();
        }
    }
}