using System.Collections;
using UnityEngine;

namespace VLCNP.Movement
{
    /**
     * 指定間隔で上昇雲リフトを生成する。
     */
    public class RisingCloudLiftSpawner : MonoBehaviour
    {
        [SerializeField] GameObject cloudLiftPrefab;
        [SerializeField] float spawnInterval = 2.5f;
        [SerializeField] float initialDelay = 0f;
        [SerializeField] bool spawnOnStart = true;
        [Header("生成する雲リフト設定")]
        [SerializeField] RisingCloudLift.Settings liftSettings = new RisingCloudLift.Settings();

        Coroutine spawnRoutine;

        private void OnEnable()
        {
            if (spawnRoutine == null)
            {
                spawnRoutine = StartCoroutine(SpawnLoop());
            }
        }

        private void OnDisable()
        {
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }
        }

        private IEnumerator SpawnLoop()
        {
            if (initialDelay > 0f)
            {
                yield return new WaitForSeconds(initialDelay);
            }

            if (spawnOnStart)
            {
                Spawn();
            }

            var wait = new WaitForSeconds(Mathf.Max(0.1f, spawnInterval));
            while (true)
            {
                yield return wait;
                Spawn();
            }
        }

        public GameObject Spawn()
        {
            if (cloudLiftPrefab == null)
            {
                Debug.LogWarning($"[RisingCloudLiftSpawner] cloudLiftPrefab is not set on {name}.", this);
                return null;
            }

            var cloudLift = Instantiate(cloudLiftPrefab, transform.position, transform.rotation);
            var lift = cloudLift.GetComponent<RisingCloudLift>();
            if (lift == null)
            {
                Debug.LogWarning($"[RisingCloudLiftSpawner] RisingCloudLift is not attached to {cloudLiftPrefab.name}.", this);
                return cloudLift;
            }

            lift.ApplySettings(liftSettings);
            return cloudLift;
        }
    }
}
