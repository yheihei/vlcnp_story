using UnityEngine;

namespace VLCNP.Combat
{
    public interface IProjectileOwnerReceiver
    {
        void SetOwner(GameObject owner);
    }
}
