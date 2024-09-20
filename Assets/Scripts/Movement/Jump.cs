using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Core;

namespace VLCNP.Movement
{
    public class Jump : MonoBehaviour, IStoppable
    {
        [SerializeField] Leg leg;
        Rigidbody2D rBody;
        bool isJumping = false;

        [SerializeField, Min(0)] float jumpPower = 7.5f;
        [SerializeField, Min(0)] float minJumpPower = 2f;
        float jumpTime = 0;
        [SerializeField, Min(0)] float maxJumpTime = 0.3f;
        [SerializeField] AnimationCurve jumpCurve = new();

        private bool isStopped = false;
        public bool IsStopped { get => isStopped; set => isStopped = value; }
        public string jumpButton = "space";

        private void Awake()
        {
            rBody = GetComponent<Rigidbody2D>();
            leg.OnLanded += OnLanded;
        }

        void Update()
        {
            if (isStopped)
            {
                EndJump();
                return;
            }
            // ジャンプの開始判定
            if (leg.IsGround && Input.GetKeyDown(jumpButton))
            {
                isJumping = true;
            }
            // ジャンプ中の処理
            if (isJumping)
            {
                if (Input.GetKeyUp(jumpButton) || jumpTime >= maxJumpTime)
                {
                    EndJump();
                }
                else if (Input.GetKey(jumpButton))
                {
                    jumpTime += Time.deltaTime;
                }
            }
        }

        public void EndJump()
        {
            isJumping = false;
            jumpTime = 0;
        }

        private void OnLanded()
        {
            EndJump();
            // 落下速度がマイナスで着地の際は落下速度を0にしてバウンドを回避
            if (rBody.velocity.y < 0)
            {
                rBody.velocity = new Vector2(rBody.velocity.x, 0);
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
            rBody.AddForce(power * Vector2.up, ForceMode2D.Impulse);
            if (t >= 1)
            {
                EndJump();
            }
        }
    }
}
