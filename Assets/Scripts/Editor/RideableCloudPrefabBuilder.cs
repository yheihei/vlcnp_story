using UnityEditor;
using UnityEngine;
using VLCNP.Movement;

public static class RideableCloudPrefabBuilder
{
    private const string TexturePath = "Assets/Game/MapObject/PagodaTilemap/Prefabs/rideable_cloud_platform_96x32.png";
    private const string PrefabPath = "Assets/Game/MapObject/PagodaTilemap/Prefabs/rideable_cloud_platform_96x32.prefab";
    private const string VerifySpawnerName = "RideableCloudLiftSpawner_Verify";

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

        var body = cloud.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;

        cloud.AddComponent<RisingCloudLift>();

        PrefabUtility.SaveAsPrefabAsset(cloud, PrefabPath);
        Object.DestroyImmediate(cloud);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/VLCNP/MapObject/Place Rideable Cloud Spawner In Current Scene", false, 3201)]
    public static void PlaceSpawnerInCurrentScene()
    {
        Build();

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            throw new System.InvalidOperationException($"Prefab not loaded: {PrefabPath}");
        }

        var existing = GameObject.Find(VerifySpawnerName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }

        var spawnerObject = new GameObject(VerifySpawnerName);
        spawnerObject.transform.position = GetPlacementPosition();

        var spawner = spawnerObject.AddComponent<RisingCloudLiftSpawner>();
        var serializedSpawner = new SerializedObject(spawner);
        serializedSpawner.FindProperty("cloudLiftPrefab").objectReferenceValue = prefab;
        serializedSpawner.FindProperty("spawnInterval").floatValue = 2.5f;
        serializedSpawner.FindProperty("initialDelay").floatValue = 0f;
        serializedSpawner.FindProperty("spawnOnStart").boolValue = true;
        serializedSpawner.ApplyModifiedPropertiesWithoutUndo();

        Selection.activeGameObject = spawnerObject;
        EditorUtility.SetDirty(spawnerObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(spawnerObject.scene);
    }

    private static Vector3 GetPlacementPosition()
    {
        var party = GameObject.Find("Party");
        if (party != null)
        {
            return party.transform.position + new Vector3(2.8f, -1.2f, 0f);
        }

        var camera = Camera.main;
        if (camera != null)
        {
            return camera.transform.position + new Vector3(0f, -2.5f, 10f);
        }

        return Vector3.zero;
    }
}
