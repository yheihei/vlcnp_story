using UnityEngine;
using VLCNP.Core;

namespace VLCNP.Movement
{
    public class KabeKickEffectController : MonoBehaviour, IStoppable
    {
        [SerializeField] GameObject effect = null;
        [SerializeField] float effectWaitTime = 0.5f;
        [SerializeField] GameObject player = null;
        [SerializeField] Leg leg = null;
        Rigidbody2D playerRigidbody2D;
        Mover playerMover;
        Animator animator;
        // エフェクトが最後に発生した後の経過時間
        float effectElapsedTime = 0f;

        // 壁に接触しているかどうか
        bool isColliding = false;

        bool isStopped = false;
        public bool IsStopped { get => isStopped; set => isStopped = value; }
        // 壁キック時の重力の倍率
        [SerializeField, Min(0)] float gravityWhenKabeKickMagnification = 0.2f;
        // 元の重力
        float originalGravity = 0f;
        bool isJumping = false;
        public bool IsJumping { get => isJumping; }
        [SerializeField, Min(0)] float maxJumpTime = 0.3f;
        [SerializeField, Min(0)] float jumpPowerX = 2.5f;
        [SerializeField, Min(0)] float jumpPowerY = 8f;
        float jumpTime = 0f;

        void Awake()
        {
            playerRigidbody2D = player.GetComponent<Rigidbody2D>();
            originalGravity = playerRigidbody2D.gravityScale;
            playerMover = player.GetComponent<Mover>();
            animator = player.GetComponent<Animator>();
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (isStopped) return;
            if (other.gameObject.tag == "Ground")
            {
                SetColliding(true);
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (isStopped) return;
            if (other.gameObject.tag == "Ground")
            {
                SetColliding(false);
            }
        }

        void SetColliding(bool value)
        {
            isColliding = value;
        }

        public bool IsKabekick()
        {
            // 地面についている間はカベキックできない
            if (leg.IsGround)
            {
                return false;
            }
            return isColliding;
        }

        void Update()
        {
            // ジャンプの開始判定
            if (IsKabekick() && Input.GetKeyUp("space") && playerRigidbody2D.velocity.y < -0.1)
            {
                isJumping = true;
            }

            if (isJumping)
            {
                if (jumpTime >= maxJumpTime)
                {
                    isJumping = false;
                    jumpTime = 0;
                }
                else
                {
                    jumpTime += Time.deltaTime;
                }
            }
        }

        void FixedUpdate()
        {
            if (isStopped) return;
            GravityChange();
            UpdateAnimation();
            DoJump();
            if (!CheckEffecting()) return;
            InstantiateEffect();
        }

        private void DoJump()
        {
            if (!isJumping)
            {
                return;
            }
            if (playerRigidbody2D.velocity.y >= 0)
            {
                return;
            }
            // ジャンプ前に縦方向の速度を0にする
            playerRigidbody2D.velocity = new Vector2(playerRigidbody2D.velocity.x, 0);
            // 斜め方向にジャンプ 左向きなら右方向に、右向きなら左方向にジャンプ
            float _jumpPowerX = playerMover.IsLeft ? jumpPowerX : -1 * jumpPowerX;
            playerRigidbody2D.AddForce(new Vector2(_jumpPowerX, jumpPowerY), ForceMode2D.Impulse);
        }

        private void UpdateAnimation()
        {
            animator.SetBool("isKabe", IsKabekick());
        }

        private void GravityChange()
        {
            // カベキック中でないか、上昇中の場合は重力は元に戻す
            if (!IsKabekick() || playerRigidbody2D.velocity.y >= 0)
            {
                playerRigidbody2D.gravityScale = originalGravity;
                return;
            }
            // カベキック中かつ落下中であれば重力を減らす
            playerRigidbody2D.velocity = new Vector2(0f, playerRigidbody2D.velocity.y);
            playerRigidbody2D.gravityScale = originalGravity * gravityWhenKabeKickMagnification;
        }

        bool CheckEffecting()
        {
            if (!IsKabekick())
            {
                effectElapsedTime = 0f;
                return false;
            }
            // x方向に移動していれば壁ではないので何もしない
            if (playerRigidbody2D.velocity.x != 0f)
            {
                effectElapsedTime = 0f;
                return false;
            }
            // y方向の速度が一定以上であれば何もしない
            if (playerRigidbody2D.velocity.y >= -0.05f)
            {
                effectElapsedTime = 0f;
                return false;
            }
            // エフェクトが最後に発生してからの経過時間がeffectWaitTimeより小さければ何もしない
            if (effectElapsedTime < effectWaitTime)
            {
                effectElapsedTime += Time.deltaTime;
                return false;
            }
            return true;
        }

        void InstantiateEffect()
        {
            GameObject _effect = Instantiate(
                effect,
                transform.position,
                playerMover.transform.lossyScale.x > 0 ? Quaternion.Euler(10, 90, 0) : Quaternion.Euler(10, -90, 0)
            );
            effectElapsedTime = 0f;
        }
    }
}
