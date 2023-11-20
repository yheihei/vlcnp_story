using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Movement
{
    public class LookAtPlayer : MonoBehaviour
    {
        Transform playerTransform;

        void Awake()
        {
            // PlayerタグからプレイヤーのTransformを取得
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        void FixedUpdate()
        {
            Look();
        }

        private void Look()
        {
            if (playerTransform == null) return;
            // Playerの位置と自分の位置を比較して、Playerの方を向く
            if (playerTransform.position.x > transform.position.x)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                return;
            }
            transform.localScale = new Vector3(-1 * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }
}


