using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using VLCNP.Attributes;

namespace VLCNP.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] float speed = 30;
        [SerializeField] GameObject hitEffect = null;
        bool isLeft = false;
        public bool IsLeft { get => isLeft; set => isLeft = value; }

        [SerializeField] float deleteTime = 0.18f;
        [SerializeField] string targetTagName = "Enemy";
        float damage = 0;
        private ParticleSystem particle;

        private void Start()
        {
            SetParticleVelocity();
            Destroy(gameObject, deleteTime);
        }

        private void SetParticleVelocity()
        {
            particle = GetComponent<ParticleSystem>();
            if (particle == null) return;
            // 進行方向の逆にパーティクルを伸ばす
            ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particle.velocityOverLifetime;
            velocityOverLifetime.x = isLeft ? 100f : -100f;
        }

        public void SetDamage(float damage)
        {
            this.damage = damage;
        }

        private void FixedUpdate()
        {
            int directionX = isLeft ? -1 : 1;
            transform.Translate(directionX * speed / 50, 0, 0);
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
