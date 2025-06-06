using UnityEngine;
using UnityEngine.Events;

namespace VLCNP.Combat
{
    public interface IProjectile
    {
        bool IsStucking { get; }
        
        void SetDirection(bool isLeft);
        void SetDamage(float damage);
        void ImpactAndDestroy();
        
        /// <summary>
        /// ターゲットにヒットした際に発火するイベント
        /// </summary>
        UnityEvent<GameObject> OnTargetHit { get; }
    }
}