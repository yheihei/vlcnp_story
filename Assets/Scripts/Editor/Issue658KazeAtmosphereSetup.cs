using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using VLCNP.Core;
using VLCNP.Effects;

/**
 * #658 風エリア(Kaze1・Kaze2)に環境演出を適用するエディタ拡張。
 *
 * 各シーンに対して次を行う(再実行すると前回の生成物を消してから置き直す)。
 *   1. AreaAtmosphere_Kaze(月夜の青紫のグローバルライト・風の Volume・風筋と葉・霧)をルートに置く。
 *      風筋と霧は雲(OuterWallSkyCloudScroll)の流れと同じ右から左へ流し、霧は Effect 層で塔の手前を横切らせる。
 *      漂う埃は風の場面に合わないので消す
 *   2. 表にある窓(round_window_lattice_02 / round_window_cross_01 のインスタンス)に冷たい点光源を付ける
 *   3. 夜空の Stars に OuterWallSkyStarTwinkle(ゆるい明滅)を付ける
 *   4. 前景(蔦・蜘蛛の巣)を PlayerUpperObject 層に置く。足場の上には置かない
 *   5. タイルマップと小物のマテリアルを AmbientSpriteLit に差し替える(AmbientMaterialReplacer)
 * 窓の光と前景の座標はこのスクリプト内の表が正。手直しは表へ戻す。
 */
public static class Issue658KazeAtmosphereSetup
{
    private const string AtmosphereName = "AreaAtmosphere_Kaze";
    private const string WindowLightName = "Issue658WindowLight";
    private const string ForegroundPrefix = "Issue658Foreground";
    private const string OrnamentPrefabDir = "Assets/Game/MapObject/PagodaTilemap/Prefabs/WallOrnaments/";

    /**
     * BGTilemap が使っている独自マテリアル(Sprites/Default と同じ見た目)。これも差し替える。
     * 小物のプレハブ(窓・蔦・扉など)はマテリアルの参照が切れていて Sprites-Default で描かれているので、参照切れも差し替える
     */
    private const string BgTilemapMaterialPath = OrnamentPrefabDir + "Materials/torn_ofuda_01_32x32.mat";

    public static readonly string[] Scenes =
    {
        "Assets/Scenes/Kaze1.unity",
        "Assets/Scenes/Kaze2.unity",
    };

    // ---------------------------------------------------------------- 窓の点光源

    /** 窓のプレハブ(この末尾を持つプレハブのインスタンスを窓とみなす) */
    private static readonly string[] WindowPrefabSuffixes =
    {
        "round_window_lattice_02_32x32.prefab",
        "round_window_cross_01_32x32.prefab",
    };

    /** 闇エリアの窓の光(Window03A)の色を参考にした冷たい色 */
    private static readonly Color WindowLightColor = new Color(0.125f, 0.22f, 0.227f);
    private const float WindowLightIntensity = 2.5f;
    private const float WindowLightOuterRadius = 2.8f;
    private const float WindowLightInnerRadius = 0.4f;
    private const float WindowLightFalloff = 0.5f;
    /** 表の座標とインスタンス座標の許容差 */
    private const float WindowMatchTolerance = 0.35f;

    private struct WindowLightSpec { public float x, y; }

    /** 点光源を付ける窓(ワールド座標)。WindowGun(敵)が重なる窓は避ける */
    private static readonly Dictionary<string, WindowLightSpec[]> WindowLights = new Dictionary<string, WindowLightSpec[]>
    {
        {
            "Kaze1", new[]
            {
                new WindowLightSpec { x = -4.85f, y = 2.46f },   // 入口の部屋
                new WindowLightSpec { x = -2.4f, y = 2.46f },
                new WindowLightSpec { x = -5.14f, y = 21.3f },   // 外壁を登る区間
                new WindowLightSpec { x = -5.14f, y = 35.73f },
                new WindowLightSpec { x = 2.82f, y = 42.73f },   // 中腹の小部屋
                new WindowLightSpec { x = 6.88f, y = 74.69f },   // 上の大部屋
            }
        },
        {
            "Kaze2", new[]
            {
                new WindowLightSpec { x = 19.42f, y = 1.65f },   // 入口右の壁
                new WindowLightSpec { x = 19.42f, y = 6.55f },
                new WindowLightSpec { x = 21.82f, y = 43.0f },   // 中腹
                new WindowLightSpec { x = 23.79f, y = 43.0f },
                new WindowLightSpec { x = 88.44f, y = 113.63f }, // ボス部屋の扉の両脇
                new WindowLightSpec { x = 91.15f, y = 113.63f },
            }
        },
    };

    // ---------------------------------------------------------------- 前景

    private struct ForegroundSpec { public string prefab; public float x, y, scale; public bool flip; }

    /** 前景。壁面(BGTilemap)の前に置き、足場やジャンプ先の上には置かない */
    private static readonly Dictionary<string, ForegroundSpec[]> Foregrounds = new Dictionary<string, ForegroundSpec[]>
    {
        {
            "Kaze1", new[]
            {
                new ForegroundSpec { prefab = "ivy_overlay_01_32x96", x = -5.5f, y = 2.5f, scale = 1.4f, flip = false },  // 入口の部屋、扉の上
                new ForegroundSpec { prefab = "ivy_overlay_01_32x96", x = 11.5f, y = 72.6f, scale = 1.5f, flip = true },  // 上の大部屋、窓の間
                new ForegroundSpec { prefab = "spider_web_01_32x32", x = 2.0f, y = 85.7f, scale = 1.6f, flip = false },  // 上の大部屋、左上の隅
            }
        },
        {
            "Kaze2", new[]
            {
                new ForegroundSpec { prefab = "ivy_overlay_01_32x96", x = 26.0f, y = 10.6f, scale = 1.5f, flip = false }, // 入口右の壁、天井から
                new ForegroundSpec { prefab = "spider_web_01_32x32", x = 18.9f, y = 12.2f, scale = 1.6f, flip = false }, // 同じ壁の左上の隅
                new ForegroundSpec { prefab = "ivy_overlay_01_32x96", x = 31.2f, y = 3.6f, scale = 1.4f, flip = true },  // 同じ壁、窓の間
            }
        },
    };

    /** 月夜なので前景は青みの暗い色にする */
    private static readonly Color ForegroundTint = new Color(0.42f, 0.46f, 0.62f, 1f);

    // ---------------------------------------------------------------- 風・霧

    /** 雲は右から左へ流れる(OuterWallCloudScroll の UV オフセットが増えると絵が左へ動く)ので粒子も同じ向きにする */
    private const float WindVelocityMin = -7f;
    private const float WindVelocityMax = -4f;
    /** 画面右端の少し外から湧かせる(カメラの半幅 10.67) */
    private const float WindSpawnOffsetX = 11f;
    private const float WindRate = 3.5f;
    private const int WindMaxParticles = 36;
    /** 目立ちすぎないよう少し透ける(プレハブの寿命アルファ 0.9 に掛かる) */
    private const float WindAlpha = 0.65f;

    private const float FogVelocityMin = -0.45f;
    private const float FogVelocityMax = -0.15f;
    private static readonly Color FogColor = new Color(0.8f, 0.86f, 1f, 1f);
    private const string FogSortingLayer = "Effect";
    private const int FogSortingOrder = -5;

    // ---------------------------------------------------------------- 確認用のカメラ位置

    private struct CameraSpot { public float x, y; }

    /** プレイモードで Cinemachine を切ってカメラを置く位置(スクリーンショット用)。CameraConfineArea の範囲内 */
    private static readonly Dictionary<string, CameraSpot[]> CameraSpots = new Dictionary<string, CameraSpot[]>
    {
        {
            "Kaze1", new[]
            {
                new CameraSpot { x = 4.5f, y = 2f },    // 入口の部屋
                new CameraSpot { x = 4.5f, y = 26f },   // 外壁を登る区間
                new CameraSpot { x = 4.5f, y = 76f },   // 上の大部屋
                new CameraSpot { x = 47.3f, y = 90f },  // ボス部屋への入口
            }
        },
        {
            "Kaze2", new[]
            {
                new CameraSpot { x = 4.5f, y = 2f },    // 入口
                new CameraSpot { x = 20f, y = 6f },     // 入口右の壁
                new CameraSpot { x = 22f, y = 43f },    // 中腹
                new CameraSpot { x = 84.3f, y = 113f }, // ボス部屋の扉
            }
        },
    };

    // ---------------------------------------------------------------- menu

    [MenuItem("Tools/Issue658/Apply Kaze Atmosphere To All Scenes", false, 3460)]
    public static void ApplyToAllScenes()
    {
        if (HasDirtyScene())
        {
            Debug.LogError("[Issue658] 未保存のシーンがあるので中断(保存確認のモーダルを避けるため)。保存か破棄をしてから再実行する");
            return;
        }
        foreach (var path in Scenes)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            Apply(scene);
            EditorSceneManager.SaveScene(scene);
        }
        Debug.Log($"[Issue658] {Scenes.Length} シーンに風の環境演出を適用して保存した");
    }

    [MenuItem("Tools/Issue658/Apply Kaze Atmosphere To Open Scene", false, 3461)]
    public static void ApplyToOpenScene()
    {
        var scene = SceneManager.GetActiveScene();
        Apply(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"[Issue658] {scene.name} に適用した(未保存)");
    }

    [MenuItem("Tools/Issue658/Remove Kaze Atmosphere From Open Scene", false, 3462)]
    public static void RemoveFromOpenScene()
    {
        var scene = SceneManager.GetActiveScene();
        Remove(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"[Issue658] {scene.name} から環境演出を外した(未保存)");
    }

    [MenuItem("Tools/Issue658/Debug/Camera Spot 1", false, 3470)]
    public static void CameraSpot1() => MoveCameraToSpot(0);

    [MenuItem("Tools/Issue658/Debug/Camera Spot 2", false, 3471)]
    public static void CameraSpot2() => MoveCameraToSpot(1);

    [MenuItem("Tools/Issue658/Debug/Camera Spot 3", false, 3472)]
    public static void CameraSpot3() => MoveCameraToSpot(2);

    [MenuItem("Tools/Issue658/Debug/Camera Spot 4", false, 3473)]
    public static void CameraSpot4() => MoveCameraToSpot(3);

    [MenuItem("Tools/Issue658/Debug/Restore Camera Follow", false, 3474)]
    public static void RestoreCameraFollow()
    {
        var brain = Object.FindObjectOfType<Cinemachine.CinemachineBrain>(true);
        if (brain != null) brain.enabled = true;
    }

    /** プレイモード中に Cinemachine を切り、表の位置へメインカメラを置く(スクリーンショット確認用) */
    private static void MoveCameraToSpot(int index)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Issue658] Camera Spot はプレイモード中だけ使える");
            return;
        }
        var scene = SceneManager.GetActiveScene();
        if (!CameraSpots.TryGetValue(scene.name, out var spots) || index >= spots.Length)
        {
            Debug.LogWarning($"[Issue658] {scene.name} にカメラ位置 {index + 1} は無い");
            return;
        }
        var brain = Object.FindObjectOfType<Cinemachine.CinemachineBrain>(true);
        if (brain != null) brain.enabled = false;
        var camera = Camera.main;
        if (camera == null)
        {
            Debug.LogWarning("[Issue658] Main Camera が無い");
            return;
        }
        var position = camera.transform.position;
        camera.transform.position = new Vector3(spots[index].x, spots[index].y, position.z);
        Debug.Log($"[Issue658] camera -> ({spots[index].x}, {spots[index].y})");
    }

    private static bool HasDirtyScene()
    {
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).isDirty) return true;
        }
        return false;
    }

    // ---------------------------------------------------------------- apply

    public static void Apply(Scene scene)
    {
        Remove(scene);

        var atmospherePrefab = Load<GameObject>(AmbientAtmosphereBuilder.AtmosphereVariantPath("Kaze"));
        var pointLightPrefab = Load<GameObject>(AmbientAtmosphereBuilder.PointLightPath);

        // 1. エリアの空気
        var atmosphere = (GameObject)PrefabUtility.InstantiatePrefab(atmospherePrefab, scene);
        atmosphere.name = AtmosphereName;
        atmosphere.transform.SetAsLastSibling();
        atmosphere.transform.Find("Dust").gameObject.SetActive(false);
        ConfigureWind(atmosphere.transform.Find("WindStreaks").gameObject);
        ConfigureFog(atmosphere.transform.Find("Fog").gameObject);

        // 2. 窓の点光源
        var lights = 0;
        if (WindowLights.TryGetValue(scene.name, out var lightSpecs))
        {
            var windows = CollectWindows(scene);
            foreach (var spec in lightSpecs)
            {
                var window = windows
                    .Where(w => Vector2.Distance(new Vector2(w.position.x, w.position.y), new Vector2(spec.x, spec.y)) <= WindowMatchTolerance)
                    .OrderBy(w => Vector2.Distance(new Vector2(w.position.x, w.position.y), new Vector2(spec.x, spec.y)))
                    .FirstOrDefault();
                if (window == null)
                {
                    Debug.LogWarning($"[Issue658] {scene.name}: 窓が見つからない ({spec.x}, {spec.y})");
                    continue;
                }
                var light = (GameObject)PrefabUtility.InstantiatePrefab(pointLightPrefab, window);
                light.name = WindowLightName;
                light.transform.localPosition = Vector3.zero;
                ConfigureWindowLight(light);
                lights++;
            }
        }

        // 3. 星の明滅
        var stars = FindStars(scene);
        if (stars == null)
        {
            Debug.LogWarning($"[Issue658] {scene.name}: OuterWallSky/Stars が見つからない");
        }
        else if (stars.GetComponent<OuterWallSkyStarTwinkle>() == null)
        {
            stars.AddComponent<OuterWallSkyStarTwinkle>();
            EditorUtility.SetDirty(stars);
        }

        // 4. 前景
        var foregrounds = 0;
        if (Foregrounds.TryGetValue(scene.name, out var foregroundSpecs))
        {
            for (var i = 0; i < foregroundSpecs.Length; i++)
            {
                PlaceForeground(scene, foregroundSpecs[i], $"{ForegroundPrefix} ({i})");
                foregrounds++;
            }
        }

        // 5. マテリアル差し替え(グローバルライトを置いた後に行う)
        var bgMaterial = Load<Material>(BgTilemapMaterialPath);
        var replaced = AmbientMaterialReplacer.Replace(scene, requireGlobalLight: true, new[] { bgMaterial }, replaceMissing: true);

        Debug.Log($"[Issue658] {scene.name}: lights={lights} stars={(stars != null)} foregrounds={foregrounds} replaced={replaced}");
    }

    /** 風筋と葉。雲と同じ右から左へ流し、画面右端の外から湧かせる */
    private static void ConfigureWind(GameObject wind)
    {
        var ps = wind.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.maxParticles = WindMaxParticles;
        main.startColor = new Color(1f, 1f, 1f, WindAlpha);

        var emission = ps.emission;
        emission.rateOverTime = WindRate;

        var velocity = ps.velocityOverLifetime;
        velocity.x = new ParticleSystem.MinMaxCurve(WindVelocityMin, WindVelocityMax);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        SetFollowOffset(wind.GetComponent<FollowMainCamera>(), new Vector2(WindSpawnOffsetX, 0f));
    }

    /** 霧(雲のかけら)。中景で塔の手前を右から左へ横切らせる */
    private static void ConfigureFog(GameObject fog)
    {
        var ps = fog.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = FogColor;

        var velocity = ps.velocityOverLifetime;
        velocity.x = new ParticleSystem.MinMaxCurve(FogVelocityMin, FogVelocityMax);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        var renderer = fog.GetComponent<ParticleSystemRenderer>();
        renderer.sortingLayerName = FogSortingLayer;
        renderer.sortingOrder = FogSortingOrder;
    }

    private static void ConfigureWindowLight(GameObject light)
    {
        var light2D = light.GetComponent<Light2D>();
        light2D.color = WindowLightColor;
        light2D.intensity = WindowLightIntensity;
        light2D.falloffIntensity = WindowLightFalloff;
        var so = new SerializedObject(light2D);
        so.FindProperty("m_PointLightInnerRadius").floatValue = WindowLightInnerRadius;
        so.FindProperty("m_PointLightOuterRadius").floatValue = WindowLightOuterRadius;
        so.ApplyModifiedPropertiesWithoutUndo();

        // 月明かりの窓はゆらがない
        var flicker = light.GetComponent<LightFlicker2D>();
        if (flicker != null) flicker.enabled = false;
    }

    private static void SetFollowOffset(FollowMainCamera follow, Vector2 offset)
    {
        if (follow == null) return;
        var so = new SerializedObject(follow);
        so.FindProperty("offset").vector2Value = offset;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    /** シーン内の窓プレハブのインスタンスルート */
    private static List<Transform> CollectWindows(Scene scene)
    {
        var result = new List<Transform>();
        foreach (var t in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)))
        {
            if (!PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject)) continue;
            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.gameObject);
            if (string.IsNullOrEmpty(path)) continue;
            if (WindowPrefabSuffixes.Any(path.EndsWith)) result.Add(t);
        }
        return result;
    }

    /** 夜空の Stars(OuterWallSky の子) */
    private static GameObject FindStars(Scene scene)
    {
        foreach (var t in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)))
        {
            if (t.name == "Stars" && t.parent != null && t.parent.name == "OuterWallSky" && t.GetComponent<SpriteRenderer>() != null)
            {
                return t.gameObject;
            }
        }
        return null;
    }

    private static void PlaceForeground(Scene scene, ForegroundSpec spec, string name)
    {
        var prefab = Load<GameObject>(OrnamentPrefabDir + spec.prefab + ".prefab");
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        go.name = name;
        go.transform.position = new Vector3(spec.x, spec.y, 0f);
        go.transform.localScale = new Vector3(spec.flip ? -spec.scale : spec.scale, spec.scale, 1f);
        foreach (var renderer in go.GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.sortingLayerName = "PlayerUpperObject";
            renderer.sortingOrder = 0;
            renderer.color = ForegroundTint;
        }
    }

    // ---------------------------------------------------------------- remove

    /**
     * 生成物を消し、星の明滅を外し、マテリアルを戻す。
     * BGTilemap は元の独自マテリアルに、それ以外は Sprites-Default に戻す(参照切れだった小物も Sprites-Default になるが見た目は同じ)
     */
    public static void Remove(Scene scene)
    {
        var roots = scene.GetRootGameObjects();
        var toDestroy = new List<GameObject>();
        foreach (var root in roots)
        {
            if (root.name == AtmosphereName || root.name.StartsWith(ForegroundPrefix))
            {
                toDestroy.Add(root);
                continue;
            }
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == WindowLightName)
                {
                    toDestroy.Add(t.gameObject);
                }
            }
        }
        foreach (var go in toDestroy)
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        // ルートを消した後なので取り直す
        foreach (var twinkle in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<OuterWallSkyStarTwinkle>(true)).ToArray())
        {
            var go = twinkle.gameObject;
            Object.DestroyImmediate(twinkle);
            EditorUtility.SetDirty(go);
        }

        var lit = AssetDatabase.LoadAssetAtPath<Material>(AmbientAtmosphereBuilder.LitMaterialPath);
        var builtin = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        var bgMaterial = AssetDatabase.LoadAssetAtPath<Material>(BgTilemapMaterialPath);
        if (lit == null) return;
        foreach (var renderer in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Renderer>(true)))
        {
            var materials = renderer.sharedMaterials;
            var changed = false;
            var restore = renderer is UnityEngine.Tilemaps.TilemapRenderer && renderer.name == "BGTilemap" && bgMaterial != null
                ? bgMaterial
                : builtin;
            for (var i = 0; i < materials.Length; i++)
            {
                if (materials[i] == lit)
                {
                    materials[i] = restore;
                    changed = true;
                }
            }
            if (changed)
            {
                renderer.sharedMaterials = materials;
                EditorUtility.SetDirty(renderer);
            }
        }
    }

    private static T Load<T>(string path) where T : Object
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            throw new System.InvalidOperationException($"アセットが見つかりません: {path}");
        }
        return asset;
    }
}
