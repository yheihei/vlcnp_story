using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VLCNP.UI
{
    public class InformationText : MonoBehaviour
    {
        [SerializeField] Text text = null;
        public void DestroyText()
        {
            Destroy(gameObject);
        }

        public void SetValue(string _text)
        {
            text.text = _text;
        }
    }
}
