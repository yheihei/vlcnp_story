using System;
using Unity.VisualScripting;
using UnityEngine;
using VLCNP.Attributes;
using VLCNP.Stats;

namespace VLCNP.Combat
{
    public class Fighter : MonoBehaviour
    {
        [SerializeField] WeaponConfig defaultWeaponConfig = null;
        [SerializeField] WeaponConfig directAttackWeaponConfig = null;
        [SerializeField] Transform handTransform = null;

        WeaponConfig currentWeaponConfig;
        BaseStats baseStats;

        private void Awake() {
            currentWeaponConfig = defaultWeaponConfig;
            EquipWeapon(currentWeaponConfig);
            baseStats = GetComponent<BaseStats>();
        }

        public void WeaponUp()
        {
            handTransform.rotation = GetIsLeft() ? Quaternion.Euler(0, 0, -90) : Quaternion.Euler(0, 0, 90);
        }

        public void WeaponDown()
        {
            handTransform.rotation = GetIsLeft() ? Quaternion.Euler(0, 0, 90) : Quaternion.Euler(0, 0, -90);
        }

        public void WeaponHorizontal()
        {
            handTransform.rotation = Quaternion.Euler(0, 0, 0);
        }

        private void EquipWeapon(WeaponConfig weaponConfig)
        {
            if (weaponConfig == null) return;
            weaponConfig.Spawn(handTransform);
        }

        public void Attack()
        {
            int level = baseStats ? baseStats.GetLevel() : 1;
            if (!currentWeaponConfig.HasProjectile()) return;
            currentWeaponConfig.LaunchProjectile(handTransform, level, GetIsLeft());
        }

        public void DirectAttack(GameObject target = null)
        {
            int level = baseStats ? baseStats.GetLevel() : 1;
            target.GetComponent<Health>().TakeDamage(directAttackWeaponConfig.GetDamage(level));
        }

        private bool GetIsLeft()
        {
            // 左向きキャラクターをlocalScaleで反転させているため、右を向いているときscaleが-1になる
            return transform.lossyScale.x > 0;
        }
    }
}