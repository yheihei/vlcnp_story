using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Damage : MonoBehaviour
{
    public void ViewDamage(int _damage)
    {
        GameObject _damageObj = Instantiate(Resources.Load<GameObject>("UI/Damage"));
        _damageObj.GetComponent<TextMesh>().text = _damage.ToString();
        _damageObj.transform.position = transform.position + new Vector3(0, .5f, 0);
    }
}

