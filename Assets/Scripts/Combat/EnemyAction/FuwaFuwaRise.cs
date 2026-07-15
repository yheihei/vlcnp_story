using System.Collections;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    /**
     * 左右に揺れながら指定した高さまで浮上する。
     */
    public class FuwaFuwaRise : EnemyAction
    {
        [SerializeField]
        float riseHeight = 2.5f;

        [SerializeField]
        float riseDuration = 1.6f;

        [SerializeField]
        float swayAmplitude = 0.25f;

        [SerializeField]
        string animationStateName = "Hover";

        Rigidbody2D rbody;
        Animator animator;
        Coroutine riseRoutine;

        void Awake()
        {
            rbody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        public override void Execute()
        {
            if (IsExecuting || IsDone)
                return;

            IsExecuting = true;
            animator.Play(animationStateName, 0, 0f);
            riseRoutine = StartCoroutine(Rise());
        }

        public override void Stop()
        {
            if (riseRoutine != null)
            {
                StopCoroutine(riseRoutine);
                riseRoutine = null;
            }

            StopMoving();
            IsExecuting = false;
            IsDone = true;
        }

        IEnumerator Rise()
        {
            Vector2 startPosition = rbody.position;
            Quaternion startRotation = transform.rotation;
            float duration = Mathf.Max(0.01f, riseDuration);
            float elapsed = 0f;

            while (!IsDone && elapsed < duration)
            {
                elapsed += Time.fixedDeltaTime;
                float rate = Mathf.Clamp01(elapsed / duration);
                float easedRate = rate * rate * (3f - 2f * rate);
                Vector2 position = Vector2.Lerp(
                    startPosition,
                    startPosition + Vector2.up * riseHeight,
                    easedRate
                );
                position.x += Mathf.Sin(rate * Mathf.PI * 4f) * swayAmplitude;
                rbody.MovePosition(position);
                transform.rotation = Quaternion.Slerp(
                    startRotation,
                    Quaternion.identity,
                    easedRate
                );
                yield return new WaitForFixedUpdate();
            }

            StopMoving();
            transform.rotation = Quaternion.identity;
            riseRoutine = null;
            IsDone = true;
        }

        void StopMoving()
        {
            if (rbody != null)
            {
                rbody.velocity = Vector2.zero;
            }
        }
    }
}
