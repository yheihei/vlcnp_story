using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.UI
{
    public class DamageTextSpawner : MonoBehaviour
    {
        [SerializeField] DamageText damageTextPrefab = null;

        public void Spawn(float damageAmount)
        {
            DamageText instance = Instantiate<DamageText>(
                damageTextPrefab,
                transform.position,
                transform.rotation
            );
            instance.SetValue(damageAmount);
        }
    }
}
