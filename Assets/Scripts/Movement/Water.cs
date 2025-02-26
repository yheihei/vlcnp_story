using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Movement
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Water : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isTarget(other))
            {
                IWaterEventListener[] waterEventListeners =
                    other.GetComponentsInChildren<IWaterEventListener>();
                foreach (IWaterEventListener waterEventListener in waterEventListeners)
                {
                    waterEventListener.OnWaterEnter();
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (isTarget(other))
            {
                IWaterEventListener[] waterEventListeners =
                    other.GetComponentsInChildren<IWaterEventListener>();
                foreach (IWaterEventListener waterEventListener in waterEventListeners)
                {
                    waterEventListener.OnWaterExit();
                }
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (isTarget(other))
            {
                IWaterEventListener[] waterEventListeners =
                    other.GetComponentsInChildren<IWaterEventListener>();
                foreach (IWaterEventListener waterEventListener in waterEventListeners)
                {
                    waterEventListener.OnWaterStay();
                }
            }
        }

        private bool isTarget(Collider2D other)
        {
            return other.gameObject.tag == "Player"
                || other.gameObject.tag == "Enemy"
                || other.gameObject.tag == "Item";
        }
    }
}
