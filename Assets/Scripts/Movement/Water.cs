using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace VLCNP.Movement
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Water : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isTarget(other))
            {
                using (ListPool<IWaterEventListener>.Get(out List<IWaterEventListener> listeners))
                {
                    other.GetComponentsInChildren(listeners);
                    foreach (IWaterEventListener waterEventListener in listeners)
                    {
                        waterEventListener.OnWaterEnter();
                    }
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (isTarget(other))
            {
                using (ListPool<IWaterEventListener>.Get(out List<IWaterEventListener> listeners))
                {
                    other.GetComponentsInChildren(listeners);
                    foreach (IWaterEventListener waterEventListener in listeners)
                    {
                        waterEventListener.OnWaterExit();
                    }
                }
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (isTarget(other))
            {
                // 水中のキャラクターの数だけ毎物理ステップ呼ばれるため、プールしたリストで受け取る
                using (ListPool<IWaterEventListener>.Get(out List<IWaterEventListener> listeners))
                {
                    other.GetComponentsInChildren(listeners);
                    foreach (IWaterEventListener waterEventListener in listeners)
                    {
                        waterEventListener.OnWaterStay();
                    }
                }
            }
        }

        // tag プロパティは呼ぶたびに文字列を生成するので CompareTag で比べる
        private bool isTarget(Collider2D other)
        {
            return other.CompareTag("Player")
                || other.CompareTag("Enemy")
                || other.CompareTag("Item");
        }
    }
}
