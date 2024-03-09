using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Movement
{
    public class Leg : MonoBehaviour
    {
        bool isGround = false;

        public bool IsGround { get => isGround; set => isGround = value; }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!collision.tag.Equals("Ground") && !collision.tag.Equals("Enemy"))
            {
                return;
            }
            IsGround = true;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.tag.Equals("Ground") && !collision.tag.Equals("Enemy"))
            {
                return;
            }
            IsGround = false;
        }
    }
}
