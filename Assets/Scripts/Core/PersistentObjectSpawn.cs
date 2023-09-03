using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Core
{
    public class PersistentObjectSpawn : MonoBehaviour
    {
        [SerializeField] GameObject persistentObjectPrefab;

        static bool hasSpawned = false;

        private void Awake() {
            if (hasSpawned) return;
            SpawnPersistentObjects();
            hasSpawned = true;
        }

        private void SpawnPersistentObjects()
        {
            GameObject persistentObject = Instantiate(persistentObjectPrefab);
            DontDestroyOnLoad(persistentObject);
        }
    }
}
