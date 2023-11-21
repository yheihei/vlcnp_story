using Unity.VisualScripting;
using UnityEngine;

namespace VLCNP.Movement
{
    public class SometimeJump : MonoBehaviour, IMoveAction
    {
        private Rigidbody2D rBody;
        [SerializeField] float jumpIntervalSecond = 3f;
        [SerializeField] float jumpPower = 8;

        [SerializeField] float velocityX = 0;
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

        private void FixedUpdate() {
            // ジャンプしていなかったら横方向の速度を0に
            if (rBody.velocity.y == 0) rBody.velocity = new Vector2(0, rBody.velocity.y);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            // ground tagじゃなければ無視
            if (collision.gameObject.tag != "Ground") return;
            isGround = true;
            UpdateJumpAnimation();
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            // ground tagじゃなければ無視
            if (collision.gameObject.tag != "Ground") return;
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
            // 向きをscaleから取得
            bool isRight = transform.localScale.x > 0;
            rBody.velocity = new Vector2(isRight ? velocityX : -1 * velocityX, rBody.velocity.y);
            timeSinceLastJump = 0f;
        }

        private void UpdateJumpAnimation()
        {
            animator.SetBool("isGround", isGround);
        }
    }
}
