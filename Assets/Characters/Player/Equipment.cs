using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    public List<GameObject> Weapons;
    private int currentIndex = 0;
    private bool isNext = false;

    void Start()
    {
        GameObject currentWeapon = Weapons[0];
        Instantiate(currentWeapon, transform);
    }

    void Update()
    {
        if (Input.GetKeyUp("a") || Input.GetKeyUp("s"))
        {
            isNext = true; 
        }
    }

    private void FixedUpdate()
    {
        if (isNext)
        {
            isNext = false;
            Destroy(transform.GetChild(0).gameObject);
            currentIndex = currentIndex + 1 < Weapons.Count ? currentIndex + 1 : 0;
            Instantiate(Weapons[currentIndex], transform);
        }
    }
}
