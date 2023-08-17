using System;
using System.Drawing.Printing;
using Fungus;
using UnityEngine;
using VLCNP.Stats;

namespace VLCNP.Combat
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/Make New Weapon", order = 0)]
    public class WeaponConfig : ScriptableObject
    {
        // [SerializeField] AnimatorOverrideController animatorOverride = null;
        [SerializeField] Weapon equieppedPrefab = null;
        [SerializeField] float[] weaponDamages = new float[] { 2f, 4f, 8f };
        [SerializeField] Projectile[] projectiles;

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

        public bool HasProjectile()
        {
            return projectiles.Length > 0;
        }

        public void LaunchProjectile(Transform handTransform, int level = 1)
        {
            Projectile projectileInstance = Instantiate(GetCurrentLevelProjectile(level), handTransform);
            projectileInstance.SetDamage(weaponDamages[GetCurrentLevelIndex(level)]);
        }

        public float GetDamage(int level = 1)
        {
            return weaponDamages[GetCurrentLevelIndex(level)];
        }

        private int GetCurrentLevelIndex(int level)
        {
            // 武器のMaxレベル以上にはならない
            return Math.Min(level, weaponDamages.Length) - 1;
        }

        private Projectile GetCurrentLevelProjectile(int level)
        {
            // 武器のMaxレベル以上にはならない
            int index = Math.Min(level, projectiles.Length) - 1;
            return projectiles[index];
        }
    }    
}
