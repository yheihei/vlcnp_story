using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Control;
using VLCNP.Stats;

namespace VLCNP.DebugSctipt
{
    public class DebugHealthLevel : MonoBehaviour
    {
        [SerializeField]
        private int healthLevel = 0;
        PartyCongroller partyCongroller;
        PartyHealthLevel partyHealthLevel;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        void Awake()
        {
            partyCongroller = GameObject
                .FindGameObjectWithTag("Party")
                .GetComponent<PartyCongroller>();
            partyHealthLevel = GameObject
                .FindGameObjectWithTag("Party")
                .GetComponent<PartyHealthLevel>();
        }

        void Start()
        {
            if (partyHealthLevel == null)
            {
                Debug.Log("PartyHealthLevel component not found on Party GameObject.");
                return;
            }

            while (partyHealthLevel.GetCurrentLevel() < healthLevel)
            {
                partyCongroller.IncrementHealthLevel();
            }
        }
#endif
    }
}
