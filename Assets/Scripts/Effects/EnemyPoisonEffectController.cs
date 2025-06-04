using UnityEngine;
using VLCNP.Stats;

namespace VLCNP.Effects
{
    public class EnemyPoisonEffectController : MonoBehaviour
    {
        [Tooltip("毒エフェクトのプレファブ")]
        public GameObject poisonEffectPrefab;

        [Tooltip("エフェクトを追従させる対象のTransform")]
        public Transform effectParent;

        private EnemyPoisonStatus targetPoisonStatus;
        private GameObject currentEffectInstance;

        void Awake()
        {
            targetPoisonStatus = GetComponentInParent<EnemyPoisonStatus>();
            if (targetPoisonStatus == null)
            {
                targetPoisonStatus = GetComponent<EnemyPoisonStatus>();
            }

            if (targetPoisonStatus == null)
            {
                Debug.LogError(
                    "EnemyPoisonStatusが見つかりませんでした。EnemyPoisonEffectControllerは動作しません。",
                    this.gameObject
                );
                enabled = false;
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
                effectParent = this.transform;
            }
        }

        void OnEnable()
        {
            if (targetPoisonStatus != null)
            {
                targetPoisonStatus.OnPoisonStarted += ShowEffect;
                targetPoisonStatus.OnPoisonCured += HideEffect;
                
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