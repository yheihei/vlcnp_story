using System;
using System.Drawing.Printing;
using Fungus;
using Unity.VisualScripting;
using UnityEngine;
using VLCNP.Stats;

namespace VLCNP.Combat
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/Make New Weapon", order = 0)]
    public class WeaponConfig : ScriptableObject
    {
        // [SerializeField] AnimatorOverrideController animatorOverride = null;
        [SerializeField] Weapon equieppedPrefab = null;
        [SerializeField] WeaponLevel[] weaponLevels = null;

        [System.Serializable]
        class WeaponLevel
        {
            [SerializeField] public float damage;
            [SerializeField] public Projectile projectile;
        }

        const string weaponName = "Weapon";

        public Weapon Spawn(Transform handTransform)
        {
            // DestroyOldWeapon(rightHand, leftHand);
            Weapon weapon = null;

            if (equieppedPrefab != null)
            {
                weapon = Instantiate(equieppedPrefab, handTransform);
                weapon.gameObject.name = weaponName;
            }
            // if (animatorOverride != null)
            // {
            //     animator.runtimeAnimatorController = animatorOverride;
            // }
            // else
            // {
            //     var overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
            //     if (overrideController != null)
            //     {
            //         animator.runtimeAnimatorController = overrideController.runtimeAnimatorController;
            //     }
            // }
            return weapon;
        }

        // private void DestroyOldWeapon(Transform rightHand, Transform leftHand)
        // {
        //     Transform oldWeapon = rightHand.Find(weaponName);
        //     if (oldWeapon == null)
        //     {
        //         oldWeapon = leftHand.Find(weaponName);
        //     }
        //     if (oldWeapon == null) return;
        //     oldWeapon.name = "DESTROYING";
        //     Destroy(oldWeapon.gameObject);
        // }

        // private Transform GetTransform(Transform rightHand, Transform leftHand)
        // {
        //     return isRightHanded ? rightHand : leftHand;
        // }

        public bool HasProjectile(int level = 1)
        {
            WeaponLevel _weaponLevel = GetCurrentWeapon(level);
            if (_weaponLevel == null) return false;
            return _weaponLevel.projectile != null;
        }

        public void LaunchProjectile(Transform handTransform, int level = 1, bool isLeft = false)
        {
            WeaponLevel _weaponLevel = GetCurrentWeapon(level);
            Projectile projectileInstance = Instantiate(_weaponLevel.projectile, handTransform.position, handTransform.rotation);
            // projectileInstance.IsLeft = isLeft;
            projectileInstance.SetDirection(isLeft);
            projectileInstance.SetDamage(_weaponLevel.damage);
        }

        private WeaponLevel GetCurrentWeapon(int level = 1)
        {
            if (weaponLevels.Length == 0) return null;
            // 武器のMaxレベル以上にはならない
            return weaponLevels[Math.Min(level, weaponLevels.Length) - 1];
        }

        public float GetDamage(int level = 1)
        {
            WeaponLevel _weaponLevel = GetCurrentWeapon(level);
            if (_weaponLevel == null) return 0;
            return _weaponLevel.damage;
        }
    }    
}
