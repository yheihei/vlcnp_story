using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VLCNP.Core;
using VLCNP.Pickups;

/**
 * Kaze3_boss_2 の壊れた丸窓と、そこから出現する VeryShortSwallow(ドロップ強化版)のセットアップ。
 * 1. ドロップ強化版 VeryShortSwallow バリアントの作成
 * 2. 壊れた丸窓プレハブの作成(PNG 配置後に実行)
 * 3. シーン内の窓 _4 / _7 を壊れた窓に差し替え、スポナーを設置
 */
public static class BrokenWindowSwallowBuilder
{
    private const string SwallowPrefabPath =
        "Assets/Game/Characters/Enemy/VeryShortSwallow.prefab";
    private const string SwallowVariantPath =
        "Assets/Game/Characters/Enemy/VeryShortSwallowForBossVariant.prefab";
    private const string HartRecoverPath = "Assets/Game/Pickups/HartRecover.prefab";
    private const string RainbowSeedMediumPath = "Assets/Game/Pickups/RainbowSeedMedium.prefab";
    private const string RainbowSeedSmallPath = "Assets/Game/Pickups/RainbowSeedSmall.prefab";
    private const string WindowPrefabPath =
        "Assets/Game/MapObject/PagodaTilemap/Prefabs/WallOrnaments/round_window_cross_01_32x32.prefab";
    private const string BrokenWindowPngPath =
        "Assets/Game/MapObject/PagodaTilemap/Prefabs/WallOrnaments/round_window_cross_01_broken_32x32.png";
    private const string BrokenWindowPrefabPath =
        "Assets/Game/MapObject/PagodaTilemap/Prefabs/WallOrnaments/round_window_cross_01_broken_32x32.prefab";
    private const string SpawnEffectPath = "Assets/Game/Effect/bombnose.prefab";
    private const string SpawnerName = "BrokenWindowSwallowSpawner";

    private static readonly string[] WindowSuffixes = { "4", "7" };

    [MenuItem("Tools/Kaze3Boss2/1. Create Swallow Drop Variant")]
    public static void CreateSwallowDropVariant()
    {
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(SwallowPrefabPath);
        if (source == null)
            throw new System.InvalidOperationException($"Load failed: {SwallowPrefabPath}");
        GameObject seedMedium = AssetDatabase.LoadAssetAtPath<GameObject>(RainbowSeedMediumPath);
        GameObject seedSmall = AssetDatabase.LoadAssetAtPath<GameObject>(RainbowSeedSmallPath);
        BasePickup hartRecover = AssetDatabase
            .LoadAssetAtPath<GameObject>(HartRecoverPath)
            ?.GetComponent<BasePickup>();
        if (seedMedium == null || seedSmall == null || hartRecover == null)
            throw new System.InvalidOperationException("Pickup prefab load failed");

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
        try
        {
            instance.name = "VeryShortSwallowForBossVariant";

            DropExperience dropExperience = instance.GetComponent<DropExperience>();
            SerializedObject serializedDropExperience = new SerializedObject(dropExperience);
            SerializedProperty experiences = serializedDropExperience.FindProperty("experiences");
            experiences.arraySize = 2;
            experiences.GetArrayElementAtIndex(0).objectReferenceValue = seedMedium;
            experiences.GetArrayElementAtIndex(1).objectReferenceValue = seedSmall;
            serializedDropExperience.ApplyModifiedPropertiesWithoutUndo();

            DropItem dropItem = instance.GetComponent<DropItem>();
            SerializedObject serializedDropItem = new SerializedObject(dropItem);
            serializedDropItem.FindProperty("dropItem").objectReferenceValue = hartRecover;
            serializedDropItem.FindProperty("dropRate").floatValue = 0.8f;
            serializedDropItem.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(instance, SwallowVariantPath);
        }
        finally
        {
            Object.DestroyImmediate(instance);
        }
        Debug.Log($"Saved: {SwallowVariantPath}");
    }

    [MenuItem("Tools/Kaze3Boss2/2. Create Broken Window Prefab")]
    public static void CreateBrokenWindowPrefab()
    {
        Sprite brokenSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BrokenWindowPngPath);
        if (brokenSprite == null)
            throw new System.InvalidOperationException(
                $"Broken window sprite load failed: {BrokenWindowPngPath}"
            );
        GameObject root = PrefabUtility.LoadPrefabContents(WindowPrefabPath);
        try
        {
            root.name = "round_window_cross_01_broken_32x32";
            root.GetComponent<SpriteRenderer>().sprite = brokenSprite;
            PrefabUtility.SaveAsPrefabAsset(root, BrokenWindowPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
        Debug.Log($"Saved: {BrokenWindowPrefabPath}");
    }

    [MenuItem("Tools/Kaze3Boss2/3. Setup Broken Windows And Spawner In Scene")]
    public static void SetupBrokenWindowsAndSpawner()
    {
        GameObject brokenWindowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            BrokenWindowPrefabPath
        );
        if (brokenWindowPrefab == null)
            throw new System.InvalidOperationException(
                $"Broken window prefab load failed: {BrokenWindowPrefabPath}"
            );
        GameObject swallowVariant = AssetDatabase.LoadAssetAtPath<GameObject>(SwallowVariantPath);
        if (swallowVariant == null)
            throw new System.InvalidOperationException(
                $"Swallow variant load failed: {SwallowVariantPath}"
            );
        GameObject spawnEffect = AssetDatabase.LoadAssetAtPath<GameObject>(SpawnEffectPath);
        if (spawnEffect == null)
            throw new System.InvalidOperationException(
                $"Spawn effect load failed: {SpawnEffectPath}"
            );

        List<Transform> spawnPoints = new List<Transform>();
        foreach (string suffix in WindowSuffixes)
        {
            spawnPoints.Add(ReplaceWindow(brokenWindowPrefab, suffix));
        }

        GameObject spawner = GameObject.Find(SpawnerName);
        if (spawner == null)
            spawner = new GameObject(SpawnerName);
        MultiPointAutoSpawn autoSpawn = spawner.GetComponent<MultiPointAutoSpawn>();
        if (autoSpawn == null)
            autoSpawn = spawner.AddComponent<MultiPointAutoSpawn>();
        SerializedObject serializedSpawn = new SerializedObject(autoSpawn);
        serializedSpawn.FindProperty("spawnObject").objectReferenceValue = swallowVariant;
        SerializedProperty points = serializedSpawn.FindProperty("spawnPoints");
        points.arraySize = spawnPoints.Count;
        for (int i = 0; i < spawnPoints.Count; i++)
            points.GetArrayElementAtIndex(i).objectReferenceValue = spawnPoints[i];
        serializedSpawn.FindProperty("intervalSecond").floatValue = 5f;
        serializedSpawn.FindProperty("spawnEffect").objectReferenceValue = spawnEffect;
        serializedSpawn.FindProperty("maxSpawnCount").intValue = 3;
        serializedSpawn.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(spawner.scene);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Broken windows and spawner setup done");
    }

    /**
     * 既存の丸窓インスタンスを壊れた窓プレハブのインスタンスに差し替える。
     * すでに差し替え済みなら既存の壊れた窓を返す(再実行可)。
     */
    private static Transform ReplaceWindow(GameObject brokenWindowPrefab, string suffix)
    {
        string brokenName = $"round_window_cross_01_broken_32x32_{suffix}";
        GameObject existing = GameObject.Find(brokenName);
        if (existing != null)
            return existing.transform;

        string originalName = $"round_window_cross_01_32x32_{suffix}";
        GameObject original = GameObject.Find(originalName);
        if (original == null)
            throw new System.InvalidOperationException($"Window not found: {originalName}");

        GameObject broken = (GameObject)
            PrefabUtility.InstantiatePrefab(brokenWindowPrefab, original.scene);
        broken.name = brokenName;
        broken.transform.SetParent(original.transform.parent, false);
        broken.transform.position = original.transform.position;
        broken.transform.localScale = original.transform.localScale;
        broken.transform.SetSiblingIndex(original.transform.GetSiblingIndex());
        Object.DestroyImmediate(original);
        return broken.transform;
    }
}
