using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class DragSwordThrow : MonoBehaviour, IEnemyAction
    {
        bool isDone = false;
        bool isExecuting = false;
        public bool IsDone { get => isDone; set => isDone = value; }
        public bool IsExecuting { get => isExecuting; set => isExecuting = value; }

        [SerializeField] WeaponConfig weaponConfig = null;
        [SerializeField] public uint priority = 1;
        public uint Priority { get => priority; }

        public void Execute()
        {
            if (isExecuting) return;
            if (isDone) return;
            isExecuting = true;
            StartCoroutine(Throw());
        }

        private IEnumerator Throw()
        {
            if (!weaponConfig.HasProjectile())
            {
                isDone = true;
                yield break;
            }
            bool isLeft = transform.lossyScale.x > 0;
            weaponConfig.LaunchProjectile(transform, 1, isLeft);
            isDone = true;
        }

        /**
         * 行動実行後 再度実行可能にする
         */
        public void Reset()
        {
            isDone = false;
            isExecuting = false;
        }
    }    
}
