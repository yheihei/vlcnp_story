using UnityEngine;
using UnityEngine.Events;
using TNRD;
using VLCNP.Combat;

namespace Projectiles.StatusEffects
{
    /// <summary>
    /// Projectileが敵にヒットした際に状態効果を適用するコンポーネント
    /// IProjectileのOnTargetHitイベントを購読し、設定された状態効果を適用する
    /// </summary>
    public class ProjectileStatusEffectApplier : MonoBehaviour
    {
        [Header("状態効果設定")]
        [SerializeField]
        [Tooltip("適用する状態効果のリスト。複数設定可能")]
        private SerializableInterface<IProjectileStatusEffect>[] statusEffects;

        private IProjectile projectile;

        void Start()
        {
            // このGameObjectにアタッチされたIProjectileを取得
            projectile = GetComponent<IProjectile>();
            if (projectile == null)
            {
                Debug.LogError($"ProjectileStatusEffectApplier: IProjectileを実装したコンポーネントが見つかりません。GameObject: {gameObject.name}");
                return;
            }

            // OnTargetHitイベントを購読
            projectile.OnTargetHit.AddListener(OnTargetHit);
        }

        void OnDestroy()
        {
            // イベントの購読を解除
            if (projectile != null)
            {
                projectile.OnTargetHit.RemoveListener(OnTargetHit);
            }
        }

        /// <summary>
        /// ターゲットにヒットした際に呼び出される
        /// </summary>
        /// <param name="target">ヒットしたターゲット</param>
        private void OnTargetHit(GameObject target)
        {
            if (statusEffects == null || statusEffects.Length == 0)
                return;

            // 設定されたすべての状態効果を適用
            foreach (var effectInterface in statusEffects)
            {
                if (effectInterface != null && effectInterface.Value != null)
                {
                    try
                    {
                        effectInterface.Value.ApplyEffect(target);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"状態効果の適用中にエラーが発生しました。効果: {effectInterface.Value.EffectName}, ターゲット: {target.name}, エラー: {e.Message}");
                    }
                }
            }
        }
    }
}