using UnityEngine;

namespace VLCNP.Combat
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/Make New Weapon", order = 0)]
    public class WeaponConfig : ScriptableObject
    {
        // [SerializeField] AnimatorOverrideController animatorOverride = null;
        [SerializeField] Weapon equieppedPrefab = null;

        [SerializeField] float weaponRange = 2f;
        [SerializeField] float weaponDamage = 5f;
        [SerializeField] float percentageBonus = 0;
        [SerializeField] bool isRightHanded = true;
        // [SerializeField] Projectile projectile = null;

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

        // public bool HasProjectile()
        // {
        //     return projectile != null;
        // }

        // public void LaunchProjectile(Transform rightHand, Transform leftHand, Health target, GameObject instigator, float calculatedDamage)
        // {
        //     Projectile projectileInstance = Instantiate(projectile, GetTransform(rightHand, leftHand).position, Quaternion.identity);
        //     projectileInstance.SetTarget(target, instigator, calculatedDamage);
        // }

        // public float GetRange()
        // {
        //     return weaponRange;
        // }

        // public float GetDamage()
        // {
        //     return weaponDamage;
        // }

        // public float GetPercentageBonus()
        // {
        //     return percentageBonus;
        // }
    }    
}
