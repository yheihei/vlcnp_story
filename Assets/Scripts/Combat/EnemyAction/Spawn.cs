using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class Spawn : MonoBehaviour, ISpawn
    {
        [SerializeField]
        GameObject enemyPrefab;

        public GameObject Execute()
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("Enemy prefab is not set.");
                return null;
            }
            return Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        }
    }
}
