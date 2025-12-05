using System.Collections;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    /// <summary>
    /// 上下にゆらぎを付けながらプレイヤーへ一定時間接近するEnemyAction。
    /// </summary>
    public class FuwaFuwaMoveToPlayer : EnemyAction
    {
        [SerializeField]
        float speed = 2f;

        [SerializeField]
        float approachDuration = 2.5f;

        [SerializeField, Min(0f)]
        float floatAmplitude = 0.25f;

        [SerializeField, Min(0.01f)]
        float floatCycleDuration = 2f;

        Rigidbody2D rbody;
        Animator animator;

        Coroutine moveRoutine;

        enum Direction
        {
            Left,
            Right,
        }

        Direction direction = Direction.Left;

        void Awake()
        {
            rbody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        public override void Execute()
        {
            if (IsExecuting)
                return;
            if (IsDone)
                return;

            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                IsDone = true;
                return;
            }

            IsExecuting = true;
            moveRoutine = StartCoroutine(MoveToPlayer(player.transform));
        }

        public new void Stop()
        {
            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
                moveRoutine = null;
            }
            if (rbody != null)
            {
                rbody.velocity = Vector2.zero;
            }
            IsExecuting = false;
            IsDone = true;
        }

        IEnumerator MoveToPlayer(Transform player)
        {
            float elapsed = 0f;
            float floatElapsed = 0f;

            while (elapsed < approachDuration)
            {
                if (IsDone)
                    break;
                if (player == null)
                    break;

                elapsed += Time.deltaTime;
                float deltaTime = Time.deltaTime;

                Vector2 directionToPlayer = (player.position - transform.position);
                if (directionToPlayer.sqrMagnitude > 0.001f)
                {
                    directionToPlayer.Normalize();
                }
                else
                {
                    directionToPlayer = Vector2.zero;
                }

                float modifiedSpeed = GetModifiedSpeed(speed);
                Vector2 moveVelocity = directionToPlayer * modifiedSpeed;

                // 上下に揺れるための速度成分（sin波の微分）
                floatElapsed += deltaTime;
                float angularFrequency = Mathf.PI * 2f / Mathf.Max(floatCycleDuration, 0.01f);
                float bobVelocity = Mathf.Cos(floatElapsed * angularFrequency) * floatAmplitude * angularFrequency;

                Vector2 finalVelocity = moveVelocity + new Vector2(0f, bobVelocity);

                UpdateDirection(moveVelocity.x);

                if (rbody != null)
                {
                    rbody.velocity = finalVelocity;
                }
                else
                {
                    transform.position += (Vector3)(finalVelocity * deltaTime);
                }

                animator?.SetFloat("vx", Mathf.Abs(moveVelocity.x));

                yield return null;
            }

            if (rbody != null)
            {
                rbody.velocity = Vector2.zero;
            }
            IsDone = true;
        }

        void UpdateDirection(float vx)
        {
            if (Mathf.Abs(vx) < 0.001f)
                return;

            direction = vx < 0 ? Direction.Left : Direction.Right;
            UpdateCharacterDirection();
        }

        void UpdateCharacterDirection()
        {
            if (direction == Direction.Left)
            {
                transform.localScale = new Vector3(
                    Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
            else
            {
                transform.localScale = new Vector3(
                    -1 * Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
        }
    }
}
