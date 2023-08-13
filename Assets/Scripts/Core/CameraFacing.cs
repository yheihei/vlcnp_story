using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Core
{
    public class CameraFacing : MonoBehaviour
    {
        private void LateUpdate()
        {
            // 親が向いている向き
            Vector3 worldDirectedLocalScale
                = transform.parent.TransformDirection(transform.localScale);
            print(worldDirectedLocalScale.x);
            // 反転
            transform.localScale.Set(
                worldDirectedLocalScale.x * -1,
                worldDirectedLocalScale.y,
                worldDirectedLocalScale.z
            );

        }
    }
}
