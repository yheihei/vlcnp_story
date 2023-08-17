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
        [SerializeField] float deleteTime = 0.18f;
        [SerializeField] string targetTagName = "Enemy";
        float damage = 0;
        private ParticleSystem particle;

        private void Start()
        {
            SetParticleVelocity();
            Destroy(this.gameObject, deleteTime);
        }

        // TODO: ProjectileをPlayer配下にだしているため、右を向きながら発射し、すぐ左に向くと弾が左に瞬間移動する
        private bool IsLeft()
        {
            return transform.lossyScale.x > 0;
        }

        private void SetParticleVelocity()
        {
            particle = GetComponent<ParticleSystem>();
            if (particle == null) return;
            // 進行方向の逆にパーティクルを伸ばす
            ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particle.velocityOverLifetime;
            velocityOverLifetime.x = IsLeft() ? 100f : -100f;
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
