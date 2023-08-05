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

        private void Start()
        {
            Destroy(this.gameObject, deleteTime);
        }

        private void FixedUpdate()
        {
            float vx = isLeft ? (-1) * speed : speed;
            transform.Translate(vx / 50, 0, 0);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                // BeamStatus beamStatus = GetComponent<BeamStatus>();
                // EnemyStatus enemyStatus = collision.gameObject.GetComponent<EnemyStatus>();
                // enemyStatus.AddDamage(beamStatus);
                print("hit!");
                Destroy(this.gameObject);
            }
        }
    }    
}
