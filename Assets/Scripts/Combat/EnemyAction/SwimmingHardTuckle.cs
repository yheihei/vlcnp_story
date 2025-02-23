using System;
using System.Collections;
using UnityEngine;
using VLCNP.Movement;

namespace VLCNP.Combat.EnemyAction
{
    /**
    * 水中で近くにきたプレイヤーに向かって突進する
    */
    public class SwimmingHardTuckle : EnemyAction, IWaterEventListener
    {
        [SerializeField]
        private FrontCollisionDetector frontCollisionDetector = null;

        [SerializeField]
        private float swimPower = 20000f;

        [SerializeField]
        private float strokeInterval = 1.5f;

        [SerializeField]
        private float attackRange = 4f;

        Rigidbody2D rbody;
        Animator animator;
        float vx = 0;

        public enum Direction
        {
            Left,
            Right,
        }

        Direction direction = Direction.Left;
        GameObject player;
        bool isInWater = false;

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
            StartCoroutine(Tackle());
        }

        private IEnumerator Tackle()
        {
            if (!isInWater)
            {
                IsDone = true;
                yield break;
            }
            player = GameObject.FindGameObjectWithTag("Player");
            if (!isAttackable(player))
            {
                IsDone = true;
                yield break;
            }
            animator?.SetBool("special1", true);
            // playerの方角に向けて力を加える
            UpdateDirectionFromCurrentTransform();
            // プレイヤーの方角のベクトルを取得
            Vector2 playerDirection = player.transform.position - transform.position;
            // プレイヤーの方角に力を加える
            rbody.AddForce(playerDirection.normalized * swimPower);
            yield return new WaitForSeconds(strokeInterval);
            // 止める
            rbody.velocity = new Vector2(0, 0);
            IsDone = true;
        }

        private bool isAttackable(GameObject player)
        {
            // プレイヤーとの距離を取得しそれが攻撃範囲内かどうかを返す
            if (player == null)
                return false;
            return Vector2.Distance(player.transform.position, transform.position) < attackRange;
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

        public void OnWaterEnter()
        {
            // 重力を0に
            rbody.gravityScale = 0;
            isInWater = true;
            // 着水後0.2s後にy方向の速度0に
            StartCoroutine(StopYVelocity());
        }

        private IEnumerator StopYVelocity()
        {
            yield return new WaitForSeconds(0.2f);
            rbody.velocity = new Vector2(rbody.velocity.x, 0);
        }

        public void OnWaterExit()
        {
            // 重力を高くする
            rbody.gravityScale = 1.5f;
            isInWater = false;
        }

        public void OnWaterStay() { }
    }
}
