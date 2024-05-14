using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class SwordThrow : MonoBehaviour, IEnemyAction
    {
        bool isDone = false;
        bool isExecuting = false;
        public bool IsDone { get => isDone; set => isDone = value; }
        public bool IsExecuting { get => isExecuting; set => isExecuting = value; }

        [SerializeField] WeaponConfig weaponConfig = null;
        [SerializeField] Transform handTransform = null;
        [SerializeField] float animationOffsetWaitTime = 0.417f;
        [SerializeField] private uint priority = 1;
        public uint Priority { get => priority; }
        private Animator animator;
        public enum Direction
        {
            Left,
            Right
        }

        Direction direction = Direction.Left;

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
            // プレイヤーの方向を向く
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                isDone = true;
                yield break;
            }
            if (player.transform.position.x < transform.position.x)
            {
                SetDirection(Direction.Left);
            }
            else
            {
                SetDirection(Direction.Right);
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

        /**
         * 行動実行後 再度実行可能にする
         */
        public void Reset()
        {
            isDone = false;
            isExecuting = false;
        }

        public void Stop()
        {
            // 特に何もしない
        }
    }    
}
