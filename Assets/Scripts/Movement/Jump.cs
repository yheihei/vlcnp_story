using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Core;

namespace VLCNP.Movement
{
    public class Jump : MonoBehaviour, IStoppable, IWaterEventListener
    {
        [SerializeField]
        Leg leg;
        Rigidbody2D rBody;
        bool isJumping = false;

        [SerializeField, Min(0)]
        float jumpPower = 7.5f;

        [SerializeField, Min(0)]
        float minJumpPower = 2f;
        float jumpTime = 0;

        [SerializeField, Min(0)]
        float maxJumpTime = 0.3f;

        [SerializeField]
        AnimationCurve jumpCurve = new();

        [SerializeField]
        private AudioSource jumpAudioSource;

        [SerializeField]
        private AudioClip jumpSe = null;

        [SerializeField]
        float jumpPitch = 1f;

        [SerializeField]
        private AudioClip waterJumpSe = null;

        [SerializeField]
        float waterJumpPitch = 0.5f;

        private bool isStopped = false;
        public bool IsStopped
        {
            get => isStopped;
            set => isStopped = value;
        }
        public string jumpButton = "space";
        float defaultJumpPower = 0;
        float waterJumpPower = 0;
        bool isInWater = false;

        private void Awake()
        {
            rBody = GetComponent<Rigidbody2D>();
            leg.OnLanded += OnLanded;
            defaultJumpPower = jumpPower;
            waterJumpPower = jumpPower * 2 / 9f;
        }

        void Update()
        {
            if (LoadCompleteManager.Instance != null && !LoadCompleteManager.Instance.IsLoaded)
                return;
            if (isStopped)
            {
                EndJump();
                return;
            }
            // ジャンプの開始判定
            if (CanJump() && Input.GetKeyDown(jumpButton))
            {
                isJumping = true;
                PlayJumpSound();
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

        private void PlayJumpSound()
        {
            AudioClip _jumpSe = GetJumpSe();
            if (jumpAudioSource != null && _jumpSe != null)
            {
                jumpAudioSource.pitch = GetJumpPitch();
                jumpAudioSource.PlayOneShot(_jumpSe, 0.2f);
            }
        }

        private AudioClip GetJumpSe()
        {
            if (isInWater && waterJumpSe != null)
            {
                return waterJumpSe;
            }
            if (!isInWater && jumpSe != null)
            {
                return jumpSe;
            }
            return null;
        }

        private float GetJumpPitch()
        {
            if (isInWater)
            {
                return waterJumpPitch;
            }
            return jumpPitch;
        }

        public bool CanJump()
        {
            return isInWater || leg.IsGround;
        }

        public void OnWaterEnter() { }

        public void OnWaterExit()
        {
            isInWater = false;
            jumpPower = defaultJumpPower;
        }

        public void OnWaterStay()
        {
            isInWater = true;
            jumpPower = waterJumpPower;
        }
    }
}
