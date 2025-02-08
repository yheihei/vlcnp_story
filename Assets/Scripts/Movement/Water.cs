using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Movement
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Water : MonoBehaviour
    {
        float waterGravityScale = 2f / 9f;
        float originalGravityScale = 0;

        private void Start()
        {
            // Playerを探してoriginalGravityScaleを設定
            GameObject player = GameObject.FindWithTag("Player");
            originalGravityScale = player.GetComponent<Rigidbody2D>().gravityScale;
        }

        // プレイヤーが入ってきたらプレイヤーの重力を1/9にする
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.tag == "Player")
            {
                // 子オブジェクトからKabeKickEffectControllerを取得して無効化
                KabeKickEffectController kabeKickEffectController =
                    other.GetComponentInChildren<KabeKickEffectController>();
                if (kabeKickEffectController != null)
                {
                    kabeKickEffectController.Stop();
                }
                Rigidbody2D rbody = other.GetComponent<Rigidbody2D>();
                rbody.gravityScale = waterGravityScale;
                // プレイヤーのスピードを1/9に減速
                rbody.velocity = rbody.velocity / 3;
            }
        }

        // プレイヤーが出て行ったらプレイヤーの重力を元に戻す
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.tag == "Player")
            {
                // 子オブジェクトからKabeKickEffectControllerを取得して有効化
                KabeKickEffectController kabeKickEffectController =
                    other.GetComponentInChildren<KabeKickEffectController>();
                if (kabeKickEffectController != null)
                {
                    kabeKickEffectController.Restart();
                }
                Rigidbody2D rbody = other.GetComponent<Rigidbody2D>();
                rbody.gravityScale = originalGravityScale;
            }
        }
    }
}
