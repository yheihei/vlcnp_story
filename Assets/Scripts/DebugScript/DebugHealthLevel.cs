using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Control;

namespace VLCNP.DebugSctipt
{
    public class DebugHealthLevel : MonoBehaviour
    {
        [SerializeField]
        private int healthLevel = 0;
        PartyCongroller partyCongroller;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        void Awake()
        {
            partyCongroller = GameObject
                .FindGameObjectWithTag("Party")
                .GetComponent<PartyCongroller>();
        }

        void Start()
        {
            // HealthLevelの数だけパーティーのHealthLevelをincrementさせる
            for (int i = 0; i < healthLevel; i++)
            {
                partyCongroller.IncrementHealthLevel();
            }
        }
#endif
    }
}
