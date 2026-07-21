using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Projectiles;

namespace VLCNP.Combat.EnemyAction
{
    /**
     * カマイタチの後方から、プレイヤーの高さを含む複数の竜巻を発生させる。
     */
    public class KamaitachiTornadoAttack : EnemyAction
    {
        [SerializeField]
        private KamaitachiTornadoProjectile tornadoPrefab = null;

        [SerializeField]
        private string triggerName = "attack";

        [SerializeField]
        [Min(1)]
        private int tornadoCount = 6;

        [SerializeField]
        [Min(0f)]
        private float damage = 2f;

        [SerializeField]
        [Min(0f)]
        private float verticalRange = 5f;

        [SerializeField]
        [Min(0f)]
        private float minimumVerticalSpacing = 1f;

        [SerializeField]
        [Min(0f)]
        private float screenEdgePadding = 0.5f;

        [SerializeField]
        [Min(0f)]
        private float spawnDistanceBehind = 10f;

        [SerializeField]
        [Min(0f)]
        private float minimumHorizontalSpacing = 1.5f;

        [SerializeField]
        [Min(0f)]
        private float maximumHorizontalSpacing = 2.5f;

        [SerializeField]
        [Min(0f)]
        private float fallbackLaunchDelay = 0.54f;

        [SerializeField]
        [Min(0f)]
        private float waitAfterAttack = 3.05f;

        [SerializeField]
        [Min(0f)]
        private float fireDistanceThreshold = 12f;

        private readonly List<KamaitachiTornadoProjectile> activeTornadoes =
            new List<KamaitachiTornadoProjectile>();

        private Animator animator;
        private Rigidbody2D rbody;
        private Transform cachedTransform;
        private Transform playerTransform;
        private Coroutine attackCoroutine;
        private bool hasLaunched;
        private float launchedAt;

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
            hasLaunched = false;
            RemoveDestroyedTornadoes();
            StopMotion();

            if (tornadoPrefab == null)
            {
                Debug.LogError($"TornadoPrefab is not set on {gameObject.name}");
                CompleteImmediately();
                return;
            }

            if (!TryGetPlayerTransform(out Transform player) || !IsWithinFireDistance(player))
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
         * 攻撃Animation Clipから呼ぶ竜巻発生タイミング。
         */
        public void OnLaunchAnimationEvent()
        {
            if (!IsExecuting || IsDone || hasLaunched)
                return;

            LaunchTornadoes();
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
                LaunchTornadoes();
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

            attackCoroutine = null;
            IsDone = true;
        }

        private void LaunchTornadoes()
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

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError($"Main Camera is not found for {gameObject.name}");
                CompleteImmediately();
                return;
            }

            SetDirectionToPlayer(player);
            bool shouldMoveLeft = cachedTransform.localScale.x > 0f;
            float cameraDistance = Mathf.Abs(
                mainCamera.transform.position.z - cachedTransform.position.z
            );
            float leftEdge = mainCamera.ViewportToWorldPoint(
                new Vector3(0f, 0.5f, cameraDistance)
            ).x;
            float rightEdge = mainCamera.ViewportToWorldPoint(
                new Vector3(1f, 0.5f, cameraDistance)
            ).x;
            float endX = shouldMoveLeft
                ? leftEdge - screenEdgePadding
                : rightEdge + screenEdgePadding;

            List<float> spawnHeights = CreateSpawnHeights(player.position.y);
            List<float> spawnXPositions = CreateSpawnXPositions(
                shouldMoveLeft,
                spawnHeights.Count
            );
            for (int i = 0; i < spawnHeights.Count; i++)
            {
                Vector3 spawnPosition = new Vector3(
                    spawnXPositions[i],
                    spawnHeights[i],
                    cachedTransform.position.z
                );
                KamaitachiTornadoProjectile tornado = Instantiate(
                    tornadoPrefab,
                    spawnPosition,
                    Quaternion.identity
                );
                tornado.SetDirection(shouldMoveLeft);
                tornado.SetDamage(damage);
                tornado.SetEndX(endX);
                activeTornadoes.Add(tornado);
                tornado.gameObject.SetActive(true);
            }

            hasLaunched = true;
            launchedAt = Time.time;
        }

        private List<float> CreateSpawnXPositions(bool shouldMoveLeft, int count)
        {
            int positionCount = Mathf.Max(1, count);
            float minimumSpacing = Mathf.Min(
                Mathf.Abs(minimumHorizontalSpacing),
                Mathf.Abs(maximumHorizontalSpacing)
            );
            float maximumSpacing = Mathf.Max(
                Mathf.Abs(minimumHorizontalSpacing),
                Mathf.Abs(maximumHorizontalSpacing)
            );
            float behindDirection = shouldMoveLeft ? 1f : -1f;
            float currentX = cachedTransform.position.x
                + behindDirection * Mathf.Abs(spawnDistanceBehind);
            var positions = new List<float>(positionCount) { currentX };

            for (int i = 1; i < positionCount; i++)
            {
                currentX += behindDirection * Random.Range(minimumSpacing, maximumSpacing);
                positions.Add(currentX);
            }

            return positions;
        }

        private List<float> CreateSpawnHeights(float playerY)
        {
            int count = Mathf.Max(1, tornadoCount);
            float range = Mathf.Abs(verticalRange);
            float minimumY = cachedTransform.position.y - range;
            float maximumY = cachedTransform.position.y + range;
            float spacing = Mathf.Abs(minimumVerticalSpacing);
            var heights = new List<float>(count) { playerY };

            const int maximumRandomAttempts = 256;
            int attempts = 0;
            while (heights.Count < count && attempts < maximumRandomAttempts)
            {
                float candidate = Random.Range(minimumY, maximumY);
                if (IsSpacedFromAll(candidate, heights, spacing))
                {
                    heights.Add(candidate);
                }
                attempts++;
            }

            while (heights.Count < count)
            {
                if (!TryFindBestAvailableHeight(minimumY, maximumY, spacing, heights, out float y))
                {
                    Debug.LogWarning(
                        $"Unable to place all tornadoes with {spacing}m spacing on {gameObject.name}"
                    );
                    break;
                }
                heights.Add(y);
            }

            for (int i = heights.Count - 1; i > 0; i--)
            {
                int swapIndex = Random.Range(0, i + 1);
                (heights[i], heights[swapIndex]) = (heights[swapIndex], heights[i]);
            }

            return heights;
        }

        private static bool IsSpacedFromAll(float candidate, List<float> heights, float spacing)
        {
            for (int i = 0; i < heights.Count; i++)
            {
                if (Mathf.Abs(candidate - heights[i]) < spacing)
                    return false;
            }
            return true;
        }

        private static bool TryFindBestAvailableHeight(
            float minimumY,
            float maximumY,
            float spacing,
            List<float> heights,
            out float bestY
        )
        {
            const int sampleCount = 201;
            float bestDistance = -1f;
            bestY = minimumY;

            for (int i = 0; i < sampleCount; i++)
            {
                float candidate = Mathf.Lerp(minimumY, maximumY, (float)i / (sampleCount - 1));
                float nearestDistance = float.PositiveInfinity;
                for (int j = 0; j < heights.Count; j++)
                {
                    nearestDistance = Mathf.Min(
                        nearestDistance,
                        Mathf.Abs(candidate - heights[j])
                    );
                }

                if (nearestDistance > bestDistance)
                {
                    bestDistance = nearestDistance;
                    bestY = candidate;
                }
            }

            return bestDistance >= spacing;
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
            return sqrDistance <= fireDistanceThreshold * fireDistanceThreshold;
        }

        private void CompleteImmediately()
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }

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

        private void RemoveDestroyedTornadoes()
        {
            activeTornadoes.RemoveAll(tornado => tornado == null);
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

            StopMotion();
            hasLaunched = false;
            IsExecuting = false;
            IsDone = true;
        }
    }
}
