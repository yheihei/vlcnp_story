using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VLCNP.Effects;

/**
 * #656 環境演出の共通部品セットを生成するエディタ拡張。
 *
 * 生成物(すべて Assets/Game 配下):
 *   Rendering/AmbientSpriteLit.mat            ライト対応の環境用マテリアル(Sprite-Lit-Default ベース)
 *   Rendering/<Area>_Post Processing Profile   エリア別 Volume プロファイル(永遠・土・風)
 *   Effect/Ambient/AreaGlobalLight(_<Area>)    Global Light 2D とエリア別 Variant
 *   Effect/Ambient/AmbientPointLight           松明・ランプ用 Point Light 2D(ゆらぎ付き)
 *   Effect/Ambient/Ambient_<種類>              粒子 5 種(埃・火の粉・落ちる土粒と胞子・風筋と葉・霧)
 *   Effect/Ambient/AreaAtmosphere(_<Area>)     上記をまとめて置く親とエリア別 Variant
 *
 * 粒子スプライト(PPU32・Point)は Effect/Ambient/Sprites に置いてから実行する。
 * 再実行しても同じ結果になる(既存アセットは中身を作り直す。GUID は維持する)。
 */
public static class AmbientAtmosphereBuilder
{
    public const string RenderingDir = "Assets/Game/Rendering";
    public const string AmbientDir = "Assets/Game/Effect/Ambient";
    public const string SpritesDir = AmbientDir + "/Sprites";

    public const string LitMaterialPath = RenderingDir + "/AmbientSpriteLit.mat";
    public const string ParticleMaterialPath = SpritesDir + "/AmbientParticleUnlit.mat";
    public const string FogMaterialPath = SpritesDir + "/AmbientFog.mat";

    public const string DustSpritePath = SpritesDir + "/ambient_dust_8x8.png";
    public const string EmberSpritePath = SpritesDir + "/ambient_ember_8x8.png";
    public const string DebrisSheetPath = SpritesDir + "/ambient_debris_sheet_16x8.png";
    public const string WindSheetPath = SpritesDir + "/ambient_wind_sheet_32x8.png";
    public const string FogAtlasPath = SpritesDir + "/ambient_fog_atlas_1024x1024.png";

    public const string GlobalLightPath = AmbientDir + "/AreaGlobalLight.prefab";
    public const string PointLightPath = AmbientDir + "/AmbientPointLight.prefab";
    public const string DustPath = AmbientDir + "/Ambient_Dust.prefab";
    public const string EmbersPath = AmbientDir + "/Ambient_Embers.prefab";
    public const string DebrisPath = AmbientDir + "/Ambient_FallingDebris.prefab";
    public const string WindPath = AmbientDir + "/Ambient_WindStreaks.prefab";
    public const string FogPath = AmbientDir + "/Ambient_Fog.prefab";
    public const string AtmospherePath = AmbientDir + "/AreaAtmosphere.prefab";

    /** エリア名(ファイル名に使う) */
    public static readonly string[] Areas = { "Eien", "Tuti", "Kaze" };

    public static string GlobalLightVariantPath(string area) => $"{AmbientDir}/AreaGlobalLight_{area}.prefab";
    public static string ProfilePath(string area) => $"{RenderingDir}/{area}_Post Processing Profile.asset";
    public static string AtmosphereVariantPath(string area) => $"{AmbientDir}/AreaAtmosphere_{area}.prefab";

    /** Sprite モードの粒子は PPU を無視して startSize がそのまま 1 辺のワールド長になるので、8px を PPU32 相当の 0.25 で描く */
    private const float PixelSpriteSize = 8f / 32f;
    private const string EffectSortingLayer = "Effect";
    private const string FogSortingLayer = "BackGroundObject";

    [MenuItem("Tools/VLCNP/Ambient/Build Ambient Assets (#656)", false, 3400)]
    public static void Build()
    {
        EnsureFolder(RenderingDir);
        EnsureFolder(AmbientDir);
        EnsureFolder(SpritesDir);

        ConfigurePixelSprite(DustSpritePath, null);
        ConfigurePixelSprite(EmberSpritePath, null);
        ConfigurePixelSprite(DebrisSheetPath, new[]
        {
            new SpriteMetaData { name = "grain", rect = new Rect(0, 0, 8, 8), alignment = (int)SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f) },
            new SpriteMetaData { name = "spore", rect = new Rect(8, 0, 8, 8), alignment = (int)SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f) },
        });
        ConfigurePixelSprite(WindSheetPath, new[]
        {
            // Sprite モードの粒子は絵柄ごとの寸法差をクアッドに反映しないので、両方 16x8 のセルにそろえる
            new SpriteMetaData { name = "streak", rect = new Rect(0, 0, 16, 8), alignment = (int)SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f) },
            new SpriteMetaData { name = "leaf", rect = new Rect(16, 0, 16, 8), alignment = (int)SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f) },
        });
        ConfigureFogAtlas(FogAtlasPath);

        var litMaterial = BuildMaterial(LitMaterialPath, "Universal Render Pipeline/2D/Sprite-Lit-Default", null);
        var particleMaterial = BuildMaterial(ParticleMaterialPath, "Universal Render Pipeline/2D/Sprite-Unlit-Default", null);
        var fogMaterial = BuildMaterial(FogMaterialPath, "Universal Render Pipeline/2D/Sprite-Unlit-Default",
            AssetDatabase.LoadAssetAtPath<Texture2D>(FogAtlasPath));

        BuildVolumeProfiles();

        var globalLight = BuildGlobalLight();
        BuildGlobalLightVariants(globalLight);
        BuildPointLight();

        var dust = BuildDust(particleMaterial);
        BuildEmbers(particleMaterial);
        var debris = BuildFallingDebris(particleMaterial);
        var wind = BuildWindStreaks(particleMaterial);
        var fog = BuildFog(fogMaterial);

        var atmosphere = BuildAtmosphere(dust, fog, debris, wind);
        BuildAtmosphereVariants(atmosphere);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[AmbientAtmosphereBuilder] 環境演出の共通部品を生成した。lit={LitMaterialPath} atmosphere={AtmospherePath}");
    }

    // ---------------------------------------------------------------- folders / textures

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        var name = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }
        AssetDatabase.CreateFolder(parent, name);
    }

    /** 粒子スプライトを PPU32・Point・非圧縮・mipmap 無しで取り込む。sheet が null なら単一スプライト。 */
    private static void ConfigurePixelSprite(string path, SpriteMetaData[] sheet)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            throw new InvalidOperationException($"粒子スプライトが見つかりません: {path}");
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = sheet == null ? SpriteImportMode.Single : SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 32;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.isReadable = false;
        if (sheet != null)
        {
#pragma warning disable CS0618
            importer.spritesheet = sheet;
#pragma warning restore CS0618
        }
        importer.SaveAndReimport();
    }

    /** 霧アトラスは滑らかでよいので Bilinear。1 列 4 段のタイル。 */
    private static void ConfigureFogAtlas(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            throw new InvalidOperationException($"霧アトラスが見つかりません: {path}");
        }

        importer.textureType = TextureImporterType.Default;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.sRGBTexture = true;
        importer.SaveAndReimport();
    }

    private static Material BuildMaterial(string path, string shaderName, Texture2D mainTexture)
    {
        var shader = Shader.Find(shaderName);
        if (shader == null)
        {
            throw new InvalidOperationException($"シェーダが見つかりません: {shaderName}");
        }

        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        else
        {
            material.shader = shader;
        }

        if (mainTexture != null)
        {
            material.mainTexture = mainTexture;
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    // ---------------------------------------------------------------- volume profiles

    private static void BuildVolumeProfiles()
    {
        // 永遠: 眠りの墓所。琥珀の松明に対して影を青寄りに
        BuildProfile(ProfilePath("Eien"),
            colorFilter: new Color(1f, 0.97f, 0.92f), contrast: 5f, saturation: -5f,
            bloomIntensity: 0.2f, vignetteColor: new Color(0.07f, 0.09f, 0.22f), vignetteIntensity: 0.26f);

        // 土: 掘り進む。土色と苔の緑、少し湿った空気
        BuildProfile(ProfilePath("Tuti"),
            colorFilter: new Color(0.97f, 0.95f, 0.88f), contrast: 3f, saturation: -8f,
            bloomIntensity: 0.15f, vignetteColor: new Color(0.12f, 0.09f, 0.05f), vignetteIntensity: 0.24f);

        // 風: 高く、寒い。月夜の青紫
        BuildProfile(ProfilePath("Kaze"),
            colorFilter: new Color(0.9f, 0.93f, 1f), contrast: 8f, saturation: -4f,
            bloomIntensity: 0.3f, vignetteColor: new Color(0.05f, 0.05f, 0.16f), vignetteIntensity: 0.28f);
    }

    private static void BuildProfile(string path, Color colorFilter, float contrast, float saturation,
        float bloomIntensity, Color vignetteColor, float vignetteIntensity)
    {
        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);
        }
        else
        {
            foreach (var existing in profile.components.ToArray())
            {
                if (existing == null) continue;
                AssetDatabase.RemoveObjectFromAsset(existing);
                UnityEngine.Object.DestroyImmediate(existing, true);
            }
            profile.components.Clear();
        }

        var colorAdjustments = AddComponent<ColorAdjustments>(profile);
        colorAdjustments.postExposure.Override(0f);
        colorAdjustments.contrast.Override(contrast);
        colorAdjustments.colorFilter.Override(colorFilter);
        colorAdjustments.saturation.Override(saturation);

        var bloom = AddComponent<Bloom>(profile);
        bloom.threshold.Override(0.9f);
        bloom.intensity.Override(bloomIntensity);
        bloom.scatter.Override(0.7f);

        var vignette = AddComponent<Vignette>(profile);
        vignette.color.Override(vignetteColor);
        vignette.intensity.Override(vignetteIntensity);
        vignette.smoothness.Override(0.35f);

        EditorUtility.SetDirty(profile);
    }

    private static T AddComponent<T>(VolumeProfile profile) where T : VolumeComponent
    {
        var component = profile.Add<T>(false);
        component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
        AssetDatabase.AddObjectToAsset(component, profile);
        return component;
    }

    // ---------------------------------------------------------------- lights

    private static GameObject BuildGlobalLight()
    {
        var go = new GameObject("AreaGlobalLight");
        try
        {
            var light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.color = Color.white;
            light.intensity = 1f;
            ApplyToAllSortingLayers(light);
            return PrefabUtility.SaveAsPrefabAsset(go, GlobalLightPath);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    private static void BuildGlobalLightVariants(GameObject basePrefab)
    {
        // 永遠: 松明の琥珀が映えるよう、全体は少し暖かい白
        BuildVariant(basePrefab, GlobalLightVariantPath("Eien"), instance =>
        {
            var light = instance.GetComponent<Light2D>();
            light.color = new Color(0.96f, 0.90f, 0.80f);
            light.intensity = 1f;
        });
        // 土: 土色と苔の緑
        BuildVariant(basePrefab, GlobalLightVariantPath("Tuti"), instance =>
        {
            var light = instance.GetComponent<Light2D>();
            light.color = new Color(0.90f, 0.88f, 0.74f);
            light.intensity = 1f;
        });
        // 風: 月夜の青紫、冷たい逆光
        BuildVariant(basePrefab, GlobalLightVariantPath("Kaze"), instance =>
        {
            var light = instance.GetComponent<Light2D>();
            light.color = new Color(0.72f, 0.78f, 1f);
            light.intensity = 1f;
        });
    }

    private static void BuildPointLight()
    {
        var go = new GameObject("AmbientPointLight");
        try
        {
            var light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.color = new Color(1f, 0.72f, 0.40f);
            light.intensity = 0.9f;
            light.falloffIntensity = 0.45f;
            ApplyToAllSortingLayers(light);

            var so = new SerializedObject(light);
            so.FindProperty("m_PointLightInnerRadius").floatValue = 0.3f;
            so.FindProperty("m_PointLightOuterRadius").floatValue = 5f;
            so.FindProperty("m_PointLightInnerAngle").floatValue = 360f;
            so.FindProperty("m_PointLightOuterAngle").floatValue = 360f;
            so.ApplyModifiedPropertiesWithoutUndo();

            go.AddComponent<LightFlicker2D>();
            PrefabUtility.SaveAsPrefabAsset(go, PointLightPath);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    private static void ApplyToAllSortingLayers(Light2D light)
    {
        var so = new SerializedObject(light);
        var layers = so.FindProperty("m_ApplyToSortingLayers");
        var ids = SortingLayer.layers.Select(l => l.id).ToArray();
        layers.arraySize = ids.Length;
        for (var i = 0; i < ids.Length; i++)
        {
            layers.GetArrayElementAtIndex(i).intValue = ids[i];
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject BuildVariant(GameObject basePrefab, string path, Action<GameObject> modify)
    {
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
        try
        {
            modify(instance);
            return PrefabUtility.SaveAsPrefabAsset(instance, path);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(instance);
        }
    }

    // ---------------------------------------------------------------- particles

    private static ParticleSystem NewParticleSystem(GameObject go, Material material, string sortingLayer, int sortingOrder,
        float lifetimeMin, float lifetimeMax, int maxParticles, float rate)
    {
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.prewarm = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetimeMin, lifetimeMax);
        main.startSpeed = 0f;
        main.startSize = PixelSpriteSize;
        main.startRotation = 0f;
        main.startColor = Color.white;
        main.maxParticles = maxParticles;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        main.playOnAwake = true;
        main.cullingMode = ParticleSystemCullingMode.Automatic;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = rate;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.alignment = ParticleSystemRenderSpace.View;
        renderer.sharedMaterial = material;
        renderer.sortingLayerName = sortingLayer;
        renderer.sortingOrder = sortingOrder;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        return ps;
    }

    private static void SetBoxShape(ParticleSystem ps, float width, float height)
    {
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(width, height, 0f);
        shape.position = Vector3.zero;
    }

    private static void SetVelocity(ParticleSystem ps, float xMin, float xMax, float yMin, float yMax)
    {
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(xMin, xMax);
        velocity.y = new ParticleSystem.MinMaxCurve(yMin, yMax);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
    }

    private static void SetNoise(ParticleSystem ps, float strength, float frequency, float scrollSpeed)
    {
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = strength;
        noise.frequency = frequency;
        noise.scrollSpeed = scrollSpeed;
        noise.damping = true;
        noise.octaveCount = 1;
        noise.quality = ParticleSystemNoiseQuality.Low;
        noise.separateAxes = false;
    }

    /** 出現時にフェードイン、消える前にフェードアウトするアルファ。 */
    private static void SetAlphaOverLifetime(ParticleSystem ps, float peakAlpha, float fadeIn, float fadeOut, Color startTint, Color endTint)
    {
        var color = ps.colorOverLifetime;
        color.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(startTint, 0f),
                new GradientColorKey(endTint, 1f),
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(peakAlpha, fadeIn),
                new GradientAlphaKey(peakAlpha, 1f - fadeOut),
                new GradientAlphaKey(0f, 1f),
            });
        color.color = new ParticleSystem.MinMaxGradient(gradient);
    }

    private static void SetSprites(ParticleSystem ps, params Sprite[] sprites)
    {
        var sheet = ps.textureSheetAnimation;
        sheet.enabled = true;
        sheet.mode = ParticleSystemAnimationMode.Sprites;
        while (sheet.spriteCount > 0)
        {
            sheet.RemoveSprite(0);
        }
        foreach (var sprite in sprites)
        {
            if (sprite == null)
            {
                throw new InvalidOperationException("粒子スプライトが null です");
            }
            sheet.AddSprite(sprite);
        }
        sheet.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
        sheet.cycleCount = 1;
        // startFrame は 0〜1 の正規化値(× スプライト数)。粒ごとにランダムな絵柄にする
        sheet.startFrame = sprites.Length > 1
            ? new ParticleSystem.MinMaxCurve(0f, 0.999f)
            : new ParticleSystem.MinMaxCurve(0f);
    }

    private static Sprite LoadSprite(string path, string name)
    {
        var sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
            .FirstOrDefault(s => name == null || s.name == name);
        if (sprite == null)
        {
            throw new InvalidOperationException($"スプライトが見つかりません: {path} ({name})");
        }
        return sprite;
    }

    private static void SetFollowOffset(FollowMainCamera follow, Vector2 offset)
    {
        var so = new SerializedObject(follow);
        so.FindProperty("offset").vector2Value = offset;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject SaveParticlePrefab(GameObject go, string path)
    {
        try
        {
            return PrefabUtility.SaveAsPrefabAsset(go, path);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    /** (a) 漂う埃・花粉。カメラに追従する箱から湧き、ゆっくり漂う。 */
    private static GameObject BuildDust(Material material)
    {
        var go = new GameObject("Ambient_Dust");
        var ps = NewParticleSystem(go, material, EffectSortingLayer, 0, 6f, 10f, 60, 5f);
        SetBoxShape(ps, 20f, 12f);
        SetVelocity(ps, -0.15f, 0.15f, 0.05f, 0.25f);
        SetNoise(ps, 0.25f, 0.15f, 0.3f);
        SetAlphaOverLifetime(ps, 0.8f, 0.2f, 0.25f, Color.white, Color.white);
        SetSprites(ps, LoadSprite(DustSpritePath, null));
        go.AddComponent<FollowMainCamera>();
        return SaveParticlePrefab(go, DustPath);
    }

    /** (b) 火の粉。松明・焚き火の上に置く。上へ舞い上がって消える。 */
    private static GameObject BuildEmbers(Material material)
    {
        var go = new GameObject("Ambient_Embers");
        var ps = NewParticleSystem(go, material, EffectSortingLayer, 1, 1.2f, 2.5f, 20, 4f);
        SetBoxShape(ps, 0.4f, 0.2f);
        SetVelocity(ps, -0.3f, 0.3f, 0.8f, 1.6f);
        SetNoise(ps, 0.4f, 0.5f, 0.5f);
        SetAlphaOverLifetime(ps, 1f, 0.05f, 0.4f, new Color(1f, 0.85f, 0.45f), new Color(1f, 0.35f, 0.1f));
        SetSprites(ps, LoadSprite(EmberSpritePath, null));
        return SaveParticlePrefab(go, EmbersPath);
    }

    /** (c) 落ちる土粒・胞子。画面上端から降る。 */
    private static GameObject BuildFallingDebris(Material material)
    {
        var go = new GameObject("Ambient_FallingDebris");
        var ps = NewParticleSystem(go, material, EffectSortingLayer, 0, 3f, 5f, 30, 3f);
        SetBoxShape(ps, 20f, 0.5f);
        SetVelocity(ps, -0.1f, 0.1f, -0.9f, -0.5f);
        SetNoise(ps, 0.15f, 0.3f, 0.3f);
        SetAlphaOverLifetime(ps, 1f, 0.1f, 0.2f, Color.white, Color.white);
        SetSprites(ps, LoadSprite(DebrisSheetPath, "grain"), LoadSprite(DebrisSheetPath, "spore"));
        SetFollowOffset(go.AddComponent<FollowMainCamera>(), new Vector2(0f, 6.5f));
        return SaveParticlePrefab(go, DebrisPath);
    }

    /** (d) 横に流れる風筋と葉。画面左端から右へ流れる。 */
    private static GameObject BuildWindStreaks(Material material)
    {
        var go = new GameObject("Ambient_WindStreaks");
        var ps = NewParticleSystem(go, material, EffectSortingLayer, 2, 2.5f, 4f, 24, 2.5f);
        SetBoxShape(ps, 1f, 12f);
        SetVelocity(ps, 4f, 7f, -0.2f, 0.3f);
        SetNoise(ps, 0.6f, 0.3f, 0.6f);
        SetAlphaOverLifetime(ps, 0.9f, 0.15f, 0.3f, Color.white, Color.white);
        SetSprites(ps, LoadSprite(WindSheetPath, "streak"), LoadSprite(WindSheetPath, "leaf"));
        // セルが 16x8 なので横 0.5・縦 0.25 のクアッドにする
        var main = ps.main;
        main.startSize3D = true;
        main.startSizeX = new ParticleSystem.MinMaxCurve(PixelSpriteSize * 2f);
        main.startSizeY = new ParticleSystem.MinMaxCurve(PixelSpriteSize);
        main.startSizeZ = new ParticleSystem.MinMaxCurve(1f);
        // 風筋 7 割・葉 3 割
        var sheet = ps.textureSheetAnimation;
        sheet.startFrame = new ParticleSystem.MinMaxCurve(0f, 0.7f);
        SetFollowOffset(go.AddComponent<FollowMainCamera>(), new Vector2(-10f, 0f));
        return SaveParticlePrefab(go, WindPath);
    }

    /** (e) 霧。中景でゆっくり横に流れる大きな帯。アトラスは 1 列 4 段。 */
    private static GameObject BuildFog(Material material)
    {
        var go = new GameObject("Ambient_Fog");
        var ps = NewParticleSystem(go, material, FogSortingLayer, 50, 24f, 36f, 12, 0.35f);
        var main = ps.main;
        main.startSize3D = true;
        main.startSizeX = new ParticleSystem.MinMaxCurve(12f);
        main.startSizeY = new ParticleSystem.MinMaxCurve(3f);
        main.startSizeZ = new ParticleSystem.MinMaxCurve(1f);
        main.startColor = new Color(0.85f, 0.9f, 1f, 1f);
        SetBoxShape(ps, 26f, 8f);
        SetVelocity(ps, 0.15f, 0.4f, -0.02f, 0.02f);
        SetAlphaOverLifetime(ps, 0.22f, 0.25f, 0.25f, Color.white, Color.white);

        var sheet = ps.textureSheetAnimation;
        sheet.enabled = true;
        sheet.mode = ParticleSystemAnimationMode.Grid;
        sheet.numTilesX = 1;
        sheet.numTilesY = 4;
        sheet.animation = ParticleSystemAnimationType.WholeSheet;
        sheet.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
        sheet.startFrame = new ParticleSystem.MinMaxCurve(0f, 0.999f);
        sheet.cycleCount = 1;

        go.AddComponent<FollowMainCamera>();
        return SaveParticlePrefab(go, FogPath);
    }

    // ---------------------------------------------------------------- atmosphere

    /**
     * 2〜5 をまとめて置く親。ベースにはエリアライトを含めない(Variant がエリア別ライトを追加する)。
     * ライト無しの状態で AmbientSpriteLit を使うとスプライトが黒く描かれるので、置くときは Variant を使う。
     */
    private static GameObject BuildAtmosphere(GameObject dust, GameObject fog, GameObject debris, GameObject wind)
    {
        var root = new GameObject("AreaAtmosphere");
        try
        {
            var volumeGo = new GameObject("Volume");
            volumeGo.transform.SetParent(root.transform, false);
            volumeGo.layer = 0; // Core の Main Camera の Volume Mask は Default だけ
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 0f;
            volume.weight = 1f;
            volume.sharedProfile = null;

            AddNested(root.transform, dust, "Dust", true);
            AddNested(root.transform, fog, "Fog", false);
            AddNested(root.transform, debris, "FallingDebris", false);
            AddNested(root.transform, wind, "WindStreaks", false);

            return PrefabUtility.SaveAsPrefabAsset(root, AtmospherePath);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static GameObject AddNested(Transform parent, GameObject prefab, string name, bool active)
    {
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.name = name;
        instance.transform.localPosition = Vector3.zero;
        instance.SetActive(active);
        return instance;
    }

    private static void BuildAtmosphereVariants(GameObject basePrefab)
    {
        BuildAtmosphereVariant(basePrefab, "Eien", dust: true, fog: false, debris: false, wind: false);
        BuildAtmosphereVariant(basePrefab, "Tuti", dust: true, fog: false, debris: true, wind: false);
        BuildAtmosphereVariant(basePrefab, "Kaze", dust: true, fog: true, debris: false, wind: true);
    }

    private static void BuildAtmosphereVariant(GameObject basePrefab, string area, bool dust, bool fog, bool debris, bool wind)
    {
        var lightPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GlobalLightVariantPath(area));
        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath(area));
        if (lightPrefab == null || profile == null)
        {
            throw new InvalidOperationException($"エリア {area} のライトまたはプロファイルが未生成です");
        }

        BuildVariant(basePrefab, AtmosphereVariantPath(area), instance =>
        {
            var existingLight = instance.transform.Find("GlobalLight");
            if (existingLight != null)
            {
                UnityEngine.Object.DestroyImmediate(existingLight.gameObject);
            }
            var light = AddNested(instance.transform, lightPrefab, "GlobalLight", true);
            light.transform.SetAsFirstSibling();

            instance.transform.Find("Volume").GetComponent<Volume>().sharedProfile = profile;
            instance.transform.Find("Dust").gameObject.SetActive(dust);
            instance.transform.Find("Fog").gameObject.SetActive(fog);
            instance.transform.Find("FallingDebris").gameObject.SetActive(debris);
            instance.transform.Find("WindStreaks").gameObject.SetActive(wind);
        });
    }
}
