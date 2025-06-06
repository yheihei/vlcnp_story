using UnityEngine;

namespace Projectiles.StatusEffects
{
    /// <summary>
    /// Projectileが敵にヒットした際に適用する状態効果のインターフェース
    /// </summary>
    public interface IProjectileStatusEffect
    {
        /// <summary>
        /// 状態効果名
        /// </summary>
        string EffectName { get; }
        
        /// <summary>
        /// 指定されたターゲットに状態効果を適用する
        /// </summary>
        /// <param name="target">状態効果を適用するターゲット</param>
        void ApplyEffect(GameObject target);
    }
}