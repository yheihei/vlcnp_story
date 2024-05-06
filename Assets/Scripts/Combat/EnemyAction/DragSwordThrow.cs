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
        [SerializeField] Transform handTransform = null;
        [SerializeField] float animationOffsetWaitTime = 0.417f;
        [SerializeField] public uint priority = 1;
        public uint Priority { get => priority; }
        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

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
            // animatorの、"throw"トリガーを発動する
            if (animator != null)
            {
                animator.SetTrigger("throw");
            }
            // animationが完了するまで待つ調整
            yield return new WaitForSeconds(animationOffsetWaitTime);
            bool isLeft = transform.lossyScale.x > 0;
            weaponConfig.LaunchProjectile(handTransform, 1, isLeft);
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
