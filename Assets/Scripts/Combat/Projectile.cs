using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] float speed = 30;
        [SerializeField] GameObject hitEffect = null;
        [SerializeField] float deleteTime = 0.18f;
        [SerializeField] bool isLeft = true;
        [SerializeField] string targetTagName = "Enemy";

        private void Start()
        {
            Destroy(this.gameObject, deleteTime);
        }

        private void FixedUpdate()
        {
            float vx = isLeft ? (-1) * speed : speed;
            transform.Translate(vx / 50, 0, 0);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag(targetTagName))
            {
                print("hit!");
                Destroy(this.gameObject);
            }
        }
    }    
}
