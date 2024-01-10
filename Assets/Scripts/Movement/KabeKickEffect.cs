using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Movement
{
    public class KabeKickEffect : MonoBehaviour
    {
        void Start()
        {
            // 1s後に消滅
            Destroy(gameObject, 1f);
        }
    }    
}
