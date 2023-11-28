using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Attributes
{
    public class PlayerHealthWrapper : MonoBehaviour
    {
        public void RestoreHealth()
        {
            // Playerをタグから取得する
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            player.GetComponent<Health>()?.RestoreHealth();
        }
    }
}
