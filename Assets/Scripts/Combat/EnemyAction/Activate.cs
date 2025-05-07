using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class Activate : EnemyAction
    {
        [SerializeField]
        private List<GameObject> activateTargets = new List<GameObject>();

        private void Awake() { }

        public override void Execute()
        {
            if (IsExecuting)
                return;
            if (IsDone)
                return;
            IsExecuting = true;
            foreach (GameObject target in activateTargets)
            {
                if (target == null)
                    continue;
                target.SetActive(true);
            }
            IsDone = true;
        }
    }
}
