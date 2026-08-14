using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Projectiles;

namespace VLCNP.Combat.EnemyAction
{
    /**
     * 力強く羽ばたき、画面全体のランダムな位置に多数の竜巻を発生させる。
     */
    public class ScreenWideTornadoBurst : EnemyAction
    {
        [SerializeField]
        private KamaitachiTornadoProjectile tornadoPrefab = null;

        [SerializeField]
        [Tooltip("羽ばたき中にtrueにするAnimator Bool。空なら変更しない")]
        private string flapBoolName = "isMagic";

        [SerializeField]
        [Min(0f)]
        [Tooltip("羽ばたき開始から竜巻発生までの時間")]
        private float flapWindupTime = 0.6f;

        [SerializeField]
        [Min(1)]
        private int tornadoCount = 10;

        [SerializeField]
        [Min(0f)]
        private float damage = 2f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("竜巻の移動速度。0ならその場に留まる想定でPrefabの速度を使う")]
        private float tornadoSpeed = 2.5f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("竜巻同士の最小間隔")]
        private float minimumSpacing = 1.5f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("竜巻を順番に発生させる間隔")]
        private float spawnInterval = 0.06f;

        [SerializeField]
        [Min(0f)]
        private float waitAfterSpawn = 2.5f;

        [SerializeField]
        [Min(0f)]
        private float screenEdgePadding = 1f;

        [SerializeField]
        [Range(0f, 0.4f)]
        [Tooltip("画面端から発生位置を離すViewport比率")]
        private float viewportPadding = 0.1f;

        private const int MaximumPlacementAttempts = 128;

        private readonly List<KamaitachiTornadoProjectile> activeTornadoes =
            new List<KamaitachiTornadoProjectile>();

        private Animator animator;
        private Rigidbody2D rbody;
        private Transform cachedTransform;
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

            if (Camera.main == null)
            {
                Debug.LogError($"Main Camera is not found for {gameObject.name}");
                Complete();
                return;
            }

            SetFlap(true);
            attackCoroutine = StartCoroutine(AttackSequence());
        }

        private IEnumerator AttackSequence()
        {
            yield return new WaitForSeconds(flapWindupTime);

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Complete();
                yield break;
            }

            float cameraDistance = Mathf.Abs(
                mainCamera.transform.position.z - cachedTransform.position.z
            );
            Vector3 minCorner = mainCamera.ViewportToWorldPoint(
                new Vector3(viewportPadding, viewportPadding, cameraDistance)
            );
            Vector3 maxCorner = mainCamera.ViewportToWorldPoint(
                new Vector3(1f - viewportPadding, 1f - viewportPadding, cameraDistance)
            );
            float leftEdge = mainCamera.ViewportToWorldPoint(
                new Vector3(0f, 0.5f, cameraDistance)
            ).x;
            float rightEdge = mainCamera.ViewportToWorldPoint(
                new Vector3(1f, 0.5f, cameraDistance)
            ).x;

            List<Vector2> spawnPositions = CreateSpawnPositions(
                new Vector2(minCorner.x, minCorner.y),
                new Vector2(maxCorner.x, maxCorner.y)
            );

            foreach (Vector2 spawnPosition in spawnPositions)
            {
                if (IsDone)
                    break;

                bool shouldMoveLeft = Random.value < 0.5f;
                KamaitachiTornadoProjectile tornado = Instantiate(
                    tornadoPrefab,
                    new Vector3(spawnPosition.x, spawnPosition.y, cachedTransform.position.z),
                    Quaternion.identity
                );
                tornado.SetDirection(shouldMoveLeft);
                tornado.SetDamage(damage);
                if (tornadoSpeed > 0f)
                {
                    tornado.SetSpeed(tornadoSpeed);
                }
                tornado.SetEndX(
                    shouldMoveLeft
                        ? leftEdge - screenEdgePadding
                        : rightEdge + screenEdgePadding
                );
                activeTornadoes.Add(tornado);
                tornado.gameObject.SetActive(true);

                if (spawnInterval > 0f)
                {
                    yield return new WaitForSeconds(spawnInterval);
                }
            }

            yield return new WaitForSeconds(waitAfterSpawn);

            attackCoroutine = null;
            Complete();
        }

        private List<Vector2> CreateSpawnPositions(Vector2 minCorner, Vector2 maxCorner)
        {
            var positions = new List<Vector2>(tornadoCount);
            float sqrSpacing = minimumSpacing * minimumSpacing;
            int attempts = 0;

            while (positions.Count < tornadoCount && attempts < MaximumPlacementAttempts)
            {
                attempts++;
                Vector2 candidate = new Vector2(
                    Random.Range(minCorner.x, maxCorner.x),
                    Random.Range(minCorner.y, maxCorner.y)
                );

                bool isSpaced = true;
                for (int i = 0; i < positions.Count; i++)
                {
                    if ((candidate - positions[i]).sqrMagnitude < sqrSpacing)
                    {
                        isSpaced = false;
                        break;
                    }
                }

                if (isSpaced)
                {
                    positions.Add(candidate);
                }
            }

            if (positions.Count < tornadoCount)
            {
                Debug.LogWarning(
                    $"Unable to place all tornadoes with {minimumSpacing}m spacing on {gameObject.name}"
                );
            }

            return positions;
        }

        private void SetFlap(bool value)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(flapBoolName))
            {
                animator.SetBool(flapBoolName, value);
            }
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
