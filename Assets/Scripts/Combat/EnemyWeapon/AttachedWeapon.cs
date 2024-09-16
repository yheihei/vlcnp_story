using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using VLCNP.Attributes;

namespace VLCNP.Combat.EnemyWeapon
{
    public class AttachedWeapon : MonoBehaviour
    {
        [SerializeField] WeaponConfig weaponConfig = null;
        [SerializeField] string targetTagName = "Player";

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.gameObject.CompareTag(targetTagName)) return;
            Attack(other.gameObject.GetComponent<Health>());
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.gameObject.CompareTag(targetTagName)) return;
            Attack(other.gameObject.GetComponent<Health>());
        }

        private void Attack(Health health)
        {
            if (health == null) return;
            health.TakeDamage(weaponConfig.GetDamage());
        }  
    }
}
