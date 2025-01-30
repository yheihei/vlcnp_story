using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Core;

namespace VLCNP.Movie
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class CameraChangeArea : MonoBehaviour
    {
        [SerializeField]
        private CameraPositionChanger cameraPositionChanger;

        [SerializeField]
        private CameraYPosition cameraModeOnEnter;

        [SerializeField]
        private CameraYPosition cameraModeOnExit;

        void Awake()
        {
            if (cameraPositionChanger == null)
            {
                // Tagで検索して取得
                GameObject cameraPositionChangerObject = GameObject.FindWithTag(
                    "CameraPositionChanger"
                );
                if (cameraPositionChangerObject == null)
                    throw new System.Exception("CameraPositionChanger is not set.");
                cameraPositionChanger =
                    cameraPositionChangerObject.GetComponent<CameraPositionChanger>();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.CompareTag("Player"))
                return;
            cameraPositionChanger.ChangeCameraPosition(cameraModeOnEnter);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.CompareTag("Player"))
                return;
            cameraPositionChanger.ChangeCameraPosition(cameraModeOnExit);
        }
    }
}
