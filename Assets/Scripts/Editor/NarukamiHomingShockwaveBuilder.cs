using System.Collections.Generic;
using TNRD;
using UnityEditor;
using UnityEngine;
using VLCNP.Combat.EnemyAction;
using VLCNP.Projectiles;
using VLCNP.Stats;

/**
 * VLNarukamiBoss の最初の衝撃波攻撃を、8方向へ発生する追尾衝撃波攻撃へ差し替えるセットアップ。
 *   1. NarukamiHomingShockwave.prefab を生成(壊せるようHealth + BaseStatsを付与)
 *   2. VLNarukamiBoss.prefab へ FlapAndLaunchHomingShockwaves を追加
 *   3. ボスプレハブの行動リスト内、最初の FlapAndLaunchShockwaves だけを差し替え
 * 再実行しても安全(既存があれば作り直さず更新のみ行う)。
 */
public static class NarukamiHomingShockwaveBuilder
{
    private const string SpritePath = "Assets/Game/Projectiles/Sprite/narukami_shockwave_128.png";
    private const string HitEffectPath = "Assets/Game/Projectiles/Effect/WhiteHitEffect.prefab";
    // 敵キャラの被弾音(TakeDamageSe)と同じクリップ
    private const string TakeDamageSePath = "Assets/Game/SE/powerup03 1.mp3";
    private const string ProgressionPath = "Assets/Game/Stats/Progression.asset";
    private const string ShockwavePrefabPath = "Assets/Game/Projectiles/NarukamiHomingShockwave.prefab";
    private const string BossPrefabPath = "Assets/Game/Characters/Enemy/VLNarukamiBoss.prefab";

    [MenuItem("Tools/VLCNP/Narukami Homing Shockwave/Setup All")]
    public static void SetupAll()
    {
        NarukamiHomingShockwave shockwave = CreateOrUpdateShockwavePrefab();
        bool replaced = AddActionAndReplaceFirstInBossPrefab(shockwave);
        Debug.Log($"NarukamiHomingShockwaveBuilder: 完了。最初の攻撃の差し替え={replaced}");
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

        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(ShockwavePrefabPath);
        GameObject root = existing != null
            ? PrefabUtility.LoadPrefabContents(ShockwavePrefabPath)
            : new GameObject("NarukamiHomingShockwave");

        root.name = "NarukamiHomingShockwave";
        // プレイヤーの攻撃(targetTagName=Enemy)で壊せるようにEnemyタグにする
        root.tag = "Enemy";
        root.layer = 0;

        SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        // NarukamiShockwave と同じ描画順
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
        AudioClip damageSe = AssetDatabase.LoadAssetAtPath<AudioClip>(TakeDamageSePath);
        if (damageSe == null)
            throw new System.InvalidOperationException($"SE load failed: {TakeDamageSePath}");

        SerializedObject serializedProjectile = new SerializedObject(projectile);
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

        GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, ShockwavePrefabPath);
        if (existing != null)
            PrefabUtility.UnloadPrefabContents(root);
        else
            Object.DestroyImmediate(root);

        return saved.GetComponent<NarukamiHomingShockwave>();
    }

    private static bool AddActionAndReplaceFirstInBossPrefab(NarukamiHomingShockwave shockwave)
    {
        GameObject bossRoot = PrefabUtility.LoadPrefabContents(BossPrefabPath);
        try
        {
            FlapAndLaunchHomingShockwaves action =
                bossRoot.GetComponent<FlapAndLaunchHomingShockwaves>();
            if (action == null)
                action = bossRoot.AddComponent<FlapAndLaunchHomingShockwaves>();

            SerializedObject serializedAction = new SerializedObject(action);
            serializedAction.FindProperty("shockwavePrefab").objectReferenceValue = shockwave;
            serializedAction.ApplyModifiedPropertiesWithoutUndo();

            bool replaced = false;
            EnemyV2Controller controller = bossRoot.GetComponent<EnemyV2Controller>();
            if (controller != null)
            {
                List<SerializableInterface<IEnemyAction>> actions = controller.enemyActions;
                for (int i = 0; i < actions.Count; i++)
                {
                    if (actions[i]?.Value is FlapAndLaunchHomingShockwaves)
                    {
                        // 差し替え済み
                        replaced = true;
                        break;
                    }
                    if (actions[i]?.Value is FlapAndLaunchShockwaves)
                    {
                        actions[i].Value = action;
                        replaced = true;
                        break;
                    }
                }
            }

            PrefabUtility.SaveAsPrefabAsset(bossRoot, BossPrefabPath);
            return replaced;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(bossRoot);
        }
    }
}
