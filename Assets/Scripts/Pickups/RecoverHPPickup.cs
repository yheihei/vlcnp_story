using UnityEngine;
using VLCNP.Attributes;
using VLCNP.Stats;

namespace VLCNP.Pickups
{
    [RequireComponent(typeof(AudioSource))]
    public class RecoverHPPickup : BasePickup
    {
        [SerializeField]
        float recoverAmount = 1f;
        AudioSource audioSource;

        [SerializeField]
        AudioClip getSeSound = null;

        [SerializeField]
        float seVolume = 0.3f;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public override void Pickup(GameObject finderGameObject)
        {
            Health health = finderGameObject.GetComponent<Health>();
            if (health == null)
                return;
            health.RestoreHealthBy(recoverAmount, getSeSound, seVolume);
        }
    }
}
