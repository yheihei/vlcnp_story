using UnityEngine;
using System.Collections;

namespace VLCNP.UI
{
    public class UIObjectBlinker : MonoBehaviour
    {
        // 消えている秒数
        public float blinkInterval = 0.5f;
        // 表示秒数
        public float visibleInterval = 1f;
        public bool startVisible = true; // 開始時の表示状態

        private Behaviour behaviour;

        void Start()
        {
            behaviour = GetComponent<Behaviour>();
            behaviour.enabled = startVisible;
            StartCoroutine(BlinkCoroutine());
        }

        IEnumerator BlinkCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(behaviour.enabled ? visibleInterval : blinkInterval);
                behaviour.enabled = !behaviour.enabled;
            }
        }
    }
}
