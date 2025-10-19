using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VLCNP.Control;
using VLCNP.Combat;
using VLCNP.Saving;

namespace VLCNP.Movement
{
    public class Mover : MonoBehaviour, IWaterEventListener
    {
        [SerializeField]
        float speed = 4;
        float speedInWater = 0;
        bool isLeft = true;
        float speedModifier = 1f;

        // isLeftのgetterを定義
        public bool IsLeft
        {
            get => isLeft;
            set => isLeft = value;
        }

        [SerializeField]
        Leg leg;

        [SerializeField]
        KabeKickEffectController kabeKickEffectController;
        float vx = 0;
        Rigidbody2D rbody;
        Animator animator;
        PlayerStun playerStun;
        Dash dash;
        bool isInWater = false;
        float defaultGravityScale = 0;

        private void Awake()
        {
            rbody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            playerStun = GetComponent<PlayerStun>();
            dash = GetComponent<Dash>();
            defaultGravityScale = rbody.gravityScale;
            speedInWater = speed / 2;
        }

        public void Move()
        {
            vx = 0;
            // Stun状態の場合は移動不可
            if (isStunned())
                return;
            float horizontal = PlayerInputAdapter.GetMoveHorizontal();
            vx = GetSpeed() * horizontal;
            if (horizontal > 0.01f)
            {
                isLeft = false;
            }
            else if (horizontal < -0.01f)
            {
                isLeft = true;
            }
            UpdateAnimator();
        }

        private float GetSpeed()
        {
            float currentSpeed = isInWater ? speedInWater : speed;
            return currentSpeed * speedModifier;
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

        public IEnumerator MoveToRelativePosition(Vector3 position, float timeout = 0)
        {
            yield return MoveToPosition(transform.position + position, timeout);
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
            if (isStunned())
                return;
            // ダッシュ中は移動不可
            if (dash != null && dash.IsDashing)
                return;
            // カベキック中は移動不可
            if (kabeKickEffectController != null && kabeKickEffectController.IsJumping)
                return;
            // カベをつかんでいたら移動不可
            if (kabeKickEffectController != null && kabeKickEffectController.IsGrabbing())
                return;
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
                transform.localScale = new Vector3(
                    Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
            else
            {
                transform.localScale = new Vector3(
                    -1 * Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
        }

        public void OnWaterEnter()
        {
            isInWater = true;
            rbody.velocity = rbody.velocity / 2;
        }

        public void OnWaterExit()
        {
            isInWater = false;
            rbody.gravityScale = defaultGravityScale;
        }

        public void OnWaterStay()
        {
            if (!isInWater)
                return;
            rbody.gravityScale = 2f / 9f;
        }

        // PoisonStatusから呼び出されるメソッド
        public void SetSpeedModifier(float modifier)
        {
            speedModifier = modifier;
        }
    }
}
