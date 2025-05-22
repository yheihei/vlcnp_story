using UnityEngine;
using VLCNP.Stats; // PoisonStatus を使用するために追加

namespace VLCNP.Effects
{
    public class PoisonEffectController : MonoBehaviour
    {
        [Tooltip("毒エフェクトのプレファブ")]
        public GameObject poisonEffectPrefab;

        [Tooltip("エフェクトを追従させる対象のTransform")]
        public Transform effectParent;

        private PoisonStatus targetPoisonStatus;
        private GameObject currentEffectInstance;

        void Awake()
        {
            // 親オブジェクト、または同じGameObjectにPoisonStatusがあると仮定
            targetPoisonStatus = GetComponentInParent<VLCNP.Stats.PoisonStatus>();
            if (targetPoisonStatus == null)
            {
                // もし見つからなければ、自身のGameObjectからも検索
                targetPoisonStatus = GetComponent<VLCNP.Stats.PoisonStatus>();
            }

            if (targetPoisonStatus == null)
            {
                Debug.LogError(
                    "PoisonStatusが見つかりませんでした。PoisonEffectControllerは動作しません。",
                    this.gameObject
                );
                enabled = false; // このコンポーネントを無効化
                return;
            }

            if (poisonEffectPrefab == null)
            {
                Debug.LogError("PoisonEffectPrefabが設定されていません。", this.gameObject);
                enabled = false;
                return;
            }

            if (effectParent == null)
            {
                Debug.LogWarning(
                    "EffectParentが設定されていません。エフェクトはこのGameObjectの子になります。",
                    this.gameObject
                );
                effectParent = this.transform; // デフォルトとして自身のTransformを使用
            }
        }

        void OnEnable()
        {
            if (targetPoisonStatus != null)
            {
                targetPoisonStatus.OnPoisonStarted += ShowEffect;
                targetPoisonStatus.OnPoisonCured += HideEffect;
                // 初期状態確認（すでに毒状態の場合）
                if (targetPoisonStatus.IsPoisoned)
                {
                    ShowEffect();
                }
            }
        }

        void OnDisable()
        {
            if (targetPoisonStatus != null)
            {
                targetPoisonStatus.OnPoisonStarted -= ShowEffect;
                targetPoisonStatus.OnPoisonCured -= HideEffect;
            }
            // オブジェクトが無効化されたときにエフェクトも消す
            HideEffect();
        }

        void ShowEffect()
        {
            if (poisonEffectPrefab == null)
                return;
            if (currentEffectInstance == null)
            {
                currentEffectInstance = Instantiate(poisonEffectPrefab, effectParent);
            }
            else
            {
                // すでにインスタンスがあるが非アクティブな場合（基本的には破棄しているので通らない想定）
                currentEffectInstance.SetActive(true);
            }
        }

        void HideEffect()
        {
            if (currentEffectInstance != null)
            {
                Destroy(currentEffectInstance);
                currentEffectInstance = null;
            }
        }
    }
}
