using UnityEngine;

namespace VLCNP.Core
{
    [DefaultExecutionOrder(1000)]
    public class BackGroundLoop : MonoBehaviour
    {
        [SerializeField, Header("視差効果"), Range(0f, 1f)]
        private float parallaxEffect = 1f;

        private float length;
        private float startPosX;
        private SpriteRenderer spriteRenderer;
        private Transform mainCameraTransform;
        private bool missingSpriteRendererLogged;
        private bool missingCameraLogged;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            startPosX = transform.position.x;
            RefreshLength();
        }

        private void Start()
        {
            CacheMainCamera();
        }

        private void LateUpdate()
        {
            if (!EnsureDependencies()) return;
            UpdateParallax();
        }

        private bool EnsureDependencies()
        {
            if (spriteRenderer == null)
            {
                if (!missingSpriteRendererLogged)
                {
                    Debug.LogWarning($"[BackGroundLoop] SpriteRenderer not found on {name}.", this);
                    missingSpriteRendererLogged = true;
                }
                return false;
            }

            if (mainCameraTransform == null && !CacheMainCamera())
            {
                return false;
            }

            if (length <= Mathf.Epsilon)
            {
                RefreshLength();
                if (length <= Mathf.Epsilon)
                {
                    return false;
                }
            }

            return true;
        }

        private bool CacheMainCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                if (!missingCameraLogged)
                {
                    Debug.LogWarning($"[BackGroundLoop] Main Camera not found for {name}.", this);
                    missingCameraLogged = true;
                }
                return false;
            }

            mainCameraTransform = mainCamera.transform;
            missingCameraLogged = false;
            return true;
        }

        private void RefreshLength()
        {
            if (spriteRenderer == null)
            {
                length = 0f;
                return;
            }

            length = spriteRenderer.bounds.size.x;
        }

        private void UpdateParallax()
        {
            float cameraX = mainCameraTransform.position.x;
            float temp = cameraX * (1f - parallaxEffect);
            float dist = cameraX * parallaxEffect;

            while (temp > startPosX + length)
            {
                startPosX += length;
            }

            while (temp < startPosX - length)
            {
                startPosX -= length;
            }

            Vector3 currentPosition = transform.position;
            transform.position = new Vector3(startPosX + dist, currentPosition.y, currentPosition.z);
        }
    }
}
