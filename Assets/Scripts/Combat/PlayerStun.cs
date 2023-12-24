using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Combat
{
    public class PlayerStun : MonoBehaviour
    {
        public float stunDuration = 0.5f;

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

        private IEnumerator StunRecovery()
        {
            yield return new WaitForSeconds(stunDuration);
            isStunned = false;
        }
    }    
}
