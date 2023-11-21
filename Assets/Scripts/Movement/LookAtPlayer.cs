using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Control;

namespace VLCNP.Movement
{
    public class LookAtPlayer : MonoBehaviour
    {
        Transform playerTransform;
        PartyCongroller partyCongroller;

        void Awake()
        {
            SetPlayerTransform(null);
        }

        private void SetPlayerTransform(GameObject player)
        {
            if (player != null) 
            {
                playerTransform = player.transform;
                return;
            }
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        void OnEnable()
        {
            // PartyタグのオブジェクトからPartyCongrollerを取得
            partyCongroller = GameObject.FindGameObjectWithTag("Party")?.GetComponent<PartyCongroller>();
            if (partyCongroller == null) return;
            partyCongroller.OnChangeCharacter += SetPlayerTransform;
        }

        void OnDisable()
        {
            partyCongroller = GameObject.FindGameObjectWithTag("Party")?.GetComponent<PartyCongroller>();
            if (partyCongroller == null) return;
            partyCongroller.OnChangeCharacter -= SetPlayerTransform;
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


