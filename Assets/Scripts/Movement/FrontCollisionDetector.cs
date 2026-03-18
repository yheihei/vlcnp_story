using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace VLCNP.Movement
{
    public class FrontCollisionDetector : MonoBehaviour
    {
        [SerializeField]
        String[] targetTags = new String[] { "Ground", "Item", "Enemy" };

        private bool isColliding = false;
        public bool IsColliding => isColliding;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!targetTags.Contains(other.gameObject.tag))
                return;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!targetTags.Contains(other.gameObject.tag))
                return;
            if (isColliding)
                return;
            isColliding = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!targetTags.Contains(other.gameObject.tag))
                return;
            isColliding = false;
        }
    }
}
