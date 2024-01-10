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
        [SerializeField] Rigidbody2D player = null;
        // エフェクトが最後に発生した後の経過時間
        float effectElapsedTime = 0f;

        // 壁に接触しているかどうか
        bool isColliding = false;

        void OnTriggerStay2D(Collider2D other)
        {
            if (other.gameObject.tag == "Ground")
            {
                isColliding = true;
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.tag == "Ground")
            {
                isColliding = false;
            }
        }

        void FixedUpdate()
        {
            // 壁に接触していなければ何もしない
            if (!isColliding) {
                effectElapsedTime = 0f;
                return;
            }
            // x方向に移動していれば壁ではないので何もしない
            if (player.velocity.x != 0f)
            {
                effectElapsedTime = 0f;
                return;
            }
            // y方向の速度が一定以上であれば何もしない
            if (player.velocity.y >= -0.05f)
            {
                effectElapsedTime = 0f;
                return;
            }
            // エフェクトが最後に発生してからの経過時間がeffectWaitTimeより小さければ何もしない
            if (effectElapsedTime < effectWaitTime)
            {
                effectElapsedTime += Time.deltaTime;
                return;
            }
            // エフェクトを発生させる
            Instantiate(effect, transform.position, Quaternion.identity);
            effectElapsedTime = 0f;
        }
    }    
}
