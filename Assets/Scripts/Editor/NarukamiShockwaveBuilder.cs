using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using VLCNP.Combat.EnemyAction;
using VLCNP.Pickups;
using VLCNP.Projectiles;
using VLCNP.Stats;

/**
 * VLNarukamiBoss のプレイヤーへ向けて撃つ衝撃波(NarukamiShockwave)を、
 * 8方向の追尾衝撃波と同じく壊せる構成へ変換するセットアップ。
 *   1. NarukamiShockwave.prefab を NarukamiHomingShockwave(追尾なし・直進設定)へ載せ替え、
 *      Health + BaseStats + ドロップ(RainbowSeedSmall x2 / HartRecover)を付与
 *   2. VLNarukamiBoss.prefab の FlapAndLaunchShockwaves へ衝撃波プレハブを紐付け
 * 再実行しても安全(既存があれば作り直さず更新のみ行う)。
 */
public static class NarukamiShockwaveBuilder
{
    private const string SpritePath = "Assets/Game/Projectiles/Sprite/narukami_shockwave_128.png";
    private const string HitEffectPath = "Assets/Game/Projectiles/Effect/WhiteHitEffect.prefab";
    // 敵キャラの被弾音(TakeDamageSe)と同じクリップ
    private const string TakeDamageSePath = "Assets/Game/SE/powerup03 1.mp3";
    private const string ExperiencePickupPath = "Assets/Game/Pickups/RainbowSeedSmall.prefab";
    private const string DropItemPickupPath = "Assets/Game/Pickups/HartRecover.prefab";
    private const string ProgressionPath = "Assets/Game/Stats/Progression.asset";
    private const string ShockwavePrefabPath = "Assets/Game/Projectiles/NarukamiShockwave.prefab";
    private const string BossPrefabPath = "Assets/Game/Characters/Enemy/VLNarukamiBoss.prefab";

    [MenuItem("Tools/VLCNP/Narukami Shockwave/Setup All")]
    public static void SetupAll()
    {
        NarukamiHomingShockwave shockwave = CreateOrUpdateShockwavePrefab();
        AddActionToBossPrefab(shockwave);
        Debug.Log("NarukamiShockwaveBuilder: 完了。");
    }

    private static NarukamiHomingShockwave CreateOrUpdateShockwavePrefab()
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        if (sprite == null)
            throw new System.InvalidOperationException($"Sprite load failed: {SpritePath}");
        GameObject hitEffect = AssetDatabase.LoadAssetAtPath<GameObject>(HitEffectPath);
        if (hitEffect == null)
            throw new System.InvalidOperationException($"HitEffect load failed: {HitEffectPath}");
        Progression progression = AssetDatabase.LoadAssetAtPath<Progression>(ProgressionPath);
        if (progression == null)
            throw new System.InvalidOperationException($"Progression load failed: {ProgressionPath}");
        AudioClip damageSe = AssetDatabase.LoadAssetAtPath<AudioClip>(TakeDamageSePath);
        if (damageSe == null)
            throw new System.InvalidOperationException($"SE load failed: {TakeDamageSePath}");

        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(ShockwavePrefabPath);
        GameObject root = existing != null
            ? PrefabUtility.LoadPrefabContents(ShockwavePrefabPath)
            : new GameObject("NarukamiShockwave");

        root.name = "NarukamiShockwave";
        // プレイヤーの攻撃(targetTagName=Enemy)で壊せるようにEnemyタグにする
        root.tag = "Enemy";
        root.layer = 0;

        // 旧構成(汎用Projectile + BoxCollider2D)が残っていれば取り除く
        VLCNP.Combat.Projectile legacyProjectile = root.GetComponent<VLCNP.Combat.Projectile>();
        if (legacyProjectile != null)
            Object.DestroyImmediate(legacyProjectile);
        BoxCollider2D legacyCollider = root.GetComponent<BoxCollider2D>();
        if (legacyCollider != null)
            Object.DestroyImmediate(legacyCollider);

        SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        // VLKamaitachiTornado と同じ描画順に合わせる
        renderer.sortingLayerID = 1751024655;
        renderer.sortingOrder = 1000;

        CircleCollider2D collider = root.GetComponent<CircleCollider2D>();
        if (collider == null)
            collider = root.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        // 自転するため回転対称の円コライダー。三日月の実体に寄せて少し小さめ
        collider.radius = sprite.bounds.size.x * 0.28f;

        Rigidbody2D rbody = root.GetComponent<Rigidbody2D>();
        if (rbody == null)
            rbody = root.AddComponent<Rigidbody2D>();
        rbody.bodyType = RigidbodyType2D.Kinematic;
        rbody.gravityScale = 0f;
        // Kinematicのまま地面(静的コライダー)とのTriggerを発生させる
        rbody.useFullKinematicContacts = true;

        NarukamiHomingShockwave projectile = root.GetComponent<NarukamiHomingShockwave>();
        if (projectile == null)
            projectile = root.AddComponent<NarukamiHomingShockwave>();
        SerializedObject serializedProjectile = new SerializedObject(projectile);
        // 追尾なしで発射方向へ直進させる。速度・寿命は旧Projectile構成と同じ
        serializedProjectile.FindProperty("initialSpeed").floatValue = 8f;
        serializedProjectile.FindProperty("maxSpeed").floatValue = 8f;
        serializedProjectile.FindProperty("homingAcceleration").floatValue = 0f;
        serializedProjectile.FindProperty("homingDuration").floatValue = 0f;
        serializedProjectile.FindProperty("selfRotationSpeed").floatValue = 1080f;
        serializedProjectile.FindProperty("maxLifetime").floatValue = 3f;
        serializedProjectile.FindProperty("spawnFadeDuration").floatValue = 0f;
        // 旧構成は地形を通過していたため、通過を維持する
        serializedProjectile.FindProperty("breakOnGround").boolValue = false;
        serializedProjectile.FindProperty("hitEffect").objectReferenceValue = hitEffect;
        serializedProjectile.FindProperty("takeDamageSe").objectReferenceValue = damageSe;
        serializedProjectile.ApplyModifiedPropertiesWithoutUndo();

        BaseStats baseStats = root.GetComponent<BaseStats>();
        if (baseStats == null)
            baseStats = root.AddComponent<BaseStats>();
        SerializedObject serializedStats = new SerializedObject(baseStats);
        serializedStats.FindProperty("statClass").intValue = (int)StatClass.NarukamiShockwave;
        serializedStats.FindProperty("progression").objectReferenceValue = progression;
        serializedStats.ApplyModifiedPropertiesWithoutUndo();

        VLCNP.Attributes.Health health = root.GetComponent<VLCNP.Attributes.Health>();
        if (health == null)
            health = root.AddComponent<VLCNP.Attributes.Health>();
        SerializedObject serializedHealth = new SerializedObject(health);
        serializedHealth.FindProperty("invincibleTime").floatValue = 0.1f;
        serializedHealth.FindProperty("deadEffect").objectReferenceValue = hitEffect;
        serializedHealth.ApplyModifiedPropertiesWithoutUndo();

        SetupDrops(root, health);

        GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, ShockwavePrefabPath);
        if (existing != null)
            PrefabUtility.UnloadPrefabContents(root);
        else
            Object.DestroyImmediate(root);

        return saved.GetComponent<NarukamiHomingShockwave>();
    }

    /**
     * 8方向の追尾衝撃波と同じ撃破時ドロップ構成にする(RainbowSeedSmall x2 + HartRecover 40%)。
     * dieEventに登録があるとHealthが自動でDeadEffectAndDestroyを呼ばなくなるため、
     * DeadEffectAndDestroyも明示的にdieEventへ登録する。
     */
    private static void SetupDrops(GameObject root, VLCNP.Attributes.Health health)
    {
        GameObject experiencePrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>(ExperiencePickupPath);
        if (experiencePrefab == null)
            throw new System.InvalidOperationException(
                $"Experience pickup load failed: {ExperiencePickupPath}"
            );
        BasePickup itemPickup = AssetDatabase
            .LoadAssetAtPath<GameObject>(DropItemPickupPath)
            ?.GetComponent<BasePickup>();
        if (itemPickup == null)
            throw new System.InvalidOperationException(
                $"Drop item pickup load failed: {DropItemPickupPath}"
            );

        DropExperience dropExperience = root.GetComponent<DropExperience>();
        if (dropExperience == null)
            dropExperience = root.AddComponent<DropExperience>();
        SerializedObject serializedDropExperience = new SerializedObject(dropExperience);
        SerializedProperty experiences = serializedDropExperience.FindProperty("experiences");
        experiences.arraySize = 2;
        experiences.GetArrayElementAtIndex(0).objectReferenceValue = experiencePrefab;
        experiences.GetArrayElementAtIndex(1).objectReferenceValue = experiencePrefab;
        serializedDropExperience.ApplyModifiedPropertiesWithoutUndo();

        DropItem dropItem = root.GetComponent<DropItem>();
        if (dropItem == null)
            dropItem = root.AddComponent<DropItem>();
        SerializedObject serializedDropItem = new SerializedObject(dropItem);
        serializedDropItem.FindProperty("dropItem").objectReferenceValue = itemPickup;
        serializedDropItem.FindProperty("dropRate").floatValue = 0.4f;
        serializedDropItem.ApplyModifiedPropertiesWithoutUndo();

        if (health.dieEvent.GetPersistentEventCount() == 0)
        {
            UnityEventTools.AddVoidPersistentListener(health.dieEvent, dropExperience.Drop);
            UnityEventTools.AddVoidPersistentListener(health.dieEvent, dropItem.Drop);
            UnityEventTools.AddVoidPersistentListener(
                health.dieEvent,
                health.DeadEffectAndDestroy
            );
        }
    }

    private static void AddActionToBossPrefab(NarukamiHomingShockwave shockwave)
    {
        GameObject bossRoot = PrefabUtility.LoadPrefabContents(BossPrefabPath);
        try
        {
            FlapAndLaunchShockwaves action = bossRoot.GetComponent<FlapAndLaunchShockwaves>();
            if (action == null)
                action = bossRoot.AddComponent<FlapAndLaunchShockwaves>();

            SerializedObject serializedAction = new SerializedObject(action);
            serializedAction.FindProperty("shockwavePrefab").objectReferenceValue = shockwave;
            serializedAction.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(bossRoot, BossPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(bossRoot);
        }
    }
}
