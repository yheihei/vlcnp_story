using Newtonsoft.Json.Linq;
using UnityEngine;
using VLCNP.Saving;

namespace VLCNP.Movement
{
    public class Mover : MonoBehaviour, IJsonSaveable
    {
        [SerializeField] float speed = 4;
        [SerializeField] float jumpPower = 7;
        bool isLeft = true;
        // isLeftのgetterを定義
        public bool IsLeft { get => isLeft;  set => isLeft = value;}
        bool isGround = true;
        bool isJumping= false;
        bool isPushing = false;
        float vx = 0;
        Rigidbody2D rbody;
        Animator animator;

        private void Awake() {
            rbody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        public void Move()
        {
            vx = 0;
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
            if (Input.GetKey("space") && CanJump())
            {
                isJumping = true;
                isPushing = true;
            }
            else
            {
                isPushing = false;
            }
            UpdateAnimator();
        }

        public void Stop()
        {
            vx = 0;
            isPushing = false;
            UpdateAnimator();
        }

        private bool CanJump()
        {
            // 地面についていて、ジャンプボタン押しっぱなしでない、かつ、上向きの速度が0.1以下
            return  isGround && !isPushing && rbody.velocity.y < 0.1f;
        }

        private void UpdateAnimator()
        {
            animator.SetFloat("vx", Mathf.Abs(vx));
            animator.SetBool("isGround", isGround);
        }

        private void FixedUpdate()
        {
            UpdateMoveSpeed();
            UpdateJumpState();
            UpdateCharacterDirection();
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!collision.tag.Equals("Ground") && !collision.tag.Equals("Enemy"))
            {
                return;
            }
            isGround = true;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.tag.Equals("Ground") && !collision.tag.Equals("Enemy"))
            {
                return;
            }
            isGround = false;
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

        private void UpdateJumpState()
        {
            if (isJumping)
            {
                isJumping = false;
                rbody.AddForce(new Vector2(0, jumpPower), ForceMode2D.Impulse);
            }
        }

        public JToken CaptureAsJToken()
        {
            return transform.position.ToToken();
        }

        public void RestoreFromJToken(JToken state)
        {
            transform.position = state.ToVector3();
        }
    }    
}
