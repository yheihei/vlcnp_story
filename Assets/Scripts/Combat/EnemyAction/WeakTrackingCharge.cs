using System.Collections;
using Core.Status;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    /**
     * プレイヤーを弱く追尾しながら、Groundに衝突するまで突撃する。
     */
    public class WeakTrackingCharge : EnemyAction
    {
        [SerializeField]
        float speed = 10f;

        [SerializeField]
        [Range(0f, 3f)]
        float trackingRate = 0.65f;

        [SerializeField]
        float maxDuration = 5f;

        [SerializeField]
        string animationStateName = "Charge";

        Rigidbody2D rbody;
        Animator animator;
        SpeedModifier speedModifier;
        Transform playerTransform;
        Vector2 chargeDirection;
        Coroutine chargeRoutine;

        void Awake()
        {
            rbody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            speedModifier = GetComponent<SpeedModifier>();
        }

        public override void Execute()
        {
            if (IsExecuting || IsDone)
                return;

            if (!TryGetPlayer(out Transform player))
            {
                IsDone = true;
                return;
            }

            Vector2 direction = player.position - transform.position;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = transform.right;
            }

            IsExecuting = true;
            chargeDirection = direction.normalized;
            UpdateOrientation();
            animator.Play(animationStateName, 0, 0f);
            chargeRoutine = StartCoroutine(Charge());
        }

        public override void Stop()
        {
            if (chargeRoutine != null)
            {
                StopCoroutine(chargeRoutine);
                chargeRoutine = null;
            }

            StopMoving();
            IsExecuting = false;
            IsDone = true;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsExecuting || IsDone || !collision.collider.CompareTag("Ground"))
                return;

            if (chargeRoutine != null)
            {
                StopCoroutine(chargeRoutine);
                chargeRoutine = null;
            }
            StopMoving();
            IsDone = true;
        }

        IEnumerator Charge()
        {
            float elapsed = 0f;
            while (!IsDone && (maxDuration <= 0f || elapsed < maxDuration))
            {
                if (TryGetPlayer(out Transform player))
                {
                    Vector2 desiredDirection = player.position - transform.position;
                    if (desiredDirection.sqrMagnitude > 0.001f)
                    {
                        desiredDirection.Normalize();
                        float followAmount = Mathf.Clamp01(trackingRate * Time.fixedDeltaTime);
                        chargeDirection = Vector2.Lerp(
                            chargeDirection,
                            desiredDirection,
                            followAmount
                        ).normalized;
                    }
                }

                float modifiedSpeed = speedModifier != null
                    ? speedModifier.CalculateModifiedSpeed(speed)
                    : speed;
                rbody.velocity = chargeDirection * modifiedSpeed;
                UpdateOrientation();
                elapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            StopMoving();
            chargeRoutine = null;
            IsDone = true;
        }

        void StopMoving()
        {
            if (rbody != null)
            {
                rbody.velocity = Vector2.zero;
            }
        }

        void UpdateOrientation()
        {
            if (chargeDirection.sqrMagnitude <= 0.001f)
                return;

            bool isFacingLeft = chargeDirection.x < 0f;
            Vector3 localScale = transform.localScale;
            localScale.x = isFacingLeft
                ? Mathf.Abs(localScale.x)
                : -Mathf.Abs(localScale.x);
            transform.localScale = localScale;

            Vector2 orientationDirection = isFacingLeft ? -chargeDirection : chargeDirection;
            float angle = Mathf.Atan2(orientationDirection.y, orientationDirection.x)
                * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        bool TryGetPlayer(out Transform player)
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
