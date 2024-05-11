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
            Health health = other.gameObject.GetComponent<Health>();
            if (health == null) return;
            health.TakeDamage(weaponConfig.GetDamage());
        }
    }
}
