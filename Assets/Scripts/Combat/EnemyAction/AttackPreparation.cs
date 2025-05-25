using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using VLCNP.Movement;

namespace VLCNP.Combat.EnemyAction
{
    public class AttackPreparation : EnemyAction
    {
        [SerializeField]
        string animationName = "special1";

        [SerializeField]
        float animationOffsetWaitTime = 0.9f;

        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError($"Animator component not found on {gameObject.name}");
            }
        }

        public override void Execute()
        {
            if (IsExecuting)
                return;
            if (IsDone)
                return;
            IsExecuting = true;
            StartCoroutine(ChangeAnimation());
        }

        private IEnumerator ChangeAnimation()
        {
            animator.SetTrigger(animationName);
            yield return new WaitForSeconds(animationOffsetWaitTime);
            IsDone = true;
        }
    }
}
