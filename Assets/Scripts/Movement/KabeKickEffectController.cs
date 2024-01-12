using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace VLCNP.Movement
{
    public class KabeKickEffectController : MonoBehaviour
    {
        [SerializeField] GameObject effect = null;
        [SerializeField] float effectWaitTime = 0.5f;
        [SerializeField] GameObject player = null;
        Rigidbody2D playerRigidbody2D;
        Mover playerMover;
        Animator animator;
        // エフェクトが最後に発生した後の経過時間
        float effectElapsedTime = 0f;

        // 壁に接触しているかどうか
        bool isColliding = false;

        void Awake()
        {
            playerRigidbody2D = player.GetComponent<Rigidbody2D>();
            playerMover = player.GetComponent<Mover>();
            animator = player.GetComponent<Animator>();
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (other.gameObject.tag == "Ground")
            {
                SetColliding(true);
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.tag == "Ground")
            {
                SetColliding(false);
            }
        }

        void SetColliding(bool value)
        {
            isColliding = value;
            animator.SetBool("isKabe", value);
        }

        void FixedUpdate()
        {
            if (!CheckEffecting()) return;
            InstantiateEffect();
        }

        bool CheckEffecting()
        {
            // 壁に接触していなければ何もしない
            if (!isColliding) {
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
