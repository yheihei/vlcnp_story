using System.Collections;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class LaunchBulletToPlayer : EnemyAction
    {
        [SerializeField]
        WeaponConfig weaponConfig = null;

        [SerializeField]
        Transform handTransform = null;

        [SerializeField] // 発射後のアニメーションの待ち時間
        float animationOffsetWaitTime = 0.417f;

        private Animator animator;

        public enum Direction
        {
            Left,
            Right,
        }

        Direction direction = Direction.Left;

        private void Awake()
        {
            animator = GetComponent<Animator>();

            if (weaponConfig == null)
                Debug.LogError("WeaponConfig is not set in the inspector for JumpSwordThrow");
            if (handTransform == null)
                Debug.LogError("HandTransform is not set in the inspector for JumpSwordThrow");
            if (animator == null)
                Debug.LogError(
                    "Animator component is missing on the GameObject with JumpSwordThrow"
                );
        }

        public override void Execute()
        {
            if (IsExecuting || IsDone)
                return;
            IsExecuting = true;
            StartCoroutine(Launch());
        }

        private IEnumerator Launch()
        {
            if (weaponConfig == null || !weaponConfig.HasProjectile())
            {
                IsDone = true;
                yield break;
            }

            SetDirectionToPlayer();

            if (animator != null)
            {
                animator.SetTrigger("special1");
            }

            GameObject player = GameObject.FindWithTag("Player");
            if (player == null || handTransform == null)
            {
                IsDone = true;
                yield break;
            }

            // プレイヤーの方向に向ける
            Vector3 playerPosition = player.transform.position;
            Vector3 direction = handTransform.position - playerPosition;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (playerPosition.x > handTransform.position.x)
            {
                angle += 180;
            }
            // ランダム性をもたせる
            angle += Random.Range(-15, 15);
            handTransform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

            bool isLeft = transform.lossyScale.x > 0;
            LaunchProjectiles(isLeft);
            yield return new WaitForSeconds(animationOffsetWaitTime);

            handTransform.rotation = Quaternion.identity;
            IsDone = true;
        }

        private void LaunchProjectiles(bool isLeft)
        {
            if (weaponConfig == null || handTransform == null)
                return;

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
