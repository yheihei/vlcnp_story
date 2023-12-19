using UnityEngine;
using VLCNP.Core;

namespace VLCNP.Combat
{
public class AkimFighter : Fighter
    {
        // Akimの武器は VeryLongGunEquipped フラグが立ったときのみ有効

        [SerializeField] WeaponConfig akimWeaponConfig = null;
        FlagManager flagManager;
        protected void Awake() {
            base.Awake();
            flagManager = GameObject.FindWithTag("FlagManager").GetComponent<FlagManager>();
        }

        void Start()
        {
            flagManager.OnChangeFlag += OnChangeFlag;
            EquipAkimWeapon();
        }

        void OnChangeFlag(Flag flag)
        {
            EquipAkimWeapon();
        }

        void EquipAkimWeapon()
        {
            if (flagManager.GetFlag(Flag.VeryLongGunEquipped)) {
                EquipWeapon(akimWeaponConfig);
            }
        }
    }
}
