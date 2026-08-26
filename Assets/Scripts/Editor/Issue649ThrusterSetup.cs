using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VLCNP.Movement;

/**
 * #649 スラスター飛行のセットアップ用エディタ拡張。
 * 風パフスプライトのインポート設定、Player.prefab への ThrusterFlight 追加、
 * 噴射パーティクルの構築と参照の配線を行う。
 */
public static class Issue649ThrusterSetup
{
    const string PlayerPrefabPath = "Assets/Game/Characters/Player.prefab";
    const string WindPuffTexturePath = "Assets/Game/Effect/wind_puff_sheet_48x16.png";
    const string ThrusterSePath = "Assets/Game/SE/thruster.wav";
    const string EffectObjectName = "ThrusterWindEffect";
    const string NarukamiTexturePath = "Assets/Game/Characters/Sprite/VLNarukami.png";
    const string NarukamiAnimPath = "Assets/Game/Characters/Animations/VLNarukamiCarry.anim";
    const string NarukamiControllerPath =
        "Assets/Game/Characters/Animations/VLNarukamiCarryController.controller";
    const string NarukamiObjectName = "ThrusterNarukami";
    const float NarukamiScale = 0.4f;

    // 頭上オフセット計算用(いちばん背の高いキャラに合わせる)
    static readonly string[] PlayerVariantPaths =
    {
        PlayerPrefabPath,
        "Assets/Game/Characters/LeeleePlayerVariant.prefab",
        "Assets/Game/Characters/OrochiPlayerVariant.prefab",
        "Assets/Game/Characters/VLMitamaPlayerVariant.prefab",
    };

    [MenuItem("Tools/Issue649/Setup Thruster")]
    public static void Setup()
    {
        ConfigureWindPuffSprite();
        Sprite[] sprites = LoadWindPuffSprites();
        if (sprites.Length == 0)
        {
            Debug.LogError($"[Issue649] 風パフスプライトが見つかりません: {WindPuffTexturePath}");
            return;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
        try
        {
            SetupThrusterFlight(root, sprites);
            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            Debug.Log("[Issue649] Player.prefab に ThrusterFlight をセットアップしました");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    [MenuItem("Tools/Issue649/Setup Narukami Carry")]
    public static void SetupNarukamiCarry()
    {
        Sprite[] narukamiSprites = LoadNarukamiSprites();
        if (narukamiSprites.Length < 2)
        {
            Debug.LogError($"[Issue649] ナルカミのスプライトが不足しています: {NarukamiTexturePath}");
            return;
        }

        AnimationClip clip = CreateCarryAnimation(narukamiSprites);
        AnimatorController controller = CreateCarryController(clip);

        GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
        try
        {
            SetupNarukamiChild(root, narukamiSprites[0], controller);
            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            Debug.Log("[Issue649] Player.prefab に ThrusterNarukami をセットアップしました");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static Sprite[] LoadNarukamiSprites()
    {
        // VLNarukami_0(翼上), VLNarukami_1(翼中)。VLNarukami_2 は非飛行ポーズなので使わない
        return AssetDatabase
            .LoadAllAssetsAtPath(NarukamiTexturePath)
            .OfType<Sprite>()
            .Where(s => s.name == "VLNarukami_0" || s.name == "VLNarukami_1")
            .OrderBy(s => s.name)
            .ToArray();
    }

    static AnimationClip CreateCarryAnimation(Sprite[] sprites)
    {
        AnimationClip clip = new() { frameRate = 8f };
        EditorCurveBinding binding = new()
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite",
        };
        // 2コマループ(0→1→0)。最後のキーはループの継ぎ目用
        ObjectReferenceKeyframe[] keyframes =
        {
            new() { time = 0f, value = sprites[0] },
            new() { time = 0.125f, value = sprites[1] },
            new() { time = 0.25f, value = sprites[0] },
        };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, NarukamiAnimPath);
        return clip;
    }

    static AnimatorController CreateCarryController(AnimationClip clip)
    {
        AssetDatabase.DeleteAsset(NarukamiControllerPath);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(
            NarukamiControllerPath
        );
        controller.AddMotion(clip);
        return controller;
    }

    static void SetupNarukamiChild(GameObject root, Sprite sprite, AnimatorController controller)
    {
        Transform existing = root.transform.Find(NarukamiObjectName);
        GameObject narukamiObject =
            existing != null ? existing.gameObject : new GameObject(NarukamiObjectName);
        narukamiObject.transform.SetParent(root.transform, false);
        narukamiObject.transform.localPosition = new Vector3(0f, CalculateHeadOffsetY(), 0f);
        narukamiObject.transform.localScale = Vector3.one * NarukamiScale;

        SpriteRenderer renderer = narukamiObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = narukamiObject.AddComponent<SpriteRenderer>();
        }
        renderer.sprite = sprite;

        // プレイヤーの背面に表示
        SpriteRenderer playerRenderer = root.GetComponent<SpriteRenderer>();
        if (playerRenderer != null)
        {
            renderer.sortingLayerID = playerRenderer.sortingLayerID;
            renderer.sortingOrder = playerRenderer.sortingOrder - 1;
        }

        Animator animator = narukamiObject.GetComponent<Animator>();
        if (animator == null)
        {
            animator = narukamiObject.AddComponent<Animator>();
        }
        animator.runtimeAnimatorController = controller;

        ThrusterNarukamiVisual visual = narukamiObject.GetComponent<ThrusterNarukamiVisual>();
        if (visual == null)
        {
            visual = narukamiObject.AddComponent<ThrusterNarukamiVisual>();
        }

        ThrusterFlight thruster = root.GetComponent<ThrusterFlight>();
        if (thruster != null)
        {
            SerializedObject so = new(thruster);
            so.FindProperty("narukamiVisual").objectReferenceValue = visual;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        else
        {
            Debug.LogWarning("[Issue649] ThrusterFlight が見つからないため narukamiVisual を配線できません");
        }
    }

    // いちばん背の高いキャラの頭上にナルカミの足元が少し重なる高さを求める
    static float CalculateHeadOffsetY()
    {
        float maxTop = 0f;
        foreach (string path in PlayerVariantPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            SpriteRenderer sr = prefab != null ? prefab.GetComponent<SpriteRenderer>() : null;
            if (sr == null || sr.sprite == null)
                continue;
            maxTop = Mathf.Max(maxTop, sr.sprite.bounds.max.y);
        }

        Sprite narukamiSprite = LoadNarukamiSprites().FirstOrDefault();
        float narukamiExtentY =
            narukamiSprite != null ? narukamiSprite.bounds.extents.y * NarukamiScale : 0.5f;
        const float overlap = 0.15f;
        return maxTop + narukamiExtentY - overlap;
    }

    static void ConfigureWindPuffSprite()
    {
        TextureImporter importer = AssetImporter.GetAtPath(WindPuffTexturePath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"[Issue649] TextureImporter が取得できません: {WindPuffTexturePath}");
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 32;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;

        List<SpriteMetaData> metas = new();
        for (int i = 0; i < 3; i++)
        {
            metas.Add(
                new SpriteMetaData
                {
                    name = $"wind_puff_{i}",
                    rect = new Rect(i * 16, 0, 16, 16),
                    alignment = (int)SpriteAlignment.Center,
                }
            );
        }
        importer.spritesheet = metas.ToArray();
        importer.SaveAndReimport();
    }

    static Sprite[] LoadWindPuffSprites()
    {
        return AssetDatabase
            .LoadAllAssetsAtPath(WindPuffTexturePath)
            .OfType<Sprite>()
            .OrderBy(s => s.name)
            .ToArray();
    }

    static void SetupThrusterFlight(GameObject root, Sprite[] sprites)
    {
        ThrusterFlight thruster = root.GetComponent<ThrusterFlight>();
        if (thruster == null)
        {
            thruster = root.AddComponent<ThrusterFlight>();
        }

        ParticleSystem effect = SetupEffect(root, sprites);

        SerializedObject so = new(thruster);
        so.FindProperty("leg").objectReferenceValue = root.GetComponentInChildren<Leg>(true);
        so.FindProperty("thrustEffect").objectReferenceValue = effect;

        // Jump と同じ AudioSource を使う
        Jump jump = root.GetComponent<Jump>();
        if (jump != null)
        {
            SerializedObject jumpSo = new(jump);
            so.FindProperty("audioSource").objectReferenceValue = jumpSo
                .FindProperty("jumpAudioSource")
                .objectReferenceValue;
        }

        AudioClip se = AssetDatabase.LoadAssetAtPath<AudioClip>(ThrusterSePath);
        if (se == null)
        {
            Debug.LogWarning($"[Issue649] スラスターSEが見つかりません: {ThrusterSePath}");
        }
        so.FindProperty("thrustSe").objectReferenceValue = se;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static ParticleSystem SetupEffect(GameObject root, Sprite[] sprites)
    {
        Transform existing = root.transform.Find(EffectObjectName);
        GameObject effectObject = existing != null ? existing.gameObject : new GameObject(EffectObjectName);
        effectObject.transform.SetParent(root.transform, false);
        effectObject.transform.localPosition = new Vector3(0f, 0.5f, 0f);

        ParticleSystem ps = effectObject.GetComponent<ParticleSystem>();
        if (ps == null)
        {
            ps = effectObject.AddComponent<ParticleSystem>();
        }

        ParticleSystem.MainModule main = ps.main;
        main.loop = true;
        main.playOnAwake = false;
        main.duration = 1f;
        main.startLifetime = 0.4f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.6f);
        main.startColor = Color.white;
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 30;

        // 洞窟物語準拠: 約3フレーム(50fps)ごとにパフを1つ残す
        ParticleSystem.EmissionModule emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 16f;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;

        // 寿命の終わりに向けてフェードアウト
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.6f),
                new GradientAlphaKey(0f, 1f),
            }
        );
        colorOverLifetime.color = gradient;

        // 3コマの風パフアニメーション
        ParticleSystem.TextureSheetAnimationModule sheet = ps.textureSheetAnimation;
        sheet.enabled = true;
        sheet.mode = ParticleSystemAnimationMode.Sprites;
        while (sheet.spriteCount > 0)
        {
            sheet.RemoveSprite(0);
        }
        foreach (Sprite sprite in sprites)
        {
            sheet.AddSprite(sprite);
        }
        sheet.frameOverTime = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0f, 1f, 1f));

        ParticleSystemRenderer renderer = effectObject.GetComponent<ParticleSystemRenderer>();
        renderer.material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");

        // プレイヤーの背面に表示
        SpriteRenderer playerRenderer = root.GetComponent<SpriteRenderer>();
        if (playerRenderer != null)
        {
            renderer.sortingLayerID = playerRenderer.sortingLayerID;
            renderer.sortingOrder = playerRenderer.sortingOrder - 1;
        }

        return ps;
    }
}
