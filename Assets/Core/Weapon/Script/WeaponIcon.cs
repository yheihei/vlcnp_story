using UnityEngine;
using UnityEngine.UI;

public class WeaponIcon : MonoBehaviour
{
    private Image iconImage;
    private Equipment equipment;
    private string currentEquipmentName = "";
    private Sprite veryLongGunIcon;
    private Sprite boomerangIcon;

    void Start()
    {
        iconImage = GetComponent<Image>();
        veryLongGunIcon = Resources.Load<Sprite>("Weapon/verylonggun_icon");
        boomerangIcon = Resources.Load<Sprite>("Weapon/bumeran_icon");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            enabled = false;
            return;
        }

        equipment = player.GetComponent<Equipment>();
        if (equipment == null)
        {
            enabled = false;
        }
    }

    private void Update()
    {
        GameObject currentWeapon = equipment.GetCurrentWeapon();
        if (currentWeapon == null || currentEquipmentName == currentWeapon.name)
        {
            return;
        }

        currentEquipmentName = currentWeapon.name;
        switch (currentEquipmentName)
        {
            case "verylonggun":
                iconImage.sprite = veryLongGunIcon;
                break;
            case "boomerang_weapon":
                iconImage.sprite = boomerangIcon;
                break;
            default:
                break;
        }
    }
}
