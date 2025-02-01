using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace VLCNP.Combat
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/Make New Weapon", order = 0)]
    public class WeaponConfig : ScriptableObject
    {
        [SerializeField]
        Weapon equippedPrefab = null;

        [SerializeField]
        WeaponLevel[] weaponLevels = null;

        [System.Serializable]
        class WeaponLevel
        {
            [SerializeField]
            public float damage;

            [SerializeField]
            public Projectile projectile;
        }

        const string weaponName = "Weapon";

        public Weapon Spawn(Transform handTransform)
        {
            // DestroyOldWeapon(rightHand, leftHand);
            Weapon weapon = null;

            if (equippedPrefab != null)
            {
                weapon = Instantiate(equippedPrefab, handTransform);
                weapon.gameObject.name = weaponName;
            }
            return weapon;
        }

        public bool HasProjectile(int level = 1)
        {
            WeaponLevel _weaponLevel = GetCurrentWeapon(level);
            if (_weaponLevel == null)
                return false;
            return _weaponLevel.projectile != null;
        }

        public void LaunchProjectile(Transform handTransform, int level = 1, bool isLeft = false)
        {
            WeaponLevel _weaponLevel = GetCurrentWeapon(level);
            Projectile projectileInstance = Instantiate(
                _weaponLevel.projectile,
                handTransform.position,
                handTransform.rotation
            );
            AudioClip clip = projectileInstance.GetComponent<AudioSource>()?.clip;
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, handTransform.position);
            }
            projectileInstance.SetDirection(isLeft);
            projectileInstance.SetDamage(_weaponLevel.damage);
        }

        private WeaponLevel GetCurrentWeapon(int level = 1)
        {
            if (weaponLevels.Length == 0)
                return null;
            // 武器のMaxレベル以上にはならない
            return weaponLevels[Math.Min(level, weaponLevels.Length) - 1];
        }

        public float GetDamage(int level = 1)
        {
            WeaponLevel _weaponLevel = GetCurrentWeapon(level);
            if (_weaponLevel == null)
                return 0;
            return _weaponLevel.damage;
        }
    }
}
