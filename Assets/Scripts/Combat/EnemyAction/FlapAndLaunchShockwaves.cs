using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    /**
     * 羽ばたきAnimationを再生しながら、プレイヤーへ向けて衝撃波を複数回発射する。
     */
    public class FlapAndLaunchShockwaves : EnemyAction
    {
        [SerializeField]
        private Projectile shockwavePrefab = null;

        [SerializeField]
        [Tooltip("羽ばたき中にtrueにするAnimator Bool。空なら変更しない")]
        private string flapBoolName = "isMagic";

        [SerializeField]
        [Min(0f)]
        [Tooltip("羽ばたき開始から1発目の発射までの時間")]
        private float firstShotDelay = 0.5f;

        [SerializeField]
        [Min(1)]
        [Tooltip("羽ばたき(発射)の回数")]
        private int shotCount = 3;

        [SerializeField]
        [Min(0f)]
        [Tooltip("発射と発射の間隔")]
        private float shotInterval = 0.6f;

        [SerializeField]
        [Min(0f)]
        private float damage = 2f;

        [SerializeField]
        [Tooltip("自分の位置からの発射オフセット。xはプレイヤー方向へ反転される")]
        private Vector2 launchOffset = new Vector2(0.8f, 0f);

        [SerializeField]
        [Min(0f)]
        private float waitAfterLaunch = 0.8f;

        private readonly List<Projectile> activeShockwaves = new List<Projectile>();

        private Animator animator;
        private Rigidbody2D rbody;
        private Transform cachedTransform;
        private Transform playerTransform;
        private Coroutine attackCoroutine;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            rbody = GetComponent<Rigidbody2D>();
            cachedTransform = transform;
        }

        public override void Execute()
        {
            if (IsExecuting || IsDone)
                return;

            IsExecuting = true;
            activeShockwaves.RemoveAll(shockwave => shockwave == null);
            StopMotion();

            if (shockwavePrefab == null)
            {
                Debug.LogError($"ShockwavePrefab is not set on {gameObject.name}");
                Complete();
                return;
            }

            if (!TryGetPlayerTransform(out Transform player))
            {
                Complete();
                return;
            }

            SetDirectionToPlayer(player);
            SetFlap(true);
            attackCoroutine = StartCoroutine(AttackSequence());
        }

        private IEnumerator AttackSequence()
        {
            yield return new WaitForSeconds(firstShotDelay);

            for (int i = 0; i < shotCount; i++)
            {
                if (i > 0)
                {
                    yield return new WaitForSeconds(shotInterval);
                }

                if (!TryGetPlayerTransform(out Transform player))
                {
                    break;
                }

                SetDirectionToPlayer(player);
                LaunchShockwave(player);
            }

            yield return new WaitForSeconds(waitAfterLaunch);

            attackCoroutine = null;
            Complete();
        }

        private void LaunchShockwave(Transform player)
        {
            bool isLeft = player.position.x < cachedTransform.position.x;
            float directionX = isLeft ? -1f : 1f;
            Vector3 spawnPosition = cachedTransform.position
                + new Vector3(directionX * launchOffset.x, launchOffset.y, 0f);

            Vector2 toPlayer = player.position - spawnPosition;
            if (toPlayer.sqrMagnitude < 0.0001f)
            {
                toPlayer = new Vector2(directionX, 0f);
            }

            float angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            // 左向きのときはProjectileがローカル-X方向へ進むため、逆向きの回転で相殺する
            if (isLeft)
            {
                angle += 180f;
            }

            Projectile shockwave = Instantiate(
                shockwavePrefab,
                spawnPosition,
                Quaternion.Euler(0f, 0f, angle)
            );
            shockwave.SetDirection(isLeft);
            shockwave.SetDamage(damage);
            activeShockwaves.Add(shockwave);
        }

        private void SetDirectionToPlayer(Transform player)
        {
            Vector3 localScale = cachedTransform.localScale;
            localScale.x = player.position.x < cachedTransform.position.x
                ? Mathf.Abs(localScale.x)
                : -Mathf.Abs(localScale.x);
            cachedTransform.localScale = localScale;
        }

        private void SetFlap(bool value)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(flapBoolName))
            {
                animator.SetBool(flapBoolName, value);
            }
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

        private void Complete()
        {
            SetFlap(false);
            StopMotion();
            IsDone = true;
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

            for (int i = 0; i < activeShockwaves.Count; i++)
            {
                if (activeShockwaves[i] != null)
                {
                    Destroy(activeShockwaves[i].gameObject);
                }
            }
            activeShockwaves.Clear();

            SetFlap(false);
            StopMotion();
            IsExecuting = false;
            IsDone = true;
        }
    }
}
