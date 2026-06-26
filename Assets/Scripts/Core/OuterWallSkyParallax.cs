using UnityEngine;

namespace VLCNP.Core
{
    public sealed class OuterWallSkyParallax : MonoBehaviour
    {
        [System.Serializable]
        private sealed class CloudLayer
        {
            public Renderer renderer;
            public float speed = 0.03f;
            public float tilingX = 1f;
            public float tilingY = 1f;

            [System.NonSerialized] public Material runtimeMaterial;
            [System.NonSerialized] public float offsetX;
        }

        [SerializeField] private Camera targetCamera;
        [SerializeField] private Renderer[] fullScreenRenderers = new Renderer[0];
        [SerializeField] private Transform moonTransform;
        [SerializeField] private Vector2 screenPadding = new Vector2(4f, 3f);
        [SerializeField] private Vector2 moonViewportPosition = new Vector2(0.76f, 0.68f);
        [SerializeField] private float moonScreenHeightRatio = 0.26f;
        [SerializeField] private CloudLayer[] cloudLayers = new CloudLayer[0];

        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int TilingId = Shader.PropertyToID("_Tiling");
        private static readonly int HorizontalOffsetId = Shader.PropertyToID("_HorizontalOffset");

        private void Awake()
        {
            CacheCamera();
            CreateRuntimeCloudMaterials();
            FitToCamera();
        }

        private void LateUpdate()
        {
            if (targetCamera == null && !CacheCamera()) return;

            FitToCamera();
            ScrollClouds();
        }

        private bool CacheCamera()
        {
            targetCamera = Camera.main;
            return targetCamera != null;
        }

        private void CreateRuntimeCloudMaterials()
        {
            for (int i = 0; i < cloudLayers.Length; i++)
            {
                CloudLayer layer = cloudLayers[i];
                if (layer == null || layer.renderer == null || layer.runtimeMaterial != null) continue;

                Material source = layer.renderer.sharedMaterial;
                if (source == null) continue;

                layer.runtimeMaterial = new Material(source);
                layer.renderer.sharedMaterial = layer.runtimeMaterial;
            }
        }

        private void FitToCamera()
        {
            if (targetCamera == null || !targetCamera.orthographic) return;

            float height = targetCamera.orthographicSize * 2f + screenPadding.y;
            float width = targetCamera.orthographicSize * 2f * targetCamera.aspect + screenPadding.x;

            Vector3 cameraPosition = targetCamera.transform.position;
            transform.position = new Vector3(cameraPosition.x, cameraPosition.y, transform.position.z);

            for (int i = 0; i < fullScreenRenderers.Length; i++)
            {
                Renderer renderer = fullScreenRenderers[i];
                if (renderer == null) continue;

                renderer.transform.localPosition = Vector3.zero;
                FitRenderer(renderer, width, height);
            }

            if (moonTransform != null)
            {
                float localX = (moonViewportPosition.x - 0.5f) * width;
                float localY = (moonViewportPosition.y - 0.5f) * height;
                float moonSize = height * moonScreenHeightRatio;
                moonTransform.localPosition = new Vector3(localX, localY, moonTransform.localPosition.z);
                FitTransformToSquare(moonTransform, moonSize);
            }
        }

        private static void FitTransformToSquare(Transform target, float size)
        {
            SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                target.localScale = new Vector3(size, size, 1f);
                return;
            }

            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            if (spriteSize.x <= Mathf.Epsilon || spriteSize.y <= Mathf.Epsilon)
            {
                target.localScale = new Vector3(size, size, 1f);
                return;
            }

            target.localScale = new Vector3(size / spriteSize.x, size / spriteSize.y, 1f);
        }

        private static void FitRenderer(Renderer renderer, float width, float height)
        {
            if (renderer is SpriteRenderer spriteRenderer && spriteRenderer.sprite != null)
            {
                if (spriteRenderer.drawMode == SpriteDrawMode.Tiled || spriteRenderer.drawMode == SpriteDrawMode.Sliced)
                {
                    spriteRenderer.size = new Vector2(width, height);
                    spriteRenderer.transform.localScale = Vector3.one;
                    return;
                }

                Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
                if (spriteSize.x > Mathf.Epsilon && spriteSize.y > Mathf.Epsilon)
                {
                    spriteRenderer.transform.localScale = new Vector3(width / spriteSize.x, height / spriteSize.y, 1f);
                    return;
                }
            }

            renderer.transform.localScale = new Vector3(width, height, 1f);
        }

        private void ScrollClouds()
        {
            for (int i = 0; i < cloudLayers.Length; i++)
            {
                CloudLayer layer = cloudLayers[i];
                if (layer == null || layer.runtimeMaterial == null) continue;

                layer.offsetX = Mathf.Repeat(layer.offsetX + layer.speed * Time.deltaTime, 1f);
                Vector2 offset = new Vector2(layer.offsetX, 0f);
                Vector2 scale = new Vector2(layer.tilingX, layer.tilingY);

                if (layer.runtimeMaterial.HasProperty(BaseMapId))
                {
                    layer.runtimeMaterial.SetTextureOffset(BaseMapId, offset);
                    layer.runtimeMaterial.SetTextureScale(BaseMapId, scale);
                }

                if (layer.runtimeMaterial.HasProperty(MainTexId))
                {
                    layer.runtimeMaterial.SetTextureOffset(MainTexId, offset);
                    layer.runtimeMaterial.SetTextureScale(MainTexId, scale);
                }

                if (layer.runtimeMaterial.HasProperty(HorizontalOffsetId))
                {
                    layer.runtimeMaterial.SetFloat(HorizontalOffsetId, layer.offsetX);
                }

                if (layer.runtimeMaterial.HasProperty(TilingId))
                {
                    layer.runtimeMaterial.SetVector(TilingId, new Vector4(layer.tilingX, layer.tilingY, 0f, 0f));
                }
            }
        }
    }
}
