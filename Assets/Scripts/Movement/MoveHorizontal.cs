using UnityEngine;

namespace VLCNP.Movement
{
    public class MoveHorizontal : MonoBehaviour, IMoveAction
    {
        private Rigidbody2D rBody;
        [SerializeField] float speed = 2;
        float velocityX = 0;
        bool isGround = false;
        Animator animator;

        private void Awake() {
            rBody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            isGround = true;
            UpdateJumpAnimation();
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            isGround = false;
            UpdateJumpAnimation();
        }

        public void Move()
        {
            velocityX = speed;
            rBody.velocity = new Vector2(velocityX, rBody.velocity.y);
            animator.SetFloat("vx", velocityX);
        }

        private void UpdateJumpAnimation()
        {
            animator.SetBool("isGround", isGround);
        }
    }
}
