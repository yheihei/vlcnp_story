using UnityEngine;

namespace VLCNP.Combat
{
    public interface IProjectileLaunchGate
    {
        bool CanLaunch(GameObject projectileOwner);
    }
}
