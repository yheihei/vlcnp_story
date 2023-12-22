using System;
using Unity.VisualScripting;
using UnityEngine;
using VLCNP.Control;

namespace VLCNP.Movement
{
    public class BossVeryEnemyAnimals : MonoBehaviour, IMoveAction
    {
        private Rigidbody2D rBody;
        [SerializeField] float jumpIntervalSecond = 3f;
        [SerializeField] float jumpPower = 8;

        [SerializeField] float velocityX = 0;

        [SerializeField] float enemyDetectionRange = 12f;
        private float timeSinceLastJump = Mathf.Infinity;
        bool isGround = false;
        Animator animator;
        GameObject player;
        PartyCongroller partyCongroller;
        private int jumpCount = 1;

        private void Awake() {
            rBody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            player = GameObject.FindGameObjectWithTag("Player");
        }

        private void SetPlayer(GameObject player)
        {
            this.player = player;
        }

        void OnEnable()
        {
            // PartyタグのオブジェクトからPartyCongrollerを取得
            partyCongroller = GameObject.FindGameObjectWithTag("Party")?.GetComponent<PartyCongroller>();
            if (partyCongroller == null) return;
            partyCongroller.OnChangeCharacter += SetPlayer;
        }

        void OnDisable()
        {
            partyCongroller = GameObject.FindGameObjectWithTag("Party")?.GetComponent<PartyCongroller>();
            if (partyCongroller == null) return;
            partyCongroller.OnChangeCharacter -= SetPlayer;
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
            if (!isDetectPlayer()) return;
            if (!isMove()) return;
            if (jumpCount % 3 == 0)
            {
                Tackle();
                return;
            }
            Jump();
        }

        private bool isMove()
        {
            return timeSinceLastJump > jumpIntervalSecond && isGround;
        }

        private bool isDetectPlayer()
        {
            // playerとの距離を出す
            if (player == null) return false;
            float distance = Vector2.Distance(player.transform.position, transform.position);
            return distance < enemyDetectionRange;
        }

        private void Jump()
        {
            jumpCount++;
            rBody.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            // 向きをscaleから取得
            bool isRight = transform.localScale.x > 0;
            rBody.velocity = new Vector2(isRight ? velocityX : -1 * velocityX, rBody.velocity.y);
            timeSinceLastJump = 0f;
        }

        private void Tackle()
        {
            jumpCount++;
            rBody.AddForce(Vector2.up * jumpPower/2, ForceMode2D.Impulse);
            // 向きをscaleから取得
            bool isRight = transform.localScale.x > 0;
            rBody.velocity = new Vector2(isRight ? velocityX * 1.5f : -1.5f * velocityX, rBody.velocity.y);
            timeSinceLastJump = 0f;
        }

        private void UpdateJumpAnimation()
        {
            animator.SetBool("isGround", isGround);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, enemyDetectionRange);
        }
    }
}
