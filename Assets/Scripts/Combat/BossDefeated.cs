using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Attributes;
using VLCNP.Core;

namespace VLCNP.Combat
{
    /**
     * ボスを倒したときにフラグを立てる
     */
    public class BossDefeated : MonoBehaviour
    {
        Health health;

        [SerializeField]
        Flag defeatedFlag;

        FlagManager flagManager;

        void Awake()
        {
            health = GetComponent<Health>();
            health.onDie += Die;
            flagManager = FindObjectOfType<FlagManager>();
        }

        void Die()
        {
            flagManager.SetFlag(defeatedFlag, true);
        }
    }    
}
