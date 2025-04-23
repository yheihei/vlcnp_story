using System.Collections;
using UnityEngine;

namespace VLCNP.Movie
{
    public class ShakeObject : MonoBehaviour
    {
        public bool IsStopped { get; private set; } = false;

        public void Shake(float magnitude = 0.1f)
        {
            StopAllCoroutines();
            IsStopped = false;
            StartCoroutine(ShakeCoroutine(magnitude));
        }

        private IEnumerator ShakeCoroutine(float magnitude)
        {
            Vector3 originalPosition = transform.position;
            while (!IsStopped)
            {
                transform.position =
                    originalPosition
                    + new Vector3(
                        UnityEngine.Random.Range(-1f, 1f) * magnitude,
                        UnityEngine.Random.Range(-1f, 1f) * magnitude,
                        0
                    );
                yield return null;
            }
            transform.position = originalPosition;
        }

        public void Stop()
        {
            IsStopped = true;
        }
    }
}
