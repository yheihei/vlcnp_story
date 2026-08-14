using System.Collections;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    /**
     * 一瞬静止した後、プレイヤーの下を通る弧(下弦)を描いて反対側へ突撃する。
     */
    public class CrescentSwoopCharge : EnemyAction
    {
        [SerializeField]
        [Min(0f)]
        [Tooltip("突撃前に静止する時間")]
        private float pauseBeforeCharge = 0.35f;

        [SerializeField]
        [Min(0.1f)]
        private float speed = 12f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("弧の最下点がプレイヤーの何m下を通るか")]
        private float diveDepthBelowPlayer = 1.2f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("プレイヤーを追い越して反対側へ抜ける距離")]
        private float overshootDistanceX = 3f;

        [SerializeField]
        [Min(0.1f)]
        private float maxDuration = 4f;

        [SerializeField]
        private string animationStateName = "Jump";

        private const int ArcLengthSampleCount = 32;

        private Rigidbody2D rbody;
        private Animator animator;
        private Transform cachedTransform;
        private Transform playerTransform;
        private Coroutine chargeCoroutine;
        private Vector2 bezierStart;
        private Vector2 bezierControl;
        private Vector2 bezierEnd;

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

            IsExecuting = true;
            StopMotion();
            SetDirection(player.position.x < cachedTransform.position.x);
            if (animator != null && !string.IsNullOrEmpty(animationStateName))
            {
                animator.Play(animationStateName, 0, 0f);
            }
            chargeCoroutine = StartCoroutine(Charge());
        }

        private IEnumerator Charge()
        {
            float pauseElapsed = 0f;
            while (!IsDone && pauseElapsed < pauseBeforeCharge)
            {
                StopMotion();
                pauseElapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            if (IsDone)
            {
                chargeCoroutine = null;
                yield break;
            }

            if (!TryGetPlayerTransform(out Transform player))
            {
                Complete();
                yield break;
            }

            SetupBezier(player);
            float arcLength = EstimateArcLength();
            float duration = Mathf.Max(0.05f, arcLength / Mathf.Max(0.1f, GetModifiedSpeed(speed)));

            float t = 0f;
            float elapsed = 0f;
            while (!IsDone && t < 1f && elapsed < maxDuration)
            {
                float deltaTime = Time.fixedDeltaTime;
                t = Mathf.Min(1f, t + deltaTime / duration);
                Vector2 targetPosition = EvaluateBezier(t);
                Vector2 velocity = (targetPosition - rbody.position) / deltaTime;
                float maxSpeed = GetModifiedSpeed(speed) * 3f;
                if (velocity.sqrMagnitude > maxSpeed * maxSpeed)
                {
                    velocity = velocity.normalized * maxSpeed;
                }
                rbody.velocity = velocity;
                if (Mathf.Abs(velocity.x) > 0.01f)
                {
                    SetDirection(velocity.x < 0f);
                }
                elapsed += deltaTime;
                yield return new WaitForFixedUpdate();
            }

            chargeCoroutine = null;
            Complete();
        }

        private void SetupBezier(Transform player)
        {
            bezierStart = rbody.position;
            float directionX = player.position.x < bezierStart.x ? -1f : 1f;
            float endX = player.position.x + directionX * overshootDistanceX;
            bezierEnd = new Vector2(endX, bezierStart.y);
            // t=0.5で最下点(プレイヤーの下)を通るよう制御点を決める
            Vector2 lowestPoint = new Vector2(
                player.position.x,
                player.position.y - diveDepthBelowPlayer
            );
            bezierControl = 2f * lowestPoint - 0.5f * (bezierStart + bezierEnd);
        }

        private float EstimateArcLength()
        {
            float length = 0f;
            Vector2 previousPoint = bezierStart;
            for (int i = 1; i <= ArcLengthSampleCount; i++)
            {
                Vector2 point = EvaluateBezier((float)i / ArcLengthSampleCount);
                length += Vector2.Distance(previousPoint, point);
                previousPoint = point;
            }
            return length;
        }

        private Vector2 EvaluateBezier(float t)
        {
            float u = 1f - t;
            return u * u * bezierStart + 2f * u * t * bezierControl + t * t * bezierEnd;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsExecuting || IsDone || !collision.collider.CompareTag("Ground"))
                return;

            if (chargeCoroutine != null)
            {
                StopCoroutine(chargeCoroutine);
                chargeCoroutine = null;
            }
            Complete();
        }

        private void SetDirection(bool isFacingLeft)
        {
            Vector3 localScale = cachedTransform.localScale;
            localScale.x = isFacingLeft
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

        private void Complete()
        {
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
            if (chargeCoroutine != null)
            {
                StopCoroutine(chargeCoroutine);
                chargeCoroutine = null;
            }

            StopMotion();
            IsExecuting = false;
            IsDone = true;
        }
    }
}
