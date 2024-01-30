using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace VLCNP.Movie
{
    public class CMCameraSetFollow : MonoBehaviour
    {
        CinemachineVirtualCamera virtualCamera;

        void Awake()
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }

        public void SetFollow(GameObject target)
        {
            if (virtualCamera == null) return;
            virtualCamera.Follow = target.transform;
        }
    }
}
