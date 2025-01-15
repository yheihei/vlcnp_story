using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class AutoSpawnAction : EnemyAction
    {
        [SerializeField]
        GameObject spawnObject = null;

        [SerializeField]
        GameObject spawnEffect;

        [SerializeField]
        Transform spawnPoint = null;

        [SerializeField]
        float spawnMaxRange = 12f;

        // Spawnする感覚のRandomの範囲を2つ指定
        [SerializeField]
        Vector2 spawnIntervalRandomRange = new(8f, 15f);

        // 同時最大Spawn数
        [SerializeField]
        int maxSpawnCount = 1;

        // SpawnしたGameObjectのリスト
        private List<GameObject> spawnedObjects = new List<GameObject>();

        private float nextSpawnTime = Mathf.Infinity;

        void Awake()
        {
            if (spawnObject == null)
                Debug.LogError(
                    $"SpawnObject is not set in the inspector for AutoSpawnAction on {gameObject.name}"
                );
            if (spawnPoint == null)
                Debug.LogError(
                    $"SpawnPoint is not set in the inspector for AutoSpawnAction on {gameObject.name}"
                );
        }

        public override void Execute()
        {
            if (IsExecuting)
                return;
            if (IsDone)
                return;
            IsExecuting = true;
            StartCoroutine(Spawn());
        }

        private IEnumerator Spawn()
        {
            nextSpawnTime = UnityEngine.Random.Range(
                spawnIntervalRandomRange.x,
                spawnIntervalRandomRange.y
            );
            yield return new WaitForSeconds(nextSpawnTime);
            SpawnObject();
            IsDone = true;
        }

        private void SpawnObject()
        {
            if (!CanSpawnRange())
                return;
            if (!CanSpawnCount())
                return;
            if (spawnEffect != null)
            {
                GameObject effect = Instantiate(
                    spawnEffect,
                    spawnPoint.position,
                    Quaternion.identity
                );
                Destroy(effect, 1f);
            }
            GameObject spawn = Instantiate(spawnObject, spawnPoint.position, Quaternion.identity);
            spawnedObjects.Add(spawn);
        }

        private bool CanSpawnCount()
        {
            // 現在のspawnedObjects内をチェックし、nullになっているものを削除
            spawnedObjects.RemoveAll(obj => obj == null);
            if (spawnedObjects.Count < maxSpawnCount)
                return true;
            return false;
        }

        private bool CanSpawnRange()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                return false;
            return Vector2.Distance(player.transform.position, transform.position) < spawnMaxRange;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, spawnMaxRange);
        }
    }
}
