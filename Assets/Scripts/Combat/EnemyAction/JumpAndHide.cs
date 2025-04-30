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
            rBody.AddForce(new Vector2(0, 2400), ForceMode2D.Impulse);
            // ちょっと待つ
            yield return new WaitForSeconds(0.5f);

            //hidePositionに移動し、重力加速度を0にして浮いておく
            transform.position = hidePosition.position;
            rBody.gravityScale = 0;
            rBody.velocity = Vector2.zero;
            IsDone = true;
        }
    }
}
