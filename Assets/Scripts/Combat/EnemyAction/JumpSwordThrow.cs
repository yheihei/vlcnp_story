using System.Collections;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class JumpSwordThrow : EnemyAction
    {

        [SerializeField] WeaponConfig weaponConfig = null;
        [SerializeField] Transform handTransform = null;
        [SerializeField] float animationOffsetWaitTime = 0.417f;
        private Animator animator;
        [SerializeField] private float jumpPowerX = 100;
        [SerializeField] private float jumpPowerY = 200;
        private Rigidbody2D rBody;
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
            rBody = GetComponent<Rigidbody2D>();
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
            if (!weaponConfig.HasProjectile())
            {
                IsDone = true;
                yield break;
            }

            damageStun.InvalidStan();

            // プレイヤーの方向を向く
            SetDirectionToPlayer();

            // プレイヤーの方向に飛ぶ
            float _jumpPowerX = this.direction == Direction.Left ? (-1) * jumpPowerX : jumpPowerX;
            rBody.AddForce(new Vector2(_jumpPowerX, jumpPowerY), ForceMode2D.Impulse);

            // 空中で剣を投げる
            if (animator != null)
            {
                animator.SetTrigger("throw");
            }
            // animationが完了するまで待つ調整
            yield return new WaitForSeconds(animationOffsetWaitTime);

            // handTransformの方向をPlayerの方向に向ける
            SetDirectionToPlayer();
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                IsDone = true;
                yield break;
            }
            Vector3 playerPosition = player.transform.position;
            Vector3 direction = handTransform.position - playerPosition;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // プレイヤーが右にいる場合は角度を180度回転させる
            if (playerPosition.x > handTransform.position.x)
            {
                angle += 180;
            }
            handTransform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

            bool isLeft = transform.lossyScale.x > 0;
            weaponConfig.LaunchProjectile(handTransform, 1, isLeft);
            // handTransformをちょっと下に回転させながらもう一発剣を投げる
            handTransform.Rotate(0, 0, 20);
            weaponConfig.LaunchProjectile(handTransform, 1, isLeft);
            // handTransformをちょっと上に回転させながらもう一発剣を投げる
            handTransform.Rotate(0, 0, -40);
            weaponConfig.LaunchProjectile(handTransform, 1, isLeft);
            // handTransformの回転をリセット
            handTransform.rotation = Quaternion.identity;
            IsDone = true;
            damageStun.ValidStan();
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
    }    
}
