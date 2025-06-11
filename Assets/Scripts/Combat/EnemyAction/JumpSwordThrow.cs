using System.Collections;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class JumpSwordThrow : EnemyAction
    {
        [SerializeField]
        WeaponConfig weaponConfig = null;

        [SerializeField]
        Transform handTransform = null;

        [SerializeField]
        float animationOffsetWaitTime = 0.417f;

        [SerializeField]
        private float jumpPowerX = 100;

        [SerializeField]
        private float jumpPowerY = 200;

        private Animator animator;
        private Rigidbody2D rBody;
        private DamageStun damageStun;

        public enum Direction
        {
            Left,
            Right,
        }

        Direction direction = Direction.Left;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            rBody = GetComponent<Rigidbody2D>();
            damageStun = GetComponent<DamageStun>();

            if (weaponConfig == null)
                Debug.LogError("WeaponConfig is not set in the inspector for JumpSwordThrow");
            if (handTransform == null)
                Debug.LogError("HandTransform is not set in the inspector for JumpSwordThrow");
            if (animator == null)
                Debug.LogError(
                    "Animator component is missing on the GameObject with JumpSwordThrow"
                );
            if (rBody == null)
                Debug.LogError(
                    "Rigidbody2D component is missing on the GameObject with JumpSwordThrow"
                );
            if (damageStun == null)
                Debug.LogError(
                    "DamageStun component is missing on the GameObject with JumpSwordThrow"
                );
        }

        public override void Execute()
        {
            if (IsExecuting || IsDone)
                return;
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

            if (damageStun != null)
                damageStun.InvalidStan();

            SetDirectionToPlayer();

            // 1s preJump animation
            animator?.SetBool("isPreJump", true);
            yield return new WaitForSeconds(0.8f);
            animator?.SetBool("isPreJump", false);
            yield return new WaitForSeconds(0.2f);

            if (rBody != null)
            {
                float _jumpPowerX =
                    this.direction == Direction.Left ? (-1) * jumpPowerX : jumpPowerX;
                rBody.AddForce(new Vector2(_jumpPowerX, jumpPowerY), ForceMode2D.Impulse);
            }

            animator?.SetTrigger("throw");

            yield return new WaitForSeconds(animationOffsetWaitTime);

            SetDirectionToPlayer();
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null || handTransform == null)
            {
                IsDone = true;
                yield break;
            }

            Vector3 playerPosition = player.transform.position;
            Vector3 direction = handTransform.position - playerPosition;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (playerPosition.x > handTransform.position.x)
            {
                angle += 180;
            }
            handTransform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

            bool isLeft = transform.lossyScale.x > 0;
            LaunchProjectiles(isLeft);

            handTransform.rotation = Quaternion.identity;
            IsDone = true;
            if (damageStun != null)
                damageStun.ValidStan();
        }

        private void LaunchProjectiles(bool isLeft)
        {
            if (weaponConfig == null || handTransform == null)
                return;

            weaponConfig.LaunchProjectile(handTransform, 1, isLeft);
            handTransform.Rotate(0, 0, 20);
            weaponConfig.LaunchProjectile(handTransform, 1, isLeft);
            handTransform.Rotate(0, 0, -40);
            weaponConfig.LaunchProjectile(handTransform, 1, isLeft);
        }

        private void SetDirectionToPlayer()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
                return;
            SetDirection(
                player.transform.position.x < transform.position.x
                    ? Direction.Left
                    : Direction.Right
            );
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (animator == null || other.tag != "Ground")
                return;
            animator.SetBool("isGround", true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (animator == null || other.tag != "Ground")
                return;
            animator.SetBool("isGround", false);
        }

        public void SetDirection(Direction _direction)
        {
            direction = _direction;
            UpdateCharacterDirection();
        }

        private void UpdateCharacterDirection()
        {
            float scaleX = Mathf.Abs(transform.localScale.x);
            transform.localScale = new Vector3(
                direction == Direction.Left ? scaleX : -scaleX,
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }
}
