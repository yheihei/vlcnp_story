using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Pickups
{
    public class BasePickup : MonoBehaviour
    {
        [SerializeField] string targetTagName = "Player";
        bool isPickedUp = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.tag != "Player") return;
            if (isPickedUp) return;
            isPickedUp = true;
            Pickup(other.gameObject);
            Destroy(gameObject);
        }

        public virtual void Pickup(GameObject gameObject)
        {
            throw new System.NotImplementedException("子クラスで実装を定義してください");
        }
    }    
}
