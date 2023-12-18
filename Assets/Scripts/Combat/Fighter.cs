using System;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine;
using VLCNP.Attributes;
using VLCNP.Saving;
using VLCNP.Stats;

namespace VLCNP.Combat
{
    public class Fighter : MonoBehaviour, IJsonSaveable
    {
        [SerializeField] WeaponConfig defaultWeaponConfig = null;
        [SerializeField] WeaponConfig directAttackWeaponConfig = null;
        [SerializeField] Transform handTransform = null;
        [SerializeField] bool canVerticalShot = true;

        WeaponConfig currentWeaponConfig;
        BaseStats baseStats;

        protected void Awake() {
            EquipWeapon(defaultWeaponConfig);
            baseStats = GetComponent<BaseStats>();
        }

        public void WeaponUp()
        {
            if (!canVerticalShot) return;
            handTransform.rotation = GetIsLeft() ? Quaternion.Euler(0, 0, -90) : Quaternion.Euler(0, 0, 90);
        }

        public void WeaponDown()
        {
            if (!canVerticalShot) return;
            handTransform.rotation = GetIsLeft() ? Quaternion.Euler(0, 0, 90) : Quaternion.Euler(0, 0, -90);
        }

        public void WeaponHorizontal()
        {
            handTransform.rotation = Quaternion.Euler(0, 0, 0);
        }

        public void EquipWeapon(WeaponConfig weaponConfig)
        {
            if (weaponConfig == null) return;
            currentWeaponConfig = weaponConfig;
            currentWeaponConfig.Spawn(handTransform);
            WeaponHorizontal();
        }

        public void Attack()
        {
            int level = baseStats ? baseStats.GetLevel() : 1;
            if (!currentWeaponConfig.HasProjectile()) return;
            currentWeaponConfig.LaunchProjectile(handTransform, level, GetIsLeft());
        }

        public void DirectAttack(GameObject target = null)
        {
            if (directAttackWeaponConfig == null) return;
            int level = baseStats ? baseStats.GetLevel() : 1;
            target.GetComponent<Health>().TakeDamage(directAttackWeaponConfig.GetDamage(level));
        }

        private bool GetIsLeft()
        {
            // 左向きキャラクターをlocalScaleで反転させているため、右を向いているときscaleが-1になる
            return transform.lossyScale.x > 0;
        }

        public JToken CaptureAsJToken()
        {
            return JToken.FromObject(currentWeaponConfig.name);
        }

        public void RestoreFromJToken(JToken state)
        {
            WeaponConfig weaponConfig = Resources.Load<WeaponConfig>(state.ToObject<string>());
            if (weaponConfig == null) return;
            EquipWeapon(weaponConfig);
        }
    }
}