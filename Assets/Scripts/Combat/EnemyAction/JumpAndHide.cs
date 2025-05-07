using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class JumpAndHide : EnemyAction
    {
        private Rigidbody2D rBody;

        [SerializeField]
        private Transform hidePosition;

        private void Awake()
        {
            rBody = GetComponent<Rigidbody2D>();
            if (hidePosition == null)
            {
                throw new Exception("hidePosition is null");
            }
        }

        public override void Execute()
        {
            if (IsExecuting)
                return;
            if (IsDone)
                return;
            IsExecuting = true;
            StartCoroutine(JumpAndHideExecute());
        }

        IEnumerator JumpAndHideExecute()
        {
            // 垂直に素早く飛ぶ
            rBody.AddForce(new Vector2(0, 1800), ForceMode2D.Impulse);
            // 0.5sかけてFadeOut
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                for (float t = 0; t < 0.5f; t += Time.deltaTime)
                {
                    float alpha = Mathf.Lerp(1, 0, t / 0.5f);
                    Color color = spriteRenderer.color;
                    color.a = alpha;
                    spriteRenderer.color = color;
                    yield return null;
                }
            }

            //hidePositionに移動し、重力加速度を0にして浮いておく
            transform.position = hidePosition.position;
            rBody.gravityScale = 0;
            rBody.velocity = Vector2.zero;
            // 透明解除
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 1;
                spriteRenderer.color = color;
            }
            IsDone = true;
        }
    }
}
