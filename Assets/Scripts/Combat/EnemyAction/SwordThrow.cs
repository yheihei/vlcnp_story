using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class SwordThrow : EnemyAction
    {

        [SerializeField] WeaponConfig weaponConfig = null;
        [SerializeField] Transform handTransform = null;
        [SerializeField] float animationOffsetWaitTime = 0.417f;
        private Animator animator;
        DamageStun damageStun;
        public enum Direction
        {
            Left,
            Right
        }

        Direction direction = Direction.Left;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            damageStun = GetComponent<DamageStun>();
        }

        public override void Execute()
        {
            if (IsExecuting) return;
            if (IsDone) return;
            IsExecuting = true;
            StartCoroutine(Throw());
        }

        private IEnumerator Throw()
        {
            if (weaponConfig == null || !weaponConfig.HasProjectile())
            {
                IsDone = true;
                yield break;
            }
            if (damageStun != null) damageStun.InvalidStan();
            
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                if (player.transform.position.x < transform.position.x)
                {
                    SetDirection(Direction.Left);
                }
                else
                {
                    SetDirection(Direction.Right);
                }
            }
            
            if (animator != null)
            {
                animator.SetTrigger("throw");
            }
            
            yield return new WaitForSeconds(animationOffsetWaitTime);
            
            if (handTransform != null)
            {
                bool isLeft = transform.lossyScale.x > 0;
                weaponConfig.LaunchProjectile(handTransform, 1, isLeft);
            }
            
            IsDone = true;
            if (damageStun != null) damageStun.ValidStan();
        }

        public void SetDirection(Direction _direction)
        {
            direction = _direction;
            UpdateCharacterDirection();
        }

        private void UpdateCharacterDirection()
        {
            if (direction == Direction.Left)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                transform.localScale = new Vector3(-1 * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }    
}
