using System.Collections;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class JumpAction : MonoBehaviour, IEnemyAction
    {
        bool isDone = false;
        bool isExecuting = false;
        public bool IsDone { get => isDone; set => isDone = value; }
        public bool IsExecuting { get => isExecuting; set => isExecuting = value; }
        [SerializeField] private uint priority = 1;
        public uint Priority { get => priority; }
        private Animator animator;
        [SerializeField] private float jumpPowerX = 100;
        [SerializeField] private float jumpPowerY = 200;
        private Rigidbody2D rBody;

        public enum Direction
        {
            Left,
            Right
        }

        Direction direction = Direction.Left;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            rBody = GetComponent<Rigidbody2D>();
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

            // プレイヤーの方向を向く
            SetDirectionToPlayer();

            // プレイヤーの方向に飛ぶ
            float _jumpPowerX = this.direction == Direction.Left ? (-1) * jumpPowerX : jumpPowerX;
            rBody.AddForce(new Vector2(_jumpPowerX, jumpPowerY), ForceMode2D.Impulse);
            // 一瞬待つ
            yield return new WaitForSeconds(1f);
            // 着地するまで待つ
            yield return new WaitUntil(() => animator.GetBool("isGround"));
            isDone = true;
        }

        private void SetDirectionToPlayer()
        {
            // プレイヤーの方向を向く
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return;
            if (player.transform.position.x < transform.position.x)
            {
                SetDirection(Direction.Left);
            }
            else
            {
                SetDirection(Direction.Right);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (animator == null) return;
            if (other.tag == "Ground")
            {
                animator.SetBool("isGround", true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (animator == null) return;
            if (other.tag == "Ground")
            {
                animator.SetBool("isGround", false);
            }
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
            // 何もしない
        }
    }    
}
