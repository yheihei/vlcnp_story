using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Combat
{
    /**
     * 1回の攻撃で同時に出した弾どうしで当てた相手を共有し、同じ相手へのダメージを1回にまとめる
     */
    public class ProjectileVolley
    {
        readonly HashSet<GameObject> hitTargets = new HashSet<GameObject>();

        // まだこの攻撃のどの弾も当てていない相手なら記録して true を返す
        public bool TryRegisterHit(GameObject target)
        {
            return hitTargets.Add(target);
        }
    }
}
