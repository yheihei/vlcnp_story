using UnityEngine;

namespace VLCNP.Projectiles
{
    interface IFire
    {
        public void Fire(Vector3 _direction);
        public GameObject GetGameObject();
    }
}
