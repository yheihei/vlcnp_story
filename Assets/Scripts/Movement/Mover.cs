using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Movement
{
    public class Mover : MonoBehaviour
    {
        [SerializeField] float speed = 4;
        bool leftFlag = true;
        float vx = 0;
        Rigidbody2D rbody;
        Animator animator;

        private void Awake() {
            rbody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        public void Move()
        {
            vx = 0;
            if (Input.GetKey("right"))
            {
                vx = speed;
                leftFlag = false;
            }
            if (Input.GetKey("left"))
            {
                vx = -speed;
                leftFlag = true;
            }
            UpdateAnimator();
        }

        private void UpdateAnimator()
        {
            animator.SetFloat("vx", Mathf.Abs(vx));
        }

        private void FixedUpdate()
        {
            UpdateMoveSpeed();
            UpdateCharacterDirection();
        }

        private void UpdateMoveSpeed()
        {
            rbody.velocity = new Vector2(vx, rbody.velocity.y);
        }

        private void UpdateCharacterDirection()
        {
            if (leftFlag)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                transform.localScale = new Vector3(-1 * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }    
}
