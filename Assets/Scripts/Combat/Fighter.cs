using UnityEngine;

namespace VLCNP.Combat
{
    public class Fighter : MonoBehaviour
    {
        [SerializeField] WeaponConfig defaultWeaponConfig = null;
        [SerializeField] Transform handTransform = null;

        WeaponConfig currentWeaponConfig;

        private void Awake() {
            currentWeaponConfig = defaultWeaponConfig;
            EquipWeapon(currentWeaponConfig);
        }

        private void EquipWeapon(WeaponConfig weaponConfig)
        {
            weaponConfig.Spawn(handTransform);
        }

        public void Attack(bool isLeft)
        {
            if (!Input.GetKeyUp("x")) return;
            if (!currentWeaponConfig.HasProjectile()) return;
            currentWeaponConfig.LaunchProjectile(handTransform);
        }
    }
}