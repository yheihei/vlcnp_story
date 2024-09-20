using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Combat
{
    public class PlayerStun : MonoBehaviour
    {
        public float stunDuration = 0.5f;
        public float stunFrame = 40f;
        private float stunRecoveryFrame = 0;

        private bool isStunned = false;
        Rigidbody2D rb;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogError("Rigidbodyが見つかりません");
            }
        }

        public void Stun()
        {
            if (!isStunned) // すでにStunned状態でないことを確認
            {
                isStunned = true;
                rb.velocity = Vector2.zero;
                StartCoroutine(StunRecovery());
            }
        }

        public bool IsStunned()
        {
            return isStunned;
        }

        public void Set(PlayerStun playerStun)
        {
            stunDuration = playerStun.stunDuration;
            stunFrame = playerStun.stunFrame;
            stunRecoveryFrame = playerStun.stunRecoveryFrame;
            isStunned = playerStun.isStunned;
            if (isStunned)
            {
                StartCoroutine(StunRecovery());
            }
        }

        private IEnumerator StunRecovery()
        {
            while (stunRecoveryFrame < stunFrame)
            {
                stunRecoveryFrame++;
                yield return null;
            }
            isStunned = false;
        }
    }    
}
