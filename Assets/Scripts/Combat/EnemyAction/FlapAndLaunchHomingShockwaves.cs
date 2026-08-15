using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Projectiles;

namespace VLCNP.Combat.EnemyAction
{
    /**
     * 激しく羽ばたき、自転する追尾衝撃波を8方向へ一斉に発生させる。
     */
    public class FlapAndLaunchHomingShockwaves : EnemyAction
    {
        [SerializeField]
        private NarukamiHomingShockwave shockwavePrefab = null;

        [SerializeField]
        [Tooltip("羽ばたき中にtrueにするAnimator Bool。空なら変更しない")]
        private string flapBoolName = "isMagic";

        [SerializeField]
        [Min(0f)]
        [Tooltip("羽ばたき開始から発射までの時間")]
        private float launchDelay = 0.6f;

        [SerializeField]
        [Min(1)]
        private int shockwaveCount = 8;

        [SerializeField]
        [Tooltip("1発目の発射角度[deg]。0=右方向")]
        private float firstAngleOffset = 0f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("自分の中心から発射位置までの距離")]
        private float spawnRadius = 0.8f;

        [SerializeField]
        [Min(0f)]
        private float damage = 1f;

        [SerializeField]
        [Min(0f)]
        private float waitAfterLaunch = 1.5f;

        private readonly List<NarukamiHomingShockwave> activeShockwaves =
            new List<NarukamiHomingShockwave>();

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

            if (TryGetPlayerTransform(out Transform player))
            {
                SetDirectionToPlayer(player);
            }

            SetFlap(true);
            attackCoroutine = StartCoroutine(AttackSequence());
        }

        private IEnumerator AttackSequence()
        {
            yield return new WaitForSeconds(launchDelay);

            LaunchShockwaves();

            yield return new WaitForSeconds(waitAfterLaunch);

            attackCoroutine = null;
            Complete();
        }

        private void LaunchShockwaves()
        {
            for (int i = 0; i < shockwaveCount; i++)
            {
                float angle = firstAngleOffset + i * (360f / shockwaveCount);
                Vector2 direction = new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad)
                );
                Vector3 spawnPosition =
                    cachedTransform.position + (Vector3)(direction * spawnRadius);

                NarukamiHomingShockwave shockwave = Instantiate(
                    shockwavePrefab,
                    spawnPosition,
                    Quaternion.identity
                );
                shockwave.Launch(direction);
                shockwave.SetDamage(damage);
                activeShockwaves.Add(shockwave);
            }
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
