using System.Collections;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class LaunchBulletToPlayer2 : EnemyAction
    {
        /**
        LaunchBulletToPlayerの改良版。
        アニメーション発火した後任意の時間待ってから発射する。
        これにより、アニメーションと発射のタイミングを合わせやすくした。
        **/

        [SerializeField]
        WeaponConfig weaponConfig = null;

        [SerializeField]
        Transform launchTransform = null;

        [SerializeField]
        string triggerName = "special1";

        [SerializeField]
        [Min(0f)]
        [Tooltip("プレイヤーとの距離がこの値より大きいと発射せず完了する。0 なら常に発射する。")]
        float fireDistanceThreshold = 8f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("攻撃後に待機する時間")]
        float waitAfterAttack = 2f;

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
            if (launchTransform == null)
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

        public void OnLaunchAnimationEvent()
        {
            SetDirectionToPlayer();

            if (!TryGetPlayer(out GameObject player))
            {
                return;
            }

            if (launchTransform == null)
            {
                IsDone = true;
                return;
            }

            // プレイヤーの方向に向ける
            Vector3 playerPosition = player.transform.position;
            Vector3 direction = launchTransform.position - playerPosition;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (playerPosition.x > launchTransform.position.x)
            {
                angle += 180;
            }
            // ランダム性をもたせる
            angle += Random.Range(-15, 15);
            launchTransform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

            bool isLeft = transform.lossyScale.x > 0;
            LaunchProjectiles(isLeft);
            StartCoroutine(WaitAndComplete());
        }

        private IEnumerator WaitAndComplete()
        {
            yield return new WaitForSeconds(waitAfterAttack);

            launchTransform.rotation = Quaternion.identity;
            IsDone = true;
        }

        private IEnumerator Launch()
        {
            if (weaponConfig == null || !weaponConfig.HasProjectile())
            {
                IsDone = true;
                yield break;
            }

            if (!TryGetPlayer(out GameObject player))
            {
                yield break;
            }

            if (fireDistanceThreshold > 0f)
            {
                float sqrDistance = (player.transform.position - transform.position).sqrMagnitude;
                float sqrThreshold = fireDistanceThreshold * fireDistanceThreshold;
                if (sqrDistance > sqrThreshold)
                {
                    IsDone = true;
                    yield break;
                }
            }

            SetDirectionToPlayer();

            if (animator != null)
            {
                animator.SetTrigger(triggerName);
            }
            else
            {
                IsDone = true;
                yield break;
            }

            // 発射はAnimation Eventで行うため、ここではアニメーション遷移させるだけ
        }

        private void LaunchProjectiles(bool isLeft)
        {
            if (weaponConfig == null || launchTransform == null)
                return;

            weaponConfig.LaunchProjectile(launchTransform, 1, isLeft);
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

        private bool TryGetPlayer(out GameObject player)
        {
            player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                IsDone = true;
                return false;
            }
            return true;
        }
    }
}
