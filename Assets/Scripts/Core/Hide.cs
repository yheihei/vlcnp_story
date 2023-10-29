using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace VLCNP.Core
{
    public class Hide : MonoBehaviour
    {
        Flag flag;
        FlagManager flagManager;

        private void Start()
        {
            flagManager = FindObjectOfType<FlagManager>();
        }

        private void FixedUpdate()
        {
            if (flagManager.GetFlag(flag))
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }
    }    
}
