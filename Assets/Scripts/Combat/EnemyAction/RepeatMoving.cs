using System;
using System.Collections;
using UnityEngine;
using VLCNP.Movement;

namespace VLCNP.Combat.EnemyAction
{
    /**
     * 足場があるところを左右に繰り返し移動する
     */
    public class RepeatMoving : EnemyAction
    {
        [SerializeField]
        float speed = 4;

        [SerializeField]
        private FrontCollisionDetector frontCollisionDetector = null;

        [SerializeField]
        private FrontCollisionDetector gakeCollisionDetector = null;

        [SerializeField]
        private float moveTimeout = 4f;

        Rigidbody2D rbody;
        Animator animator;
        float vx = 0;
        bool isGround = false;

        public enum Direction
        {
            Left,
            Right,
        }

        Direction direction = Direction.Left;

        private void Awake()
        {
            rbody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        public override void Execute()
        {
            if (IsExecuting)
                return;
            if (IsDone)
                return;
            if (!isGround)
                return;
            IsExecuting = true;
            StartCoroutine(Move(moveTimeout));
        }

        private IEnumerator Move(float _moveTimeout)
        {
            float elapsedTime = 0f;
            // 向いてる方向で移動（速度修正を適用）
            float modifiedSpeed = GetModifiedSpeed(speed);
            float _speed = direction == Direction.Left ? -modifiedSpeed : modifiedSpeed;
            while (elapsedTime <= _moveTimeout)
            {
                UpdateMoveSpeed(_speed);
                // 経過時間加算
                elapsedTime += Time.deltaTime;

                // ガケに来たら方向転換
                if (gakeCollisionDetector != null && !gakeCollisionDetector.IsColliding)
                {
                    SetDirection(direction == Direction.Left ? Direction.Right : Direction.Left);
                    float newModifiedSpeed = GetModifiedSpeed(speed);
                    _speed = direction == Direction.Left ? -newModifiedSpeed : newModifiedSpeed;
                    UpdateMoveSpeed(_speed);
                    yield return null;
                }
                // 前方で壁にぶつかったら方向転換
                if (frontCollisionDetector != null && frontCollisionDetector.IsColliding)
                {
                    SetDirection(direction == Direction.Left ? Direction.Right : Direction.Left);
                    float newModifiedSpeed = GetModifiedSpeed(speed);
                    _speed = direction == Direction.Left ? -newModifiedSpeed : newModifiedSpeed;
                    UpdateMoveSpeed(_speed);
                    yield return null;
                }
                yield return null;
            }
            UpdateMoveSpeed(0);
            IsDone = true;
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!collision.gameObject.CompareTag("Ground"))
                return;
            isGround = true;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.gameObject.CompareTag("Ground"))
                return;
            isGround = false;
        }

        private void UpdateMoveSpeed(float _vx)
        {
            vx = _vx;
            rbody.velocity = new Vector2(vx, rbody.velocity.y);
            animator?.SetFloat("vx", Mathf.Abs(vx));
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
                transform.localScale = new Vector3(
                    Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
            else
            {
                transform.localScale = new Vector3(
                    -1 * Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
        }
    }
}
