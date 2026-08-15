using System.Collections;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    /**
     * 羽ばたきながら、上下に揺れつつ指定のY座標(ワールド座標)まで戻る。
     * X座標は維持する。急降下攻撃後の高さリセット用。
     */
    public class FlapAndReturnToHeight : EnemyAction
    {
        [SerializeField]
        [Tooltip("戻り先のY座標(ワールド座標)")]
        private float targetY = 4.24f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("上昇/下降の基準速度")]
        private float speed = 3f;

        [SerializeField]
        [Tooltip("羽ばたき中にtrueにするAnimator Bool。空なら変更しない")]
        private string flapBoolName = "isMagic";

        [SerializeField]
        [Min(0f)]
        [Tooltip("移動中の上下の揺れ幅")]
        private float bobAmplitude = 0.25f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("上下の揺れの周波数[Hz]")]
        private float bobFrequency = 2.5f;

        [SerializeField]
        [Min(0.01f)]
        [Tooltip("到達とみなす距離")]
        private float arrivalThreshold = 0.05f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("移動全体のタイムアウト")]
        private float maxDuration = 4f;

        private Rigidbody2D rbody;
        private Animator animator;
        private Transform cachedTransform;
        private Coroutine moveCoroutine;
        private readonly WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

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

            SetFlap(true);
            moveCoroutine = StartCoroutine(ReturnToHeight());
        }

        private IEnumerator ReturnToHeight()
        {
            float elapsedTime = 0f;
            float baseY = cachedTransform.position.y;

            // 揺れを含まない基準高さが目標に達するまで、揺れを乗せながら移動する
            while (Mathf.Abs(targetY - baseY) > arrivalThreshold && elapsedTime < maxDuration)
            {
                if (IsDone)
                    break;

                float deltaTime = Time.fixedDeltaTime;
                elapsedTime += deltaTime;

                float modifiedSpeed = GetModifiedSpeed(speed);
                baseY = Mathf.MoveTowards(baseY, targetY, modifiedSpeed * deltaTime);
                // 残距離が短くなったら揺れを減衰させ、目標付近で暴れないようにする
                float damp = Mathf.Clamp01(Mathf.Abs(targetY - baseY) / (bobAmplitude + 0.2f));
                float bob = Mathf.Sin(elapsedTime * bobFrequency * 2f * Mathf.PI)
                    * bobAmplitude * damp;
                float desiredY = baseY + bob;

                rbody.velocity = new Vector2(
                    0f,
                    (desiredY - cachedTransform.position.y) / deltaTime
                );
                yield return waitForFixedUpdate;
            }

            // 揺れの残差を収めて目標高さに整える
            while (
                Mathf.Abs(targetY - cachedTransform.position.y) > arrivalThreshold
                && elapsedTime < maxDuration
            )
            {
                if (IsDone)
                    break;

                float deltaTime = Time.fixedDeltaTime;
                elapsedTime += deltaTime;
                float distance = targetY - cachedTransform.position.y;
                float modifiedSpeed = GetModifiedSpeed(speed);
                float velocityY = Mathf.Clamp(
                    distance / deltaTime,
                    -modifiedSpeed,
                    modifiedSpeed
                );
                rbody.velocity = new Vector2(0f, velocityY);
                yield return waitForFixedUpdate;
            }

            moveCoroutine = null;
            Complete();
        }

        private void SetFlap(bool value)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(flapBoolName))
            {
                animator.SetBool(flapBoolName, value);
            }
        }

        private void Complete()
        {
            SetFlap(false);
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
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }

            SetFlap(false);
            StopMotion();
            IsExecuting = false;
            IsDone = true;
        }
    }
}
