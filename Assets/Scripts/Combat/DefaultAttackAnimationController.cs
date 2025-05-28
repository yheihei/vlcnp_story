using UnityEngine;

namespace VLCNP.Combat
{
    public class DefaultAttackAnimationController : MonoBehaviour, IAttackAnimationController
    {
        public void TriggerAttackAnimation()
        {
            // デフォルトでは何もしない（Leelee、Akimなどの攻撃アニメーション無しキャラクター用）
        }
    }
}