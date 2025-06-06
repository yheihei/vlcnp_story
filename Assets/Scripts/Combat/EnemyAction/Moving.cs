using System;
using System.Collections;
using UnityEngine;
using VLCNP.Movement;

namespace VLCNP.Combat.EnemyAction
{
    public class Moving : EnemyAction
    {
        [SerializeField] float speed = 4;
        [SerializeField] float moveX = 0;
        [SerializeField] bool keepDirection = false;
        [SerializeField] private bool isTowardPlayer = true;
        [SerializeField] private FrontCollisionDetector frontCollisionDetector = null;
        [SerializeField] private float moveTimeout = 4f;

        Rigidbody2D rbody;
        Animator animator;
        float vx = 0;

        public enum Direction
        {
            Left,
            Right
        }

        Direction direction = Direction.Left;

        private void Awake() {
            rbody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        public override void Execute()
        {
            if (IsExecuting) return;
            if (IsDone) return;
            IsExecuting = true;
            Vector3 position = transform.position;
            float _moveX = moveX;
            if (isTowardPlayer)
            {
                GameObject player = GameObject.FindWithTag("Player");
                // プレイヤーが左にいる場合は左に移動
                if (player != null)
                {
                    _moveX = player.transform.position.x < position.x ? -moveX : moveX;
                }
            }
            Vector3 destinationPosition = new Vector3(position.x + _moveX, position.y, position.z);
            StartCoroutine(MoveToPosition(destinationPosition, moveTimeout, keepDirection));
        }

        private IEnumerator MoveToPosition(Vector3 position, float timeout = 0, bool keepDirection = false)
        {
            // プレイヤーの位置が指定のx位置より左にある場合は左を向く
            if (!keepDirection)
            {
                if (position.x < transform.position.x)
                {
                    SetDirection(Direction.Left);
                }
                else
                {
                    SetDirection(Direction.Right);
                }
            }
            // 経過時間を格納する変数
            float elapsedTime = 0;
            // プレイヤーの位置と指定のx位置が特定の値以内になるまでループ
            while (Mathf.Abs(position.x - transform.position.x) > 0.05f)
            {
                if (IsDone) break;
                // 経過時間加算
                elapsedTime += Time.deltaTime;
                // タイムアウト値になったらループを抜ける
                if (timeout > 0 && elapsedTime > timeout)
                {
                    break;
                }
                // 前方に障害物がある場合はループを抜ける
                if (frontCollisionDetector != null && frontCollisionDetector.IsColliding)
                {
                    break;
                }
                // 指定の位置に向かって移動（速度修正を適用）
                float modifiedSpeed = GetModifiedSpeed(speed);
                UpdateMoveSpeed(position.x < transform.position.x ? -modifiedSpeed : modifiedSpeed);
                yield return null;
            }
            UpdateMoveSpeed(0);
            IsDone = true;
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
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                transform.localScale = new Vector3(-1 * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }
}
