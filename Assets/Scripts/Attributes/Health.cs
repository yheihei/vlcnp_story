using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Stats;

namespace VLCNP.Attributes
{
    public class Health : MonoBehaviour
    {
        float healthPoints = -1f;

        bool isDead = false;

        public bool IsDead { get => isDead; set => isDead = value; }

        private void Awake() {
            healthPoints = GetComponent<BaseStats>().GetStat(Stat.Health);
            print("Health: " + healthPoints);
        }
    }    
}
