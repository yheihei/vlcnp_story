using UnityEngine;

namespace VLCNP.Pickups
{
    public class DropItem : MonoBehaviour, IDropItem
    {
        [SerializeField]
        BasePickup dropItem = null;

        [SerializeField]
        float dropRate = 0.2f;

        public void Drop()
        {
            if (dropItem == null)
                return;
            if (Random.value > dropRate)
                return;
            Instantiate(dropItem, transform.position, Quaternion.identity);
        }
    }
}
