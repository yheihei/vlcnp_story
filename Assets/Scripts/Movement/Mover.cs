using UnityEngine;

namespace VLCNP.Movement
{
    public class Mover : MonoBehaviour
    {
        [SerializeField] float speed = 4;
        [SerializeField] float jumpPower = 8;
        bool isLeft = true;
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
            if (Input.GetKey("space") && isGround)
            {
                if (!isPushing)
                {
                    isJumping = true;
                    isPushing = true;
                }
            }
            else
            {
                isPushing = false;
            }
            UpdateAnimator();
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
            isGround = true;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
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
                rbody.AddForce(new Vector2(0, jumpPower), ForceMode2D.Impulse);
                isJumping = false;
            }
        }
    }    
}
