using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Projectiles;

namespace VLCNP.Combat.EnemyAction
{
    /**
     * 羽ばたきAnimationを再生しながら、プレイヤーへ向かって進む竜巻を発生させる。
     */
    public class LaunchTornadoesToPlayer : EnemyAction
    {
        [SerializeField]
        private KamaitachiTornadoProjectile tornadoPrefab = null;

        [SerializeField]
        [Tooltip("羽ばたき中にtrueにするAnimator Bool。空なら変更しない")]
        private string flapBoolName = "isMagic";

        [SerializeField]
        [Min(0f)]
        [Tooltip("羽ばたき開始から竜巻発射までの時間")]
        private float launchDelay = 0.5f;

        [SerializeField]
        [Min(1)]
        private int tornadoCount = 3;

        [SerializeField]
        [Min(0f)]
        private float damage = 1f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("プレイヤー高さを基準にした竜巻同士の縦間隔")]
        private float verticalSpacing = 1.3f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("自分からプレイヤー方向への発生位置オフセット")]
        private float spawnOffsetX = 1.2f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("プレイヤーから最低この距離だけ離れた位置で生成する(近すぎて避けられないのを防ぐ)")]
        private float minSpawnDistanceFromPlayer = 4f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("竜巻の速度。0ならPrefabの速度を使う")]
        private float projectileSpeed = 0f;

        [SerializeField]
        [Min(0f)]
        private float waitAfterLaunch = 1.2f;

        [SerializeField]
        [Min(0f)]
        private float screenEdgePadding = 1f;

        private readonly List<KamaitachiTornadoProjectile> activeTornadoes =
            new List<KamaitachiTornadoProjectile>();

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
            activeTornadoes.RemoveAll(tornado => tornado == null);
            StopMotion();

            if (tornadoPrefab == null)
            {
                Debug.LogError($"TornadoPrefab is not set on {gameObject.name}");
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
            yield return new WaitForSeconds(launchDelay);

            if (!TryGetPlayerTransform(out Transform player))
            {
                Complete();
                yield break;
            }

            LaunchTornadoes(player);
            yield return new WaitForSeconds(waitAfterLaunch);

            attackCoroutine = null;
            Complete();
        }

        private void LaunchTornadoes(Transform player)
        {
            SetDirectionToPlayer(player);
            bool shouldMoveLeft = player.position.x < cachedTransform.position.x;
            float directionX = shouldMoveLeft ? -1f : 1f;
            float endX = GetScreenEndX(shouldMoveLeft);

            float spawnX = cachedTransform.position.x + directionX * spawnOffsetX;
            // プレイヤーに近すぎる場合は、進行方向の手前側へ最低距離だけ離す
            if (Mathf.Abs(spawnX - player.position.x) < minSpawnDistanceFromPlayer)
            {
                spawnX = player.position.x - directionX * minSpawnDistanceFromPlayer;
            }

            for (int i = 0; i < tornadoCount; i++)
            {
                // プレイヤー高さを起点に上下交互へ広げる
                int step = (i + 1) / 2;
                float sign = i % 2 == 1 ? 1f : -1f;
                float spawnY = player.position.y + (i == 0 ? 0f : sign * step * verticalSpacing);
                Vector3 spawnPosition = new Vector3(
                    spawnX,
                    spawnY,
                    cachedTransform.position.z
                );

                KamaitachiTornadoProjectile tornado = Instantiate(
                    tornadoPrefab,
                    spawnPosition,
                    Quaternion.identity
                );
                tornado.SetDirection(shouldMoveLeft);
                tornado.SetDamage(damage);
                if (projectileSpeed > 0f)
                {
                    tornado.SetSpeed(projectileSpeed);
                }
                tornado.SetEndX(endX);
                activeTornadoes.Add(tornado);
                tornado.gameObject.SetActive(true);
            }
        }

        private float GetScreenEndX(bool shouldMoveLeft)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                float fallbackDistance = 20f;
                return cachedTransform.position.x
                    + (shouldMoveLeft ? -fallbackDistance : fallbackDistance);
            }

            float cameraDistance = Mathf.Abs(
                mainCamera.transform.position.z - cachedTransform.position.z
            );
            float viewportX = shouldMoveLeft ? 0f : 1f;
            float edgeX = mainCamera.ViewportToWorldPoint(
                new Vector3(viewportX, 0.5f, cameraDistance)
            ).x;
            return shouldMoveLeft ? edgeX - screenEdgePadding : edgeX + screenEdgePadding;
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

            for (int i = 0; i < activeTornadoes.Count; i++)
            {
                if (activeTornadoes[i] != null)
                {
                    Destroy(activeTornadoes[i].gameObject);
                }
            }
            activeTornadoes.Clear();

            SetFlap(false);
            StopMotion();
            IsExecuting = false;
            IsDone = true;
        }
    }
}
