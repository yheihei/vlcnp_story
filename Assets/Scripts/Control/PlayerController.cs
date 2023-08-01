using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Movement;

namespace VLCNP.Control
{
    public class PlayerController : MonoBehaviour
    {
        Mover mover;

        private void Awake() {
            mover = GetComponent<Mover>();
        }

        void Update()
        {
            mover.Move();
        }
    }    
}
