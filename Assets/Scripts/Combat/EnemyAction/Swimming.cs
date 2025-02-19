using System;
using System.Collections;
using UnityEngine;
using VLCNP.Movement;

namespace VLCNP.Combat.EnemyAction
{
    /**
     * 足場があるところを左右に繰り返し移動する
     */
    public class Swimming : EnemyAction
    {
        [SerializeField]
        private FrontCollisionDetector frontCollisionDetector = null;

        [SerializeField]
        private float swimPower = 10000f;

        [SerializeField]
        private float strokeInterval = 1.5f;

        Rigidbody2D rbody;
        Animator animator;
        float vx = 0;

        public enum Direction
        {
            Left,
            Right,
        }

        Direction direction = Direction.Left;

        private void Awake()
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
            IsExecuting = true;
            StartCoroutine(Swim());
        }

        private IEnumerator Swim()
        {
            UpdateDirectionFromCurrentTransform();
            animator?.SetBool("throw", true);
            yield return new WaitForSeconds(0.5f);
            // 向いている方向にpowerを加える
            UpdateSwimAnimation(10f);
            rbody.AddForce(new Vector2(direction == Direction.Left ? -swimPower : swimPower, 0));
            yield return new WaitForSeconds(strokeInterval);
            // 停止
            rbody.velocity = new Vector2(0, 0);
            UpdateSwimAnimation(0);
            IsDone = true;
        }

        private void UpdateSwimAnimation(float vx)
        {
            animator?.SetFloat("vx", Mathf.Abs(vx));
        }

        public void SetDirection(Direction _direction)
        {
            direction = _direction;
            UpdateDirectionFromCurrentTransform();
        }

        private void UpdateDirectionFromCurrentTransform()
        {
            if (transform.localScale.x > 0)
            {
                direction = Direction.Left;
            }
            else
            {
                direction = Direction.Right;
            }
        }
    }
}
