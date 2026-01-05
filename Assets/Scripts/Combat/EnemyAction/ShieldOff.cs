using System.Collections;
using System.Collections.Generic;
using Fungus;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class ShieldOff : EnemyAction
    {
        /**
        リストで指定されたShieldコンポーネントをdisableする。
        **/

        [SerializeField]
        List<Shield> shields;

        public override void Execute()
        {
            if (IsExecuting || IsDone)
                return;
            IsExecuting = true;
            StartCoroutine(Off());
        }

        private IEnumerator Off()
        {
            foreach (Shield shield in shields)
            {
                shield.gameObject.SetActive(false);
            }
            IsDone = true;
            yield break;
        }
    }
}
