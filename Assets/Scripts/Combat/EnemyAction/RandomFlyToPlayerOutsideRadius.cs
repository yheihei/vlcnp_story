using System.Collections;
using UnityEngine;
using VLCNP.Movement;

namespace VLCNP.Combat.EnemyAction
{
    /**
     * プレイヤーの指定半径内へ入らないように距離を保ちながら飛行する。
     * 移動先と移動中の位置を補正し、プレイヤー側から接近された場合も半径外へ退避する。
     */
    public class RandomFlyToPlayerOutsideRadius : EnemyAction
    {
        [SerializeField]
        private float speed = 6f;

        [SerializeField]
        private float directionMultiplier = 4f;

        [SerializeField]
        private Vector2 offset = Vector2.zero;

        [SerializeField]
        [Tooltip("この距離以内にいる場合はプレイヤーから離れる方向へ移動する")]
        private float maxApproachDistance = 2f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("プレイヤーを中心とする、この半径内には侵入しない")]
        private float minimumDistanceFromPlayer = 2.5f;

        [SerializeField]
        private FrontCollisionDetector frontCollisionDetector = null;

        [SerializeField]
        private float moveTimeout = 2f;

        [SerializeField]
        [Tooltip("プレイヤー周囲のランダム地点を移動先にする")]
        private bool useRandomPositionAroundPlayer = false;

        [SerializeField]
        [Tooltip("プレイヤー位置から見たランダム移動先の最小オフセット")]
        private Vector2 randomOffsetMin = new Vector2(-4f, 1f);

        [SerializeField]
        [Tooltip("プレイヤー位置から見たランダム移動先の最大オフセット")]
        private Vector2 randomOffsetMax = new Vector2(4f, 3f);

        [SerializeField]
        [Min(0.01f)]
        private float arrivalDistance = 0.15f;

        private Rigidbody2D rbody;
        private Animator animator;
        private Transform cachedTransform;
        private Transform playerTransform;
        private Coroutine moveCoroutine;
        private float vx;
        private float vy;
        private readonly WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

        public enum Direction
        {
            Left,
            Right,
        }

        private Direction direction = Direction.Left;

        private void Awake()
        {
            rbody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            cachedTransform = transform;
        }

        public override void Execute()
        {
            if (IsExecuting || IsDone)
                return;

            IsExecuting = true;

            if (rbody == null)
            {
                Debug.LogError($"Rigidbody2D component not found on {gameObject.name}");
                IsDone = true;
                return;
            }

            if (!TryGetPlayerTransform(out Transform player))
            {
                IsDone = true;
                return;
            }

            Vector3 currentPosition = cachedTransform.position;
            Vector3 destinationPosition = useRandomPositionAroundPlayer
                ? GetRandomPositionAroundPlayer(player)
                : GetLegacyDestinationPosition(player, currentPosition);

            Vector2 fallbackDirection = (Vector2)(currentPosition - player.position);
            Vector2 safeDestination = ClampOutsidePlayerRadius(
                destinationPosition,
                player.position,
                minimumDistanceFromPlayer,
                fallbackDirection
            );
            destinationPosition = new Vector3(
                safeDestination.x,
                safeDestination.y,
                currentPosition.z
            );

            moveCoroutine = StartCoroutine(MoveToPosition(destinationPosition, player, moveTimeout));
        }

        private Vector3 GetRandomPositionAroundPlayer(Transform player)
        {
            float minX = Mathf.Min(randomOffsetMin.x, randomOffsetMax.x);
            float maxX = Mathf.Max(randomOffsetMin.x, randomOffsetMax.x);
            float minY = Mathf.Min(randomOffsetMin.y, randomOffsetMax.y);
            float maxY = Mathf.Max(randomOffsetMin.y, randomOffsetMax.y);

            return new Vector3(
                player.position.x + Random.Range(minX, maxX),
                player.position.y + Random.Range(minY, maxY),
                cachedTransform.position.z
            );
        }

        private Vector3 GetLegacyDestinationPosition(Transform player, Vector3 position)
        {
            Vector3 playerDirection = player.position - position;
            if (playerDirection.sqrMagnitude < maxApproachDistance * maxApproachDistance)
            {
                playerDirection *= -1f;
            }
            playerDirection.Normalize();
            playerDirection *= directionMultiplier;

            Vector3 destinationPosition = new Vector3(
                position.x + playerDirection.x,
                position.y + playerDirection.y + offset.y,
                position.z
            );
            float randomAngle = playerDirection.y < 0f
                ? Random.Range(0f, 15f)
                : Random.Range(-15f, 0f);

            return Quaternion.Euler(0f, 0f, randomAngle) * (destinationPosition - position)
                + position;
        }

        private IEnumerator MoveToPosition(Vector3 position, Transform player, float timeout)
        {
            SetDirection(
                position.x < cachedTransform.position.x ? Direction.Left : Direction.Right
            );

            float elapsedTime = 0f;
            float sqrArrivalDistance = useRandomPositionAroundPlayer
                ? arrivalDistance * arrivalDistance
                : 0.01f;

            while (true)
            {
                if (IsDone || player == null)
                    break;

                Vector2 playerPosition = player.position;
                Vector2 currentPosition = rbody.position;
                Vector2 fallbackDirection = currentPosition - playerPosition;
                Vector2 safeCurrentPosition = ClampOutsidePlayerRadius(
                    currentPosition,
                    playerPosition,
                    minimumDistanceFromPlayer,
                    fallbackDirection
                );
                if (safeCurrentPosition != currentPosition)
                {
                    rbody.position = safeCurrentPosition;
                    currentPosition = safeCurrentPosition;
                }

                if ((currentPosition - (Vector2)position).sqrMagnitude <= sqrArrivalDistance)
                    break;

                elapsedTime += Time.fixedDeltaTime;
                if (timeout > 0f && elapsedTime > timeout)
                    break;

                if (frontCollisionDetector != null && frontCollisionDetector.IsColliding)
                    break;

                float modifiedSpeed = GetModifiedSpeed(speed);
                Vector2 velocity = GetMoveVelocity(position, currentPosition, modifiedSpeed);
                velocity = ClampVelocityOutsidePlayerRadius(
                    currentPosition,
                    velocity,
                    playerPosition,
                    minimumDistanceFromPlayer,
                    Time.fixedDeltaTime
                );

                UpdateMoveSpeed(velocity.x, velocity.y);
                yield return waitForFixedUpdate;
            }

            UpdateMoveSpeed(0f, 0f);
            moveCoroutine = null;
            IsDone = true;
        }

        private Vector2 GetMoveVelocity(
            Vector3 destinationPosition,
            Vector2 currentPosition,
            float modifiedSpeed
        )
        {
            if (useRandomPositionAroundPlayer)
            {
                return ((Vector2)destinationPosition - currentPosition).normalized * modifiedSpeed;
            }

            return new Vector2(
                destinationPosition.x < currentPosition.x ? -modifiedSpeed : modifiedSpeed,
                destinationPosition.y < currentPosition.y ? -modifiedSpeed : modifiedSpeed
            );
        }

        private static Vector2 ClampVelocityOutsidePlayerRadius(
            Vector2 currentPosition,
            Vector2 velocity,
            Vector2 playerPosition,
            float minimumDistance,
            float deltaTime
        )
        {
            if (minimumDistance <= 0f || deltaTime <= 0f)
                return velocity;

            Vector2 nextPosition = currentPosition + velocity * deltaTime;
            Vector2 safeNextPosition = ClampOutsidePlayerRadius(
                nextPosition,
                playerPosition,
                minimumDistance,
                currentPosition - playerPosition
            );

            return (safeNextPosition - currentPosition) / deltaTime;
        }

        private static Vector2 ClampOutsidePlayerRadius(
            Vector2 position,
            Vector2 playerPosition,
            float minimumDistance,
            Vector2 fallbackDirection
        )
        {
            float radius = Mathf.Max(0f, minimumDistance);
            if (radius <= 0f)
                return position;

            Vector2 offsetFromPlayer = position - playerPosition;
            if (offsetFromPlayer.sqrMagnitude >= radius * radius)
                return position;

            Vector2 direction = offsetFromPlayer;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                direction = fallbackDirection.sqrMagnitude > Mathf.Epsilon
                    ? fallbackDirection
                    : Vector2.right;
            }

            return playerPosition + direction.normalized * radius;
        }

        private void UpdateMoveSpeed(float newVx, float newVy)
        {
            vx = newVx;
            vy = newVy;
            if (rbody != null)
            {
                rbody.velocity = new Vector2(vx, vy);
            }
            animator?.SetFloat("vx", Mathf.Abs(vx));
        }

        public override void Stop()
        {
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }

            UpdateMoveSpeed(0f, 0f);
            IsExecuting = false;
            IsDone = true;
        }

        public void SetDirection(Direction newDirection)
        {
            direction = newDirection;
            UpdateCharacterDirection();
        }

        private void UpdateCharacterDirection()
        {
            Vector3 localScale = cachedTransform.localScale;
            cachedTransform.localScale = direction == Direction.Left
                ? new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z)
                : new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z);
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
    }
}
