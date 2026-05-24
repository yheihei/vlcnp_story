using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using VLCNP.Movement;

namespace VLCNP.Combat.EnemyAction
{
    public class RandomFlyToPlayer : EnemyAction
    {
        [SerializeField]
        float speed = 6;

        [SerializeField]
        float directionMultiplier = 4;

        [SerializeField]
        Vector2 offset = new Vector2(0, 0);

        [SerializeField] // どの距離まで近づくか
        float maxApproachDistance = 2f;

        [SerializeField]
        private FrontCollisionDetector frontCollisionDetector = null;

        [SerializeField]
        private float moveTimeout = 2f;

        Rigidbody2D rbody;
        Animator animator;
        Transform cachedTransform;
        Transform playerTransform;
        float vx = 0;
        float vy = 0;

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
            if (IsExecuting)
                return;
            if (IsDone)
                return;
            IsExecuting = true;
            Vector3 position = cachedTransform.position;
            if (!TryGetPlayerTransform(out Transform player))
            {
                IsDone = true;
                return;
            }
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
            StartCoroutine(MoveToPosition(destinationPosition, moveTimeout));
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
            // プレイヤーの位置と指定の位置の距離が特定の値以下になるまでループ
            while ((cachedTransform.position - position).sqrMagnitude > 0.01f)
            {
                if (IsDone)
                    break;
                // 経過時間加算
                elapsedTime += Time.deltaTime;
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
                UpdateMoveSpeed(
                    position.x < currentPosition.x ? -modifiedSpeed : modifiedSpeed,
                    position.y < currentPosition.y ? -modifiedSpeed : modifiedSpeed
                );
                yield return null;
            }
            UpdateMoveSpeed(0, 0);
            IsDone = true;
        }

        private void UpdateMoveSpeed(float _vx, float _vy)
        {
            vx = _vx;
            vy = _vy;
            rbody.velocity = new Vector2(vx, vy);
            animator?.SetFloat("vx", Mathf.Abs(vx));
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
