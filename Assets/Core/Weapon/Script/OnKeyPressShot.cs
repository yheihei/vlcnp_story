using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnKeyPressShot : MonoBehaviour
{
    public GameObject BeamLevel1;
    public GameObject BeamLevel2;
    public GameObject BeamLevel3;
    private bool isShot = false;
    private bool isReload = false;
    private SpriteRenderer sprite;
    private GameObject equipedCharacter;
    private int reloadCount = 0;
    public int ReloadTime = 25;
    public int ShotLimit = 3;
    private ILevel playerLevel;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        equipedCharacter = transform.root.gameObject;
        playerLevel = equipedCharacter.GetComponent<ILevel>();
    }

    void Update()
    {
        if (Input.GetKeyUp("x") && !isReload)
        {
            isShot = true;
        }
    }

    private void FixedUpdate()
    {
        if (isShot && !isReload) {
            GameObject beamObject = BeamLevel1;
            if (playerLevel.Level == 2) {
                beamObject = BeamLevel2;
            }
            if (playerLevel.Level >= 3)
            {
                beamObject = BeamLevel3;
            }
            GameObject beam = Instantiate(beamObject);
            Vector3 position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            beam.transform.position = position;
            beam.GetComponent<IWeapon>().IsLeft = equipedCharacter.transform.localScale.x > 0;
            isReload = true;
            isShot = false;
        }

        if (isReload) {
            reloadCount += 1;
            if (reloadCount > ReloadTime) {
                reloadCount = 0;
                isReload = false;
            }
        }
    }
}
