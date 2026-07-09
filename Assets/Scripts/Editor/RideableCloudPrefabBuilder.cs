using UnityEditor;
using UnityEngine;

public static class RideableCloudPrefabBuilder
{
    private const string TexturePath = "Assets/Game/MapObject/PagodaTilemap/Prefabs/rideable_cloud_platform_96x32.png";
    private const string PrefabPath = "Assets/Game/MapObject/PagodaTilemap/Prefabs/rideable_cloud_platform_96x32.prefab";

    [MenuItem("Tools/VLCNP/MapObject/Build Rideable Cloud Prefab", false, 3200)]
    public static void Build()
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
        if (importer == null)
        {
            throw new System.InvalidOperationException($"TextureImporter not found: {TexturePath}");
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 32;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.spritePivot = new Vector2(0.5f, 0.5f);
        importer.SaveAndReimport();

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(TexturePath);
        if (sprite == null)
        {
            throw new System.InvalidOperationException($"Sprite not loaded: {TexturePath}");
        }

        var cloud = new GameObject("rideable_cloud_platform_96x32");
        cloud.tag = "Ground";

        var spriteRenderer = cloud.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 100;

        var boxCollider = cloud.AddComponent<BoxCollider2D>();
        boxCollider.offset = new Vector2(0f, 0.22f);
        boxCollider.size = new Vector2(2.875f, 0.28f);

        PrefabUtility.SaveAsPrefabAsset(cloud, PrefabPath);
        Object.DestroyImmediate(cloud);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
