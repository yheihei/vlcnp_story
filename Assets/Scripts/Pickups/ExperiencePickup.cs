using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Stats;

namespace VLCNP.Pickups
{
    public class ExperiencePickup : BasePickup
    {
        [SerializeField] float experienceAmount = 0f;

        public override void Pickup(GameObject finderGameObject)
        {
            Experience experience = finderGameObject.GetComponent<Experience>();
            if (experience == null) return;
            experience.GainExperience(experienceAmount);
            Destroy(gameObject);
        }
    }

}