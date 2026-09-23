using UnityEngine;
using VLCNP.Attributes;

namespace VLCNP.Combat
{
    /**
     * 触れた相手を無敵時間に関係なく即死させる。DirectAttack の OnAttackSuccess から呼ぶ
     */
    public class InstantKill : MonoBehaviour
    {
        public void Kill(GameObject target)
        {
            if (target == null)
                return;
            Health health = target.GetComponent<Health>();
            if (health == null)
                return;
            health.Kill();
        }
    }
}
