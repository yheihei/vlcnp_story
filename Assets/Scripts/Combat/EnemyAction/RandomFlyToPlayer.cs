using System.Collections;
using UnityEngine;
using VLCNP.Movement;

namespace VLCNP.Combat.EnemyAction
{
    /**
     * プレイヤーとの距離を調整しながら飛行する。
     * useRandomPositionAroundPlayer が有効な場合は、プレイヤー周囲のランダム地点へ移動する。
     */
    public class RandomFlyToPlayer : EnemyAction
    {
        [SerializeField]
        private float speed = 6;

        [SerializeField]
        private float directionMultiplier = 4;

        [SerializeField]
        private Vector2 offset = new Vector2(0, 0);

        [SerializeField] // どの距離まで近づくか
        private float maxApproachDistance = 2f;

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
        private float vx = 0;
        private float vy = 0;
        private readonly WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

        public enum Direction
        {
            Left,
            Right,
        }

        Direction direction = Direction.Left;

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

            Vector3 position = cachedTransform.position;
            Vector3 destinationPosition = useRandomPositionAroundPlayer
                ? GetRandomPositionAroundPlayer(player)
                : GetLegacyDestinationPosition(player, position);

            moveCoroutine = StartCoroutine(MoveToPosition(destinationPosition, moveTimeout));
        }

        private Vector3 GetRandomPositionAroundPlayer(Transform player)
        {
            float minX = Mathf.Min(randomOffsetMin.x, randomOffsetMax.x);
            float maxX = Mathf.Max(randomOffsetMin.x, randomOffsetMax.x);
            float minY = Mathf.Min(randomOffsetMin.y, randomOffsetMax.y);
            float maxY = Mathf.Max(randomOffsetMin.y, randomOffsetMax.y);

            return new Vector3(
                player.position.x + UnityEngine.Random.Range(minX, maxX),
                player.position.y + UnityEngine.Random.Range(minY, maxY),
                cachedTransform.position.z
            );
        }

        private Vector3 GetLegacyDestinationPosition(Transform player, Vector3 position)
        {
            // プレイヤーの方向のベクトルを計算
            Vector3 playerDirection = player.position - position;
            // プレイヤーと自分の距離がmaxApproachDistance 以下の場合は逆ベクトルに向かう
            if (playerDirection.sqrMagnitude < maxApproachDistance * maxApproachDistance)
            {
                playerDirection *= -1;
            }
            playerDirection.Normalize();
            playerDirection *= directionMultiplier;
            // 到達地点を計算
            Vector3 destinationPosition = new Vector3(
                position.x + playerDirection.x,
                position.y + playerDirection.y + offset.y,
                position.z
            );
            // ランダムに方向を変える プレイヤーが下方向の場合上側に、上方向の場合下側にランダムに角度をつける
            float randomAngle =
                playerDirection.y < 0
                    ? UnityEngine.Random.Range(0, 15f)
                    : UnityEngine.Random.Range(-15f, 0);

            destinationPosition =
                Quaternion.Euler(0, 0, randomAngle) * (destinationPosition - position) + position;
            return destinationPosition;
        }

        private IEnumerator MoveToPosition(Vector3 position, float timeout = 0)
        {
            // プレイヤーの位置が指定のx位置より左にある場合は左を向く
            if (position.x < cachedTransform.position.x)
            {
                SetDirection(Direction.Left);
            }
            else
            {
                SetDirection(Direction.Right);
            }
            // 経過時間を格納する変数
            float elapsedTime = 0;
            float sqrArrivalDistance = useRandomPositionAroundPlayer
                ? arrivalDistance * arrivalDistance
                : 0.01f;
            // プレイヤーの位置と指定の位置の距離が特定の値以下になるまでループ
            while ((cachedTransform.position - position).sqrMagnitude > sqrArrivalDistance)
            {
                if (IsDone)
                    break;
                // 経過時間加算
                elapsedTime += Time.fixedDeltaTime;
                // タイムアウト値になったらループを抜ける
                if (timeout > 0 && elapsedTime > timeout)
                {
                    break;
                }
                // 前方に障害物がある場合はループを抜ける
                if (frontCollisionDetector != null && frontCollisionDetector.IsColliding)
                {
                    break;
                }
                // 指定の位置に向かって移動（速度修正を適用）
                float modifiedSpeed = GetModifiedSpeed(speed);
                Vector3 currentPosition = cachedTransform.position;
                Vector2 velocity;
                if (useRandomPositionAroundPlayer)
                {
                    velocity = ((Vector2)(position - currentPosition)).normalized * modifiedSpeed;
                }
                else
                {
                    velocity = new Vector2(
                        position.x < currentPosition.x ? -modifiedSpeed : modifiedSpeed,
                        position.y < currentPosition.y ? -modifiedSpeed : modifiedSpeed
                    );
                }
                UpdateMoveSpeed(velocity.x, velocity.y);
                yield return waitForFixedUpdate;
            }
            UpdateMoveSpeed(0, 0);
            moveCoroutine = null;
            IsDone = true;
        }

        private void UpdateMoveSpeed(float _vx, float _vy)
        {
            vx = _vx;
            vy = _vy;
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

            UpdateMoveSpeed(0, 0);
            IsExecuting = false;
            IsDone = true;
        }

        public void SetDirection(Direction _direction)
        {
            direction = _direction;
            UpdateCharacterDirection();
        }

        private void UpdateCharacterDirection()
        {
            Vector3 localScale = cachedTransform.localScale;
            if (direction == Direction.Left)
            {
                cachedTransform.localScale = new Vector3(
                    Mathf.Abs(localScale.x),
                    localScale.y,
                    localScale.z
                );
            }
            else
            {
                cachedTransform.localScale = new Vector3(
                    -1 * Mathf.Abs(localScale.x),
                    localScale.y,
                    localScale.z
                );
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
    }
}
