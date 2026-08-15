using System.Collections;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    /**
     * スプライトの角度は変えず、その場から指定半径の円をくるっと一周して開始位置へ戻る。
     * 攻撃前の予備動作(合図)として攻撃Actionの直前に挟んで使う。
     */
    public class CircularLoop : EnemyAction
    {
        [SerializeField]
        [Tooltip("一周にかける秒数")]
        float duration = 0.5f;

        [SerializeField]
        [Tooltip("円の半径(m)")]
        float radius = 0.3f;

        [SerializeField]
        [Tooltip("向いている方向へ回り始めるか。無効なら常に右回り始動")]
        bool startTowardFacing = true;

        Rigidbody2D rbody;
        Coroutine loopRoutine;

        void Awake()
        {
            rbody = GetComponent<Rigidbody2D>();
        }

        public override void Execute()
        {
            if (IsExecuting || IsDone)
                return;

            IsExecuting = true;
            loopRoutine = StartCoroutine(Loop());
        }

        public override void Stop()
        {
            if (loopRoutine != null)
            {
                StopCoroutine(loopRoutine);
                loopRoutine = null;
            }

            StopMoving();
            IsExecuting = false;
            IsDone = true;
        }

        IEnumerator Loop()
        {
            float loopDuration = Mathf.Max(0.01f, duration);
            if (rbody == null)
            {
                yield return new WaitForSeconds(loopDuration);
                IsDone = true;
                yield break;
            }

            Vector2 startPosition = rbody.position;
            // 円の中心は開始位置の真上。θ=0で開始位置、一周して開始位置へ戻る
            Vector2 center = startPosition + Vector2.up * radius;
            // 正のXスケールで左向きの規約。向いている方向へ回り始める
            float directionX = 1f;
            if (startTowardFacing && transform.localScale.x > 0f)
            {
                directionX = -1f;
            }

            float elapsed = 0f;
            while (!IsDone && elapsed < loopDuration)
            {
                elapsed += Time.fixedDeltaTime;
                float theta = Mathf.Clamp01(elapsed / loopDuration) * Mathf.PI * 2f;
                Vector2 position = center
                    + new Vector2(Mathf.Sin(theta) * radius * directionX, -Mathf.Cos(theta) * radius);
                rbody.velocity = Vector2.zero;
                rbody.MovePosition(position);
                yield return new WaitForFixedUpdate();
            }

            rbody.MovePosition(startPosition);
            StopMoving();
            loopRoutine = null;
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
