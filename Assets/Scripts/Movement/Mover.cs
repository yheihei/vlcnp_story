using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VLCNP.Combat;
using VLCNP.Saving;

namespace VLCNP.Movement
{
    public class Mover : MonoBehaviour
    {
        [SerializeField] float speed = 4;
        bool isLeft = true;
        // isLeftのgetterを定義
        public bool IsLeft { get => isLeft; set => isLeft = value; }
        [SerializeField] Leg leg;
        float vx = 0;
        Rigidbody2D rbody;
        Animator animator;
        PlayerStun playerStun;
        Dash dash;

        private void Awake()
        {
            rbody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            playerStun = GetComponent<PlayerStun>();
            dash = GetComponent<Dash>();
        }

        public void Move()
        {
            vx = 0;
            // Stun状態の場合は移動不可
            if (isStunned()) return;
            if (Input.GetKey("right"))
            {
                vx = speed;
                isLeft = false;
            }
            if (Input.GetKey("left"))
            {
                vx = -speed;
                isLeft = true;
            }
            UpdateAnimator();
        }

        private bool isStunned()
        {
            return playerStun != null && playerStun.IsStunned();
        }

        public IEnumerator MoveToPosition(Vector3 position, float timeout = 0)
        {
            // 経過時間を格納する変数
            float elapsedTime = 0;
            // プレイヤーの位置と指定のx位置が0.05以内になるまでループ
            while (Mathf.Abs(position.x - transform.position.x) > 0.05f)
            {
                // 経過時間加算
                elapsedTime += Time.deltaTime;
                // タイムアウト値になったらループを抜ける
                if (timeout > 0 && elapsedTime > timeout)
                {
                    break;
                }
                // 指定の位置に向かって移動
                vx = position.x < transform.position.x ? -speed : speed;
                UpdateMoveSpeed();
                UpdateAnimator();
                UpdateCharacterDirection();
                yield return null;
            }
        }

        public void Stop()
        {
            vx = 0;
            UpdateAnimator();
        }

        private void UpdateAnimator()
        {
            animator.SetFloat("vx", Mathf.Abs(vx));
            animator.SetBool("isGround", leg.IsGround);
        }

        private void FixedUpdate()
        {
            // Stun状態の場合は移動不可
            if (isStunned()) return;
            // ダッシュ中は移動不可
            if (dash != null && dash.IsDashing) return;
            UpdateMoveSpeed();
            UpdateCharacterDirection();
        }

        private void UpdateMoveSpeed()
        {
            rbody.velocity = new Vector2(vx, rbody.velocity.y);
        }

        private void UpdateCharacterDirection()
        {
            if (isLeft)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                transform.localScale = new Vector3(-1 * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }
}
