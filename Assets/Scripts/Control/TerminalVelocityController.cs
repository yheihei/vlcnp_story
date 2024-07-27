using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Control
{
    public class TerminalVelocityController : MonoBehaviour
    {
        [SerializeField, Min(0)] float terminalAbsoluteVelocity = 13f;

        private Rigidbody2D rb;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        void FixedUpdate()
        {
            // y方向の落下速度がterminalAbsoluteVelocityを超えないようにする
            if (rb.velocity.y < -terminalAbsoluteVelocity)
            {
                rb.velocity = new Vector2(rb.velocity.x, -terminalAbsoluteVelocity);
            }
        }
    }    
}
