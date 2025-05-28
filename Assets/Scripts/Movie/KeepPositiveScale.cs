using UnityEngine;

namespace VLCNP.Movie
{
    public class KeepPositiveScale : MonoBehaviour
    {
        void Update()
        {
            // 自分のlocalScaleを常に正に保つ
            Vector3 currentScale = transform.localScale;

            if (currentScale.x < 0 || currentScale.y < 0 || currentScale.z < 0)
            {
                transform.localScale = new Vector3(
                    Mathf.Abs(currentScale.x),
                    Mathf.Abs(currentScale.y),
                    Mathf.Abs(currentScale.z)
                );
            }
        }
    }
}
