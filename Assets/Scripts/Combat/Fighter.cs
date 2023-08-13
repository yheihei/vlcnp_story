using UnityEngine;
using VLCNP.Attributes;

namespace VLCNP.Combat
{
    public class Fighter : MonoBehaviour
    {
        [SerializeField] WeaponConfig defaultWeaponConfig = null;
        [SerializeField] WeaponConfig directAttackWeaponConfig = null;
        [SerializeField] Transform handTransform = null;

        WeaponConfig currentWeaponConfig;

        private void Awake() {
            currentWeaponConfig = defaultWeaponConfig;
            EquipWeapon(currentWeaponConfig);
        }

        private void EquipWeapon(WeaponConfig weaponConfig)
        {
            if (weaponConfig == null) return;
            weaponConfig.Spawn(handTransform);
        }

        public void Attack(GameObject target = null)
        {
            if (currentWeaponConfig.HasProjectile()) {
                currentWeaponConfig.LaunchProjectile(handTransform);
            } else {
                target.GetComponent<Health>().TakeDamage(currentWeaponConfig.GetDamage());
            }
        }

        public void DirectAttack(GameObject target = null)
        {
            target.GetComponent<Health>().TakeDamage(directAttackWeaponConfig.GetDamage());
        }
    }
}