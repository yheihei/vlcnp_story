using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        float gravityWhenKabeKickMagnification = 0.3f;
        // 元の重力
        float originalGravity = 0f;

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

        bool isKabekick()
        {
            // 地面についている間はカベキックしない
            if (leg.IsGround)
            {
                return false;
            }
            return isColliding;
        }

        void FixedUpdate()
        {
            if (isStopped) return;
            GravityChange();
            UpdateAnimation();
            if (!CheckEffecting()) return;
            InstantiateEffect();
        }

        private void UpdateAnimation()
        {
            animator.SetBool("isKabe", isKabekick());
        }

        private void GravityChange()
        {
            if (!isKabekick())
            {
                playerRigidbody2D.gravityScale = originalGravity;
                return;
            }
            // 壁に接触していて、落下中であれば重力を減らす
            if (playerRigidbody2D.velocity.y < 0)
            {
                playerRigidbody2D.gravityScale = originalGravity * gravityWhenKabeKickMagnification;
            }
            else
            {
                playerRigidbody2D.gravityScale = originalGravity;
            }
        }

        bool CheckEffecting()
        {
            if (!isKabekick())
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
