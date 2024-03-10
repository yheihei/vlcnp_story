using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Movement
{
    public class Jump : MonoBehaviour
    {
        [SerializeField] Leg leg;
        Rigidbody2D rBody;
        bool isJumping = false;
        // ジャンプボタンが押しっぱなしかどうか。押しっぱなしで連続ジャンプはしない
        bool isJumpButtonKeep = false;

        [SerializeField, Min(0)] float jumpPower = 7.5f;
        [SerializeField, Min(0)] float minJumpPower = 2f;
        float jumpTime = 0;
        [SerializeField, Min(0)] float maxJumpTime = 0.3f;
        [SerializeField] AnimationCurve jumpCurve = new();

        private void Awake()
        {
            rBody = GetComponent<Rigidbody2D>();
            leg.OnLanded += OnLanded;
        }

        void Update()
        {
            // ジャンプボタン押しっぱなしの場合は一度離さないとジャンプできない
            if (Input.GetKeyUp("space"))
            {
                isJumpButtonKeep = false;
            }
            if (isJumpButtonKeep)
            {
                return;
            }

            // ジャンプの開始判定
            if (leg.IsGround && Input.GetKey("space"))
            {
                isJumping = true;
            }
            // ジャンプ中の処理
            if (isJumping)
            {
                if (Input.GetKeyUp("space") || jumpTime >= maxJumpTime)
                {
                    isJumping = false;
                    jumpTime = 0;
                }
                else if (Input.GetKey("space"))
                {
                    jumpTime += Time.deltaTime;
                }
            }
        }

        private void OnLanded()
        {
            // 着地時にジャンプボタン押しっぱなしの場合はジャンプボタン押しっぱなしと判定
            if (Input.GetKey("space"))
            {
                isJumpButtonKeep = true;
            }
        }

        private void FixedUpdate()
        {
            DoJump();
        }

        private void DoJump()
        {
            if (!isJumping)
            {
                return;
            }

            rBody.velocity = new Vector2(rBody.velocity.x, 0);

            // ジャンプの速度をアニメーションカーブから取得
            float t = jumpTime / maxJumpTime;
            float power = jumpPower * jumpCurve.Evaluate(t);
            // 最低ジャンプ力を保証
            power = Mathf.Max(power, minJumpPower);
            if (t >= 1)
            {
                isJumping = false;
                jumpTime = 0;
            }

            rBody.AddForce(power * Vector2.up, ForceMode2D.Impulse);
        }
    }
}
