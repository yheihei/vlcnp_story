using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.UI
{
    public class OpenLink : MonoBehaviour
    {
        [SerializeField] string url = "https://www.google.com";
        
        void OnMouseDown()
        {
            OpenLinkInNewTab();
        }

        void OpenLinkInNewTab()
        {
            Application.OpenURL(url);
        }
    }    
}
