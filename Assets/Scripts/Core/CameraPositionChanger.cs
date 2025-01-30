using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace VLCNP.Core
{
    public class CameraPositionChanger : MonoBehaviour
    {
        [SerializeField]
        CinemachineVirtualCamera virtualCamera;

        [SerializeField]
        private float moveAmount = 4f; // 上下移動量

        [SerializeField]
        private float moveSpeed = 1f; // 移動速度

        CameraYPosition cameraPosition = CameraYPosition.Middle;
        Vector3 startOffset;

        CinemachineFramingTransposer transposer;

        void Start()
        {
            transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            startOffset = transposer.m_TrackedObjectOffset;
        }

        public void ChangeCameraPosition(CameraYPosition _cameraPosition)
        {
            cameraPosition = _cameraPosition;
            StartCoroutine(ChangeCameraPositionCoroutine());
        }

        private IEnumerator ChangeCameraPositionCoroutine()
        {
            float targetY = 0;
            switch (cameraPosition)
            {
                case CameraYPosition.Upper:
                    targetY = moveAmount;
                    break;
                case CameraYPosition.Middle:
                    targetY = 0;
                    break;
                case CameraYPosition.Lower:
                    targetY = -moveAmount;
                    break;
            }

            targetY += startOffset.y;
            Vector3 targetOffset = new Vector3(startOffset.x, targetY, startOffset.z);
            transposer.m_TrackedObjectOffset = targetOffset; // 最終位置を設定
            yield return null;
        }
    }
}
