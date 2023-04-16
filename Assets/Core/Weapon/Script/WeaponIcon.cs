using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponIcon : MonoBehaviour
{
    private Image iconImage;
    private GameObject player;
    private Equipment equipment;
    private string currentEquipmentName = "";

    void Start()
    {
        iconImage = GetComponent<Image>();
        player = GameObject.FindGameObjectWithTag("Player");
        equipment = player.GetComponent<Equipment>();
    }

    private void FixedUpdate()
    {
        if (currentEquipmentName == equipment.GetCurrentWeapon().name)
        {
            return;
        }
        currentEquipmentName = equipment.GetCurrentWeapon().name;
        switch (currentEquipmentName)
        {
            case "verylonggun":
                iconImage.sprite = Resources.Load<Sprite>("Weapon/verylonggun_icon");
                break;
            case "boomerang_weapon":
                iconImage.sprite = Resources.Load<Sprite>("Weapon/bumeran_icon");
                break;
            default:
                break;
        }
    }
}
