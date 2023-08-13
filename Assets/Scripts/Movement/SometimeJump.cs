using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace VLCNP.Movement
{
    public class SometimeJump : MonoBehaviour, IMoveAction
    {
        private Rigidbody2D rBody;
        [SerializeField] float jumpIntervalSecond = 3f;
        [SerializeField] float jumpPower = 8;
        private float timeSinceLastJump = Mathf.Infinity;
        bool isGround = false;

        private void Awake() {
            rBody = GetComponent<Rigidbody2D>();
        }

        private void Update() {
            timeSinceLastJump += Time.deltaTime;
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            isGround = true;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            isGround = false;
        }

        public void Move()
        {
            if (timeSinceLastJump > jumpIntervalSecond && isGround)
            {
                Jump();
            }
        }

        private void Jump()
        {
            rBody.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            timeSinceLastJump = 0f;
        }
    }
}
