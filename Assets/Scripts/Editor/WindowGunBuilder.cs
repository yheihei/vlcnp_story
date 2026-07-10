using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VLCNP.Combat;

/** 窓の銃ギミック(WindowGun)のプレハブ生成とシーン配置を行うエディタツール */
public static class WindowGunBuilder
{
    private const string BarrelSpritePath =
        "Assets/Game/Projectiles/Sprite/window_gun_barrel.png";
    private const string BulletSpritePath =
        "Assets/Game/Projectiles/Sprite/window_gun_bullet.png";
    private const string FireEffectPath = "Assets/Game/Projectiles/Effect/WhiteHitEffect.prefab";
    private const string BulletPrefabPath = "Assets/Game/Projectiles/WindowGunBullet.prefab";
    private const string GunPrefabPath = "Assets/Game/Characters/Enemy/WindowGun.prefab";
    private const string DownGunPrefabPath =
        "Assets/Game/Characters/Enemy/DownWindowGun.prefab";

    [MenuItem("Tools/WindowGun/Build Prefabs", false, 3000)]
    public static void BuildPrefabs()
    {
        // 銃身は左右反転の軸にするため銃尻(右端)をピボットにする
        ConfigureSpriteImporter(BarrelSpritePath, new Vector2(1f, 0.5f));
        ConfigureSpriteImporter(BulletSpritePath, null);

        GameObject bulletPrefab = BuildBulletPrefab();
        GameObject gunPrefab = BuildGunPrefab(bulletPrefab);
        BuildDownGunPrefab(gunPrefab);
        AssetDatabase.SaveAssets();
        Debug.Log("WindowGun prefabs built.");
    }

    private static void ConfigureSpriteImporter(string path, Vector2? customPivot)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 32;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)(
            customPivot.HasValue ? SpriteAlignment.Custom : SpriteAlignment.Center
        );
        importer.SetTextureSettings(settings);
        if (customPivot.HasValue)
        {
            importer.spritePivot = customPivot.Value;
        }
        importer.SaveAndReimport();
    }

    private static GameObject BuildBulletPrefab()
    {
        Sprite bulletSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BulletSpritePath);
        GameObject fireEffect = AssetDatabase.LoadAssetAtPath<GameObject>(FireEffectPath);

        GameObject bullet = new GameObject("WindowGunBullet");
        bullet.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        SpriteRenderer renderer = bullet.AddComponent<SpriteRenderer>();
        renderer.sprite = bulletSprite;
        renderer.sortingOrder = 10;

        CircleCollider2D collider = bullet.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.14f;

        Projectile projectile = bullet.AddComponent<Projectile>();
        SerializedObject so = new SerializedObject(projectile);
        so.FindProperty("speed").floatValue = 3.5f;
        // 時間ではなく飛距離(DestroyAfterMovedDistance)で消す
        so.FindProperty("deleteTime").floatValue = -1f;
        so.FindProperty("targetTagName").stringValue = "Player";
        so.FindProperty("IsPenetration").boolValue = false;
        so.FindProperty("hitEffect").objectReferenceValue = fireEffect;
        so.ApplyModifiedPropertiesWithoutUndo();

        DestroyAfterMovedDistance destroyAfterMovedDistance =
            bullet.AddComponent<DestroyAfterMovedDistance>();
        SerializedObject soDistance = new SerializedObject(destroyAfterMovedDistance);
        soDistance.FindProperty("maxDistance").floatValue = 8f;
        soDistance.FindProperty("fadeOutDuration").floatValue = 0.8f;
        soDistance.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(bullet, BulletPrefabPath);
        Object.DestroyImmediate(bullet);
        return prefab;
    }

    private static GameObject BuildGunPrefab(GameObject bulletPrefab)
    {
        Sprite barrelSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BarrelSpritePath);
        GameObject fireEffect = AssetDatabase.LoadAssetAtPath<GameObject>(FireEffectPath);

        GameObject gun = new GameObject("WindowGun");
        gun.tag = "Enemy";

        SpriteRenderer renderer = gun.AddComponent<SpriteRenderer>();
        renderer.sprite = barrelSprite;
        renderer.sortingOrder = 5;

        // ピボットが銃尻(右端)なのでコライダーと銃口は左側にオフセットする
        BoxCollider2D collider = gun.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.8f, 0.36f);
        collider.offset = new Vector2(-0.4f, 0f);

        GameObject muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(gun.transform);
        muzzle.transform.localPosition = new Vector3(-0.75f, 0f, 0f);

        WindowGun windowGun = gun.AddComponent<WindowGun>();
        SerializedObject so = new SerializedObject(windowGun);
        so.FindProperty("projectilePrefab").objectReferenceValue =
            bulletPrefab.GetComponent<Projectile>();
        so.FindProperty("fireEffect").objectReferenceValue = fireEffect;
        so.FindProperty("muzzleTransform").objectReferenceValue = muzzle.transform;
        so.FindProperty("fireInterval").floatValue = 3f;
        so.FindProperty("damage").floatValue = 1f;
        so.FindProperty("isLeft").boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(gun, GunPrefabPath);
        Object.DestroyImmediate(gun);
        return prefab;
    }

    private static void BuildDownGunPrefab(GameObject gunPrefab)
    {
        GameObject gun = (GameObject)PrefabUtility.InstantiatePrefab(gunPrefab);
        gun.name = "DownWindowGun";
        gun.transform.rotation = Quaternion.Euler(0f, 0f, 90f);

        SerializedObject so = new SerializedObject(gun.GetComponent<WindowGun>());
        so.FindProperty("isDownward").boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(gun, DownGunPrefabPath);
        Object.DestroyImmediate(gun);
    }

    [MenuItem("Tools/WindowGun/Place In Kaze1 Windows 5-7", false, 3001)]
    public static void PlaceInKaze1()
    {
        GameObject gunPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GunPrefabPath);
        if (gunPrefab == null)
        {
            Debug.LogError("WindowGun.prefab not found. Run Build Prefabs first.");
            return;
        }

        string[] windowNames =
        {
            "round_window_lattice_02_32x32_5",
            "round_window_lattice_02_32x32_6",
            "round_window_lattice_02_32x32_7",
        };
        float firstDelay = 0f;
        foreach (string windowName in windowNames)
        {
            GameObject window = GameObject.Find(windowName);
            if (window == null)
            {
                Debug.LogError($"Window not found: {windowName}");
                continue;
            }
            string gunName = $"WindowGun_{windowName}";
            if (GameObject.Find(gunName) != null)
            {
                Debug.Log($"Already placed: {gunName}");
                continue;
            }
            GameObject gun = (GameObject)PrefabUtility.InstantiatePrefab(gunPrefab);
            gun.name = gunName;
            // 銃尻ピボットを窓の中心に置く。向きは実行時にプレイヤー位置で切り替わる
            gun.transform.position = window.transform.position + new Vector3(0f, -0.02f, 0f);
            SerializedObject so = new SerializedObject(gun.GetComponent<WindowGun>());
            so.FindProperty("firstDelay").floatValue = firstDelay;
            so.ApplyModifiedPropertiesWithoutUndo();
            firstDelay += 1f;
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("WindowGun placed at Kaze1 windows 5-7.");
    }
}
