using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Combat;

namespace VLCNP.Control
{
    public class TrapController : MonoBehaviour
    {
        Fighter fighter;
        [SerializeField] string targetTagName = "Player";

        private void Awake() {
            fighter = GetComponent<Fighter>();
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            if (other.gameObject.tag != targetTagName) return;
            fighter.DirectAttack(other.gameObject);
        }
    }
}
