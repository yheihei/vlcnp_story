using UnityEngine;

namespace VLCNP.Pickups
{
    public class DropExperience : MonoBehaviour, IDropItem
    {
        [SerializeField]
        GameObject[] experiences = null;

        public void Drop()
        {
            if (experiences == null)
                return;
            foreach (GameObject experience in experiences)
            {
                Instantiate(experience, transform.position, Quaternion.identity);
            }
        }
    }
}
