using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Core
{
    /**
     * 複数のスポーン地点から一定間隔で敵を出現させる。
     * 同時存在数の上限は全地点の合計で制限する。
     */
    public class MultiPointAutoSpawn : MonoBehaviour, IStoppable
    {
        [SerializeField]
        GameObject spawnObject;

        [SerializeField]
        Transform[] spawnPoints;

        [SerializeField]
        float intervalSecond = 5f;

        [SerializeField]
        GameObject spawnEffect;

        // 全スポーン地点合計の同時存在数上限
        [SerializeField]
        int maxSpawnCount = 3;

        private float timeSinceLastSpawn = 0f;
        private int nextSpawnPointIndex = 0;
        private readonly List<GameObject> spawnedObjects = new List<GameObject>();

        bool isStopped = false;
        public bool IsStopped
        {
            get => isStopped;
            set => isStopped = value;
        }

        void FixedUpdate()
        {
            if (isStopped)
                return;
            if (spawnObject == null || spawnPoints == null || spawnPoints.Length == 0)
                return;
            timeSinceLastSpawn += Time.deltaTime;
            if (timeSinceLastSpawn <= intervalSecond)
                return;
            timeSinceLastSpawn = 0f;
            if (!CanSpawnCount())
                return;
            Spawn();
        }

        private void Spawn()
        {
            Transform spawnPoint = spawnPoints[nextSpawnPointIndex % spawnPoints.Length];
            nextSpawnPointIndex = (nextSpawnPointIndex + 1) % spawnPoints.Length;
            if (spawnPoint == null)
                return;
            GameObject spawn = Instantiate(
                spawnObject,
                spawnPoint.position,
                Quaternion.identity
            );
            spawnedObjects.Add(spawn);
            if (spawnEffect != null)
            {
                GameObject effect = Instantiate(
                    spawnEffect,
                    spawnPoint.position,
                    Quaternion.identity
                );
                Destroy(effect, 1f);
            }
        }

        private bool CanSpawnCount()
        {
            spawnedObjects.RemoveAll(obj => obj == null);
            return spawnedObjects.Count < maxSpawnCount;
        }
    }
}
