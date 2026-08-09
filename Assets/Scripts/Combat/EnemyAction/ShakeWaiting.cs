using System.Collections;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    /**
     * その場で飛行を保ちながら左右に細かく揺れて待機する。
     * 揺れ幅と速さを終わりにかけて強めることで、次の攻撃の予備動作として使える。
     */
    public class ShakeWaiting : EnemyAction
    {
        [SerializeField]
        float waitTimeSecond = 1f;

        [SerializeField]
        [Tooltip("揺れ始めの横振幅")]
        float startShakeAmplitude = 0.03f;

        [SerializeField]
        [Tooltip("揺れ終わりの横振幅")]
        float endShakeAmplitude = 0.12f;

        [SerializeField]
        [Tooltip("揺れ始めの往復回数(回/秒)")]
        float startShakeFrequency = 6f;

        [SerializeField]
        [Tooltip("揺れ終わりの往復回数(回/秒)")]
        float endShakeFrequency = 16f;

        [SerializeField]
        [Tooltip("飛行感を残すための上下の揺れ幅。0で無効")]
        float verticalBobAmplitude = 0.05f;

        [SerializeField]
        [Tooltip("待機中に上下へ揺れる回数")]
        float verticalBobCycles = 2f;

        [SerializeField]
        [Tooltip("待機中に再生するAnimation State。空なら再生中のものを維持する")]
        string animationStateName = "";

        Rigidbody2D rbody;
        Animator animator;
        DamageStun damageStun;
        Coroutine shakeRoutine;

        void Awake()
        {
            rbody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            damageStun = GetComponent<DamageStun>();
        }

        public override void Execute()
        {
            if (IsExecuting || IsDone)
                return;

            IsExecuting = true;
            if (animator != null && !string.IsNullOrEmpty(animationStateName))
            {
                animator.Play(animationStateName, 0, 0f);
            }
            shakeRoutine = StartCoroutine(Shake());
        }

        public override void Stop()
        {
            if (shakeRoutine != null)
            {
                StopCoroutine(shakeRoutine);
                shakeRoutine = null;
            }

            StopMoving();
            IsExecuting = false;
            IsDone = true;
        }

        IEnumerator Shake()
        {
            if (damageStun != null)
            {
                damageStun.ValidStan();
            }

            if (rbody == null)
            {
                yield return new WaitForSeconds(Mathf.Max(0.01f, waitTimeSecond));
                IsDone = true;
                yield break;
            }

            Vector2 anchor = rbody.position;
            float duration = Mathf.Max(0.01f, waitTimeSecond);
            float elapsed = 0f;
            float phase = 0f;

            while (!IsDone && elapsed < duration)
            {
                float rate = Mathf.Clamp01(elapsed / duration);
                float amplitude = Mathf.Lerp(startShakeAmplitude, endShakeAmplitude, rate);
                float frequency = Mathf.Lerp(startShakeFrequency, endShakeFrequency, rate);
                phase += frequency * Mathf.PI * 2f * Time.fixedDeltaTime;

                Vector2 position = anchor;
                position.x += Mathf.Sin(phase) * amplitude;
                position.y +=
                    Mathf.Sin(rate * Mathf.PI * 2f * verticalBobCycles) * verticalBobAmplitude;
                rbody.velocity = Vector2.zero;
                rbody.MovePosition(position);

                elapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            StopMoving();
            shakeRoutine = null;
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
