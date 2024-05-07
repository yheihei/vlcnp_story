using System.Collections;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class JumpSwordThrow : MonoBehaviour, IEnemyAction
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
        [SerializeField] private float jumpPowerX = 100;
        [SerializeField] private float jumpPowerY = 200;
        private Rigidbody2D rigidbody2D;

        public enum Direction
        {
            Left,
            Right
        }

        Direction direction = Direction.Left;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            rigidbody2D = GetComponent<Rigidbody2D>();
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

            // プレイヤーが左にいる場合は左を向く
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

            // プレイヤーの方向に飛んでくる
            float _jumpPowerX = direction == Direction.Left ? (-1) * jumpPowerX : jumpPowerX;
            rigidbody2D.AddForce(new Vector2(_jumpPowerX, jumpPowerY), ForceMode2D.Impulse);

            // 空中で剣を投げる
            if (animator != null)
            {
                animator.SetTrigger("throw");
            }
            // animationが完了するまで待つ調整
            yield return new WaitForSeconds(animationOffsetWaitTime);
            bool isLeft = transform.lossyScale.x <= 0;

            // handTransformの方向をPlayerの方向に向ける
            if (player != null)
            {
                Vector3 playerPosition = player.transform.position;
                Vector3 direction = playerPosition - handTransform.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                // プレイヤーが右にいる場合は角度を180度回転させる
                if (playerPosition.x > handTransform.position.x)
                {
                    angle += 180;
                }
                handTransform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            }

            weaponConfig.LaunchProjectile(handTransform, 1, isLeft);
            // handTransformをちょっと下に回転させながらもう一発剣を投げる
            handTransform.Rotate(0, 0, 20);
            weaponConfig.LaunchProjectile(handTransform, 1, isLeft);
            // handTransformをちょっと上に回転させながらもう一発剣を投げる
            handTransform.Rotate(0, 0, -40);
            weaponConfig.LaunchProjectile(handTransform, 1, isLeft);
            // handTransformの回転をリセット
            handTransform.rotation = Quaternion.identity;
            isDone = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // GroundタグであればアニメーションをisGround=Trueに
            if (other.tag == "Ground")
            {
                if (animator != null)
                {
                    animator.SetBool("isGround", true);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // GroundタグであればアニメーションをisGround=Falseに
            if (other.tag == "Ground")
            {
                if (animator != null)
                {
                    animator.SetBool("isGround", false);
                }
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
    }    
}
