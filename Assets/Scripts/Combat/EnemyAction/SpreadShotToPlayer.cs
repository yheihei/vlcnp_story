using System.Collections;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    /**
     * 予備動作の後、プレイヤーを中心に複数の飛翔物を扇状に発射する。
     * Animation Event がない仮アニメーションでも fallbackLaunchDelay 後に発射できる。
     */
    public class SpreadShotToPlayer : EnemyAction
    {
        [SerializeField]
        private WeaponConfig weaponConfig = null;

        [SerializeField]
        private Transform launchTransform = null;

        [SerializeField]
        private string triggerName = "special1";

        [SerializeField]
        [Min(1)]
        private int minProjectileCount = 3;

        [SerializeField]
        [Min(1)]
        private int maxProjectileCount = 4;

        [SerializeField]
        [Min(0f)]
        [Tooltip("扇状に発射する全体の角度")]
        private float totalSpreadAngle = 36f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Animation Event が来なかった場合に自動発射するまでの時間")]
        private float fallbackLaunchDelay = 0.8f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("発射後に停止して後隙を作る時間")]
        private float waitAfterAttack = 2f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("プレイヤーとの距離がこの値より大きいと発射せず完了する。0なら常に発射する。")]
        private float fireDistanceThreshold = 12f;

        private Animator animator;
        private Rigidbody2D rbody;
        private Transform cachedTransform;
        private Transform playerTransform;
        private Quaternion initialLaunchLocalRotation;
        private Coroutine attackCoroutine;
        private bool hasLaunched;
        private float launchedAt;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            rbody = GetComponent<Rigidbody2D>();
            cachedTransform = transform;

            if (launchTransform != null)
            {
                initialLaunchLocalRotation = launchTransform.localRotation;
            }
        }

        public override void Execute()
        {
            if (IsExecuting || IsDone)
                return;

            IsExecuting = true;
            hasLaunched = false;
            StopMotion();

            if (weaponConfig == null || !weaponConfig.HasProjectile())
            {
                Debug.LogError($"WeaponConfig is not set on {gameObject.name}");
                CompleteImmediately();
                return;
            }

            if (launchTransform == null)
            {
                Debug.LogError($"LaunchTransform is not set on {gameObject.name}");
                CompleteImmediately();
                return;
            }

            if (!TryGetPlayerTransform(out Transform player))
            {
                CompleteImmediately();
                return;
            }

            if (!IsWithinFireDistance(player))
            {
                CompleteImmediately();
                return;
            }

            SetDirectionToPlayer(player);
            animator?.SetFloat("vx", 0f);
            if (animator != null && !string.IsNullOrWhiteSpace(triggerName))
            {
                animator.SetTrigger(triggerName);
            }

            attackCoroutine = StartCoroutine(AttackSequence());
        }

        /**
         * 攻撃Animation Clipから呼ぶ発射タイミング。
         */
        public void OnLaunchAnimationEvent()
        {
            if (!IsExecuting || IsDone || hasLaunched)
                return;

            LaunchSpread();
        }

        private IEnumerator AttackSequence()
        {
            float elapsedTime = 0f;
            while (!hasLaunched && !IsDone && elapsedTime < fallbackLaunchDelay)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (IsDone)
            {
                attackCoroutine = null;
                yield break;
            }

            if (!hasLaunched)
            {
                LaunchSpread();
            }

            if (!hasLaunched || IsDone)
            {
                attackCoroutine = null;
                yield break;
            }

            while (Time.time - launchedAt < waitAfterAttack)
            {
                yield return null;
            }

            ResetLaunchRotation();
            attackCoroutine = null;
            IsDone = true;
        }

        private void LaunchSpread()
        {
            if (
                hasLaunched
                || !TryGetPlayerTransform(out Transform player)
                || !IsWithinFireDistance(player)
            )
            {
                CompleteImmediately();
                return;
            }

            SetDirectionToPlayer(player);

            Vector2 aimDirection = player.position - launchTransform.position;
            if (aimDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                aimDirection = cachedTransform.localScale.x > 0f ? Vector2.left : Vector2.right;
            }

            float baseAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            bool isLeft = aimDirection.x < 0f;
            int minimum = Mathf.Max(1, Mathf.Min(minProjectileCount, maxProjectileCount));
            int maximum = Mathf.Max(minimum, Mathf.Max(minProjectileCount, maxProjectileCount));
            int projectileCount = Random.Range(minimum, maximum + 1);
            float spreadAngle = Mathf.Abs(totalSpreadAngle);

            for (int i = 0; i < projectileCount; i++)
            {
                float ratio = projectileCount == 1 ? 0.5f : (float)i / (projectileCount - 1);
                float angleOffset = Mathf.Lerp(-spreadAngle * 0.5f, spreadAngle * 0.5f, ratio);
                float movementAngle = baseAngle + angleOffset;
                float launchAngle = isLeft ? movementAngle + 180f : movementAngle;
                launchTransform.rotation = Quaternion.Euler(0f, 0f, launchAngle);

                // 左向きProjectileはローカル-Xへ進むため、発射角を180度補正する。
                weaponConfig.LaunchProjectile(launchTransform, 1, isLeft);
            }

            ResetLaunchRotation();
            hasLaunched = true;
            launchedAt = Time.time;
        }

        private void SetDirectionToPlayer(Transform player)
        {
            Vector3 localScale = cachedTransform.localScale;
            localScale.x = player.position.x < cachedTransform.position.x
                ? Mathf.Abs(localScale.x)
                : -Mathf.Abs(localScale.x);
            cachedTransform.localScale = localScale;
        }

        private bool TryGetPlayerTransform(out Transform player)
        {
            if (
                playerTransform == null
                || !playerTransform.gameObject.activeInHierarchy
                || !playerTransform.CompareTag("Player")
            )
            {
                GameObject playerObject = GameObject.FindWithTag("Player");
                playerTransform = playerObject != null ? playerObject.transform : null;
            }

            player = playerTransform;
            return player != null;
        }

        private bool IsWithinFireDistance(Transform player)
        {
            if (fireDistanceThreshold <= 0f)
                return true;

            float sqrDistance = (player.position - cachedTransform.position).sqrMagnitude;
            float sqrThreshold = fireDistanceThreshold * fireDistanceThreshold;
            return sqrDistance <= sqrThreshold;
        }

        private void CompleteImmediately()
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }

            ResetLaunchRotation();
            StopMotion();
            IsDone = true;
        }

        private void ResetLaunchRotation()
        {
            if (launchTransform != null)
            {
                launchTransform.localRotation = initialLaunchLocalRotation;
            }
        }

        private void StopMotion()
        {
            if (rbody != null)
            {
                rbody.velocity = Vector2.zero;
            }
        }

        public override void Stop()
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }

            ResetLaunchRotation();
            StopMotion();
            hasLaunched = false;
            IsExecuting = false;
            IsDone = true;
        }
    }
}
