using UnityEngine;

namespace VLCNP.Movement
{
    public class SometimeJump : MonoBehaviour, IMoveAction
    {
        private Rigidbody2D rBody;
        [SerializeField] float jumpIntervalSecond = 3f;
        [SerializeField] float jumpPower = 8;
        private float timeSinceLastJump = Mathf.Infinity;
        bool isGround = false;
        Animator animator;

        private void Awake() {
            rBody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        private void Update() {
            timeSinceLastJump += Time.deltaTime;
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
            if (timeSinceLastJump > jumpIntervalSecond && isGround)
            {
                Jump();
            }
        }

        private void Jump()
        {
            rBody.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            timeSinceLastJump = 0f;
        }

        private void UpdateJumpAnimation()
        {
            animator.SetBool("isGround", isGround);
        }
    }
}
