using System.Collections;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    /**
     * 一瞬静止した後、プレイヤー位置を最下点(下弦の頂点)とする円弧を描いて突撃し、
     * プレイヤーを過ぎたらそのまま円弧に沿って開始時と同じ高さへ戻る。
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
        [Min(0.1f)]
        [Tooltip("プレイヤーが開始高さより上にいる場合に確保する最低降下量")]
        private float minDiveDepth = 0.5f;

        [SerializeField]
        [Min(0.1f)]
        private float maxDuration = 4f;

        [SerializeField]
        private string animationStateName = "Jump";

        [SerializeField]
        [Tooltip("Ground接触で突撃を打ち切るか。無効なら弧の終点まで飛び切る")]
        private bool stopOnGroundHit = false;

        private Rigidbody2D rbody;
        private Animator animator;
        private Transform cachedTransform;
        private Transform playerTransform;
        private Coroutine chargeCoroutine;
        private Vector2 arcCenter;
        private float arcRadius;
        private float arcStartAngle;
        private float arcTotalAngle;

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

            SetupArc(player);
            float arcLength = arcRadius * Mathf.Abs(arcTotalAngle);
            float duration = Mathf.Max(0.05f, arcLength / Mathf.Max(0.1f, GetModifiedSpeed(speed)));

            float t = 0f;
            float elapsed = 0f;
            while (!IsDone && t < 1f && elapsed < maxDuration)
            {
                float deltaTime = Time.fixedDeltaTime;
                t = Mathf.Min(1f, t + deltaTime / duration);
                Vector2 targetPosition = EvaluateArc(t);
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

        private void SetupArc(Transform player)
        {
            Vector2 start = rbody.position;
            // 最下点はプレイヤー位置。開始高さより上にいる場合だけ最低降下量を確保する
            Vector2 lowest = new Vector2(
                player.position.x,
                Mathf.Min(player.position.y, start.y - minDiveDepth)
            );
            float halfWidth = lowest.x - start.x;
            float depth = start.y - lowest.y;
            // 開始点・最下点・対称な終点(開始と同じ高さ)の3点を通る円
            arcRadius = (halfWidth * halfWidth + depth * depth) / (2f * depth);
            arcCenter = new Vector2(lowest.x, lowest.y + arcRadius);

            bool movesRight = halfWidth >= 0f;
            arcStartAngle = Mathf.Atan2(start.y - arcCenter.y, start.x - arcCenter.x);
            // 右向きは開始角を最下点(-90°)より小さく、左向きは大きく正規化し、
            // 弧が必ず最下点を経由するようにする
            if (movesRight && arcStartAngle > -Mathf.PI / 2f)
            {
                arcStartAngle -= Mathf.PI * 2f;
            }
            else if (!movesRight && arcStartAngle < -Mathf.PI / 2f)
            {
                arcStartAngle += Mathf.PI * 2f;
            }
            // 終点角は最下点(-90°)に対して開始角と対称
            float endAngle = -Mathf.PI - arcStartAngle;
            arcTotalAngle = endAngle - arcStartAngle;
        }

        private Vector2 EvaluateArc(float t)
        {
            float angle = arcStartAngle + arcTotalAngle * t;
            return arcCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * arcRadius;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!stopOnGroundHit || !IsExecuting || IsDone || !collision.collider.CompareTag("Ground"))
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
