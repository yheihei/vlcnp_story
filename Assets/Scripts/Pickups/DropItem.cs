using UnityEngine;

namespace VLCNP.Pickups
{
    public class DropItem : MonoBehaviour, IDropItem
    {
        [SerializeField]
        GameObject dropItem = null;

        [SerializeField]
        float dropRate = 0.2f;

        public void Drop() { }
    }
}
