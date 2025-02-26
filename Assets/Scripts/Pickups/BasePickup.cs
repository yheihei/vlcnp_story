using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Movement;

namespace VLCNP.Pickups
{
    public class BasePickup : MonoBehaviour, IWaterEventListener
    {
        [SerializeField]
        string targetTagName = "Player";
        bool isPickedUp = false;

        Rigidbody2D rbody;
        float defaultGravityScale = 0;

        private void Awake()
        {
            rbody = GetComponent<Rigidbody2D>();
            if (rbody != null)
                defaultGravityScale = rbody.gravityScale;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.tag != "Player")
                return;
            if (isPickedUp)
                return;
            isPickedUp = true;
            Pickup(other.gameObject);
            Destroy(gameObject);
        }

        public virtual void Pickup(GameObject gameObject)
        {
            throw new System.NotImplementedException("子クラスで実装を定義してください");
        }

        public void OnWaterEnter()
        {
            if (rbody == null)
                return;
            rbody.velocity *= 0.5f;
        }

        public void OnWaterExit()
        {
            if (rbody == null)
                return;
            rbody.gravityScale = defaultGravityScale;
        }

        public void OnWaterStay()
        {
            if (rbody == null)
                return;
            rbody.gravityScale = 2f / 9f;
        }
    }
}
