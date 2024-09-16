using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Combat
{
    public class PlayerStun : MonoBehaviour
    {
        public float stunDuration = 0.5f;
        public float stunFrame = 120f;
        private float stunRecoveryFrame = 0;

        private bool isStunned = false;

        public void Stun()
        {
            if (!isStunned) // すでにStunned状態でないことを確認
            {
                isStunned = true;
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
            // yield return new WaitForSeconds(stunDuration);
            // isStunned = false;
            while (stunRecoveryFrame < stunFrame)
            {
                stunRecoveryFrame++;
                yield return null;
            }
            isStunned = false;
        }
    }    
}
