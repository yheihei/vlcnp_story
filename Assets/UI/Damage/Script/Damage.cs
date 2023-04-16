using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Damage : MonoBehaviour
{
    [SerializeField]
    private GameObject ParentObj;
    [SerializeField]
    private GameObject DamageObj;
    [SerializeField]
    private GameObject PosObj;
    [SerializeField]
    private Vector3 AdjPos;

    public void ViewDamage(int _damage)
    {
        if (ParentObj == null)
        {
            ParentObj = GameObject.Find("Canvas");
        }
        GameObject _damageObj = Instantiate(DamageObj, ParentObj.transform);
        _damageObj.GetComponent<Text>().text = _damage.ToString();
        SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();
        _damageObj.transform.position = playerSprite.transform.position;
    }
}

