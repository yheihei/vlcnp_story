using System.Collections.Generic;
using TNRD;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VLCNP.Combat;
using VLCNP.Combat.EnemyAction;

/**
 * VLNarukamiBoss の竜巻攻撃を衝撃波攻撃(FlapAndLaunchShockwaves)へ差し替えるためのセットアップ。
 *   1. narukami_shockwave_128.png のインポート設定を整え、NarukamiShockwave.prefab を生成
 *   2. VLNarukamiBoss.prefab へ FlapAndLaunchShockwaves を追加して衝撃波プレハブを紐付け
 *   3. 開いているシーンのボスインスタンスの行動リスト内 LaunchTornadoesToPlayer を差し替え
 * 再実行しても安全(既存があれば作り直さず更新のみ行う)。
 */
public static class NarukamiShockwaveBuilder
{
    private const string SpritePath = "Assets/Game/Projectiles/Sprite/narukami_shockwave_128.png";
    private const string ShockwavePrefabPath = "Assets/Game/Projectiles/NarukamiShockwave.prefab";
    private const string BossPrefabPath = "Assets/Game/Characters/Enemy/VLNarukamiBoss.prefab";

    [MenuItem("Tools/VLCNP/Narukami Shockwave/Setup All")]
    public static void SetupAll()
    {
        ConfigureSpriteImport();
        Projectile shockwave = CreateOrUpdateShockwavePrefab();
        AddActionToBossPrefab(shockwave);
        int replaced = RewireOpenSceneActions();
        Debug.Log($"NarukamiShockwaveBuilder: 完了。シーン内の差し替え数={replaced}");
    }

    private static void ConfigureSpriteImport()
    {
        TextureImporter importer = AssetImporter.GetAtPath(SpritePath) as TextureImporter;
        if (importer == null)
        {
            throw new System.InvalidOperationException($"Sprite not found: {SpritePath}");
        }
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
    }

    private static Projectile CreateOrUpdateShockwavePrefab()
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        if (sprite == null)
        {
            throw new System.InvalidOperationException($"Sprite asset load failed: {SpritePath}");
        }

        GameObject root;
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(ShockwavePrefabPath);
        root = existing != null
            ? PrefabUtility.LoadPrefabContents(ShockwavePrefabPath)
            : new GameObject("NarukamiShockwave");

        root.name = "NarukamiShockwave";
        root.tag = "Projectile";
        root.layer = 0;

        SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        // VLKamaitachiTornado と同じ描画順に合わせる
        renderer.sortingLayerID = 1751024655;
        renderer.sortingOrder = 1000;

        BoxCollider2D collider = root.GetComponent<BoxCollider2D>();
        if (collider == null)
            collider = root.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        Vector2 spriteSize = sprite.bounds.size;
        // 三日月の実体(凸側)に寄せた当たり判定
        collider.size = new Vector2(spriteSize.x * 0.45f, spriteSize.y * 0.8f);
        collider.offset = new Vector2(spriteSize.x * 0.1f, 0f);

        Projectile projectile = root.GetComponent<Projectile>();
        if (projectile == null)
            projectile = root.AddComponent<Projectile>();
        SerializedObject serializedProjectile = new SerializedObject(projectile);
        serializedProjectile.FindProperty("speed").floatValue = 12f;
        serializedProjectile.FindProperty("deleteTime").floatValue = 3f;
        serializedProjectile.FindProperty("targetTagName").stringValue = "Player";
        serializedProjectile.FindProperty("IsPenetration").boolValue = false;
        serializedProjectile.FindProperty("isFadeOut").boolValue = true;
        serializedProjectile.FindProperty("mirrorScaleWhenFacingLeft").boolValue = true;
        serializedProjectile.ApplyModifiedPropertiesWithoutUndo();

        GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, ShockwavePrefabPath);
        if (existing != null)
            PrefabUtility.UnloadPrefabContents(root);
        else
            Object.DestroyImmediate(root);

        return saved.GetComponent<Projectile>();
    }

    private static void AddActionToBossPrefab(Projectile shockwave)
    {
        GameObject bossRoot = PrefabUtility.LoadPrefabContents(BossPrefabPath);
        try
        {
            FlapAndLaunchShockwaves action = bossRoot.GetComponent<FlapAndLaunchShockwaves>();
            if (action == null)
                action = bossRoot.AddComponent<FlapAndLaunchShockwaves>();

            SerializedObject serializedAction = new SerializedObject(action);
            serializedAction.FindProperty("shockwavePrefab").objectReferenceValue = shockwave;
            serializedAction.FindProperty("damage").floatValue = 2f;
            serializedAction.FindProperty("shotCount").intValue = 3;
            serializedAction.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(bossRoot, BossPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(bossRoot);
        }
    }

    private static int RewireOpenSceneActions()
    {
        int replacedCount = 0;
        foreach (EnemyV2Controller controller in Object.FindObjectsOfType<EnemyV2Controller>(true))
        {
            FlapAndLaunchShockwaves shockwaveAction =
                controller.GetComponent<FlapAndLaunchShockwaves>();
            if (shockwaveAction == null)
                continue;

            bool changed = false;
            List<SerializableInterface<IEnemyAction>> actions = controller.enemyActions;
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i]?.Value is LaunchTornadoesToPlayer)
                {
                    actions[i].Value = shockwaveAction;
                    changed = true;
                    replacedCount++;
                }
            }

            if (changed)
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
                EditorUtility.SetDirty(controller);
                EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            }
        }
        return replacedCount;
    }
}
