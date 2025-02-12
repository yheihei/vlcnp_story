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
                IWaterEventListener[] waterEventListeners =
                    other.GetComponentsInChildren<IWaterEventListener>();
                foreach (IWaterEventListener waterEventListener in waterEventListeners)
                {
                    waterEventListener.OnWaterEnter();
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.tag == "Player")
            {
                IWaterEventListener[] waterEventListeners =
                    other.GetComponentsInChildren<IWaterEventListener>();
                foreach (IWaterEventListener waterEventListener in waterEventListeners)
                {
                    waterEventListener.OnWaterExit();
                }
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.gameObject.tag == "Player")
            {
                IWaterEventListener[] waterEventListeners =
                    other.GetComponentsInChildren<IWaterEventListener>();
                foreach (IWaterEventListener waterEventListener in waterEventListeners)
                {
                    waterEventListener.OnWaterStay();
                }
            }
        }
    }
}
