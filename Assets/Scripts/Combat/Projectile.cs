using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Attributes;

namespace VLCNP.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] float speed = 30;
        [SerializeField] GameObject hitEffect = null;
        [SerializeField] float deleteTime = 0.18f;
        [SerializeField] bool isLeft = true;
        [SerializeField] string targetTagName = "Enemy";
        float damage = 0;

        private void Start()
        {
            Destroy(this.gameObject, deleteTime);
        }

        public void SetDamage(float damage)
        {
            this.damage = damage;
        }

        private void FixedUpdate()
        {
            transform.Translate((-1) * speed / 50, 0, 0);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag(targetTagName))
            {
                Health health = other.gameObject.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                }
                Destroy(this.gameObject);
            }
        }
    }    
}
