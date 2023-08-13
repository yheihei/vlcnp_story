using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VLCNP.UI
{
    public class DamageText : MonoBehaviour
    {
        [SerializeField] Text damageText = null;
        public void DestroyDamageText()
        {
            Destroy(gameObject);
        }

        public void SetValue(float amount)
        {
            damageText.text = amount.ToString();
        }
    }
}
