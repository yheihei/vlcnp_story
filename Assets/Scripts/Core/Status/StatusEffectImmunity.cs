using Projectiles.StatusEffects;
using UnityEngine;

namespace Core.Status
{
    /// <summary>
    /// 付けた敵に、指定した状態効果をかからなくする
    /// </summary>
    public class StatusEffectImmunity : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("かからない状態効果。オロチの毒の玉は ParalysisEffectByOrochi")]
        private ScriptableObject[] immuneEffects = new ScriptableObject[0];

        /// <summary>
        /// ターゲットがこの状態効果にかからないか
        /// </summary>
        public static bool IsImmune(GameObject target, IProjectileStatusEffect effect)
        {
            StatusEffectImmunity immunity = target.GetComponent<StatusEffectImmunity>();
            return immunity != null && immunity.IsImmuneTo(effect);
        }

        private bool IsImmuneTo(IProjectileStatusEffect effect)
        {
            foreach (ScriptableObject immuneEffect in immuneEffects)
            {
                if (immuneEffect != null && ReferenceEquals(immuneEffect, effect))
                    return true;
            }
            return false;
        }
    }
}
