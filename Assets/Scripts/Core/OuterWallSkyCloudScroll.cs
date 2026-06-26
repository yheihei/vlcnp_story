using UnityEngine;

namespace VLCNP.Core
{
    public sealed class OuterWallSkyCloudScroll : MonoBehaviour
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

        [SerializeField] private CloudLayer[] cloudLayers = new CloudLayer[0];

        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int TilingId = Shader.PropertyToID("_Tiling");
        private static readonly int HorizontalOffsetId = Shader.PropertyToID("_HorizontalOffset");
        private const float MirrorRepeatPeriod = 2f;

        private void Awake()
        {
            CreateRuntimeCloudMaterials();
        }

        private void LateUpdate()
        {
            ScrollClouds();
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

        private void ScrollClouds()
        {
            for (int i = 0; i < cloudLayers.Length; i++)
            {
                CloudLayer layer = cloudLayers[i];
                if (layer == null || layer.runtimeMaterial == null) continue;

                layer.offsetX = Mathf.Repeat(
                    layer.offsetX + layer.speed * Time.deltaTime,
                    MirrorRepeatPeriod);
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
