using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Core
{
    public class BackGroundLoop : MonoBehaviour
    {
        [SerializeField, Header("視差効果"), Range(0f, 1f)]
        float parallaxEffect = 1f;
        float length;
        float startPosX;
        Transform mainCameraTransform;
        Camera mainCamera;
        bool isCameraMoving = false;

        void Awake()
        {
            startPosX = transform.position.x;
            length = GetComponent<SpriteRenderer>().bounds.size.x;
        }

        void Start()
        {
            mainCamera = Camera.main;
            mainCameraTransform = Camera.main.transform;
        }

        void Update()
        {
            // メインカメラのスピードの絶対値が0より大きい場合、カメラが動いていると判断する
            if (Mathf.Abs(mainCameraTransform.position.x) > 0.01f)
            {
                isCameraMoving = true;
            }
            else
            {
                // メインカメラが動いている状態からストップした場合カクつき防止の為Parallax()を実行しない
                isCameraMoving = false;
            }
        }

        void FixedUpdate()
        {
            if (!isCameraMoving) return;
            Parallax();
        }

        void Parallax()
        {
            float temp = mainCameraTransform.position.x * (1 - parallaxEffect);
            float dist = mainCameraTransform.position.x * parallaxEffect;

            Vector3 targetPosition = new Vector3(startPosX + dist, transform.position.y, transform.position.z);
            transform.position = targetPosition;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);

            if (temp > startPosX + length) startPosX += length;
            else if (temp < startPosX - length) startPosX -= length;
        }
    }

}
