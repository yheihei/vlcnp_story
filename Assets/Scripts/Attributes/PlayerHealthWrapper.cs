using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Control;

namespace VLCNP.Attributes
{
    public class PlayerHealthWrapper : MonoBehaviour
    {
        public void RestoreHealth()
        {
            PartyCongroller partyCongroller = GameObject.FindGameObjectWithTag("Party")?.GetComponent<PartyCongroller>();
            if (partyCongroller == null) return;
            partyCongroller.AllMemberRestoreHealth();
        }
    }
}
