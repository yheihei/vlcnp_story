using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Movement
{
    public class FrontCollisionDetector : MonoBehaviour
    {
        [SerializeField] private String targetTag = "Ground";
        private bool isColliding = false;
        public bool IsColliding => isColliding;

        private void OnTriggerStay2D(Collider2D other)
        {
            if (isColliding) return;
            if (other.gameObject.CompareTag(targetTag))
            {
                isColliding = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag(targetTag))
            {
                isColliding = false;
            }
        }
    }
}
