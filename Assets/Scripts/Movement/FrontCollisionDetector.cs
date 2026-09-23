using System;
using System.Collections;
using UnityEngine;

namespace VLCNP.Movement
{
    public class FrontCollisionDetector : MonoBehaviour
    {
        [SerializeField]
        String[] targetTags = new String[] { "Ground", "Item", "Enemy" };

        private bool isColliding = false;
        public bool IsColliding => isColliding;

        // tag プロパティは呼ぶたびに文字列を生成するので CompareTag で比べる
        private bool IsTarget(Collider2D other)
        {
            foreach (string targetTag in targetTags)
            {
                if (other.CompareTag(targetTag))
                    return true;
            }
            return false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsTarget(other))
                return;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!IsTarget(other))
                return;
            if (isColliding)
                return;
            isColliding = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsTarget(other))
                return;
            isColliding = false;
        }
    }
}
