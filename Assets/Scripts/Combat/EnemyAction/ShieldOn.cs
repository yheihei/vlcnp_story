using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class ShieldOn : EnemyAction
    {
        /**
        リストで指定されたShieldコンポーネントをenableする。
        **/

        [SerializeField]
        List<Shield> shields;

        public override void Execute()
        {
            if (IsExecuting || IsDone)
                return;
            IsExecuting = true;
            StartCoroutine(On());
        }

        private IEnumerator On()
        {
            foreach (Shield shield in shields)
            {
                shield.gameObject.SetActive(true);
            }
            IsDone = true;
            yield break;
        }
    }
}
