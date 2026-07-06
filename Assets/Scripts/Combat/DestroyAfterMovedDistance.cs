using UnityEngine;

namespace VLCNP.Combat
{
    /** 生成位置から一定距離進んだら自身を削除する。飛距離制限のある弾に付ける */
    public class DestroyAfterMovedDistance : MonoBehaviour
    {
        [SerializeField]
        float maxDistance = 8f;

        private Vector3 startPosition;

        private void Start()
        {
            startPosition = transform.position;
        }

        private void Update()
        {
            if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
            {
                Destroy(gameObject);
            }
        }
    }
}
