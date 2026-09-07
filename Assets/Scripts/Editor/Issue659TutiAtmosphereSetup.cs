using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using VLCNP.Effects;

/**
 * #659 土エリア(Ohirunebeya_tuti_1〜5、tuti_6_boss_1〜3)に環境演出を適用するエディタ拡張。
 *
 * 各シーンに対して次を行う(再実行すると前回の生成物を消してから置き直す)。
 *   1. 既存の白いグローバルライト(ルートの "Light 2D")を消し、AreaAtmosphere_Tuti(土色のグローバルライト・土の Volume・埃・落ちる土粒と胞子)をルートに置く。
 *      埃と土粒の発生範囲はシーンのカメラ ortho に合わせて広げ、土粒は画面上端(天井付近)から降らせる
 *   2. Cainos の Torch と Campfire のインスタンスに点光源(ゆらぎ付き)を付ける
 *   3. Core の CMCamera 配下にある既存の埃(DangeonParticle・DangeonParticle_1)を無効にする(#656 の埃に置き換える)
 *   4. tuti_2 の水面(WaterSarface のインスタンス)に WaterSurfaceSway(上下 1px と明るさの周期変化)を付ける。当たり判定は触らない
 *   5. builtin Sprites-Default の背景と小物のマテリアルを AmbientSpriteLit に差し替える(AmbientMaterialReplacer の既定)
 *   6. 霧(2026-09-07 追加、#657 の永遠エリアより多め)。AreaAtmosphere_Tuti の Fog(カメラ追従の中景の霧)を有効にして画面幅に合わせ、
 *      さらに Tilemap(tag Ground)の床面ごとに低い靄の帯(TutiMist)を床に固定して置く
 * ボス戦とイベントのシーン(tuti_6_boss_1〜3)はライト(グローバル・松明の点光源)と Volume だけにし、粒子と既存の埃は触らない。
 * 確認用のカメラ位置はこのスクリプト内の表が正。手直しは表へ戻す。
 */
public static class Issue659TutiAtmosphereSetup
{
    private const string AtmosphereName = "AreaAtmosphere_Tuti";
    private const string PointLightName = "Issue659PointLight";
    private const string LegacyGlobalLightName = "Light 2D";
    private const string LegacyDustPrefix = "DangeonParticle";
    private const string WaterSurfacePrefabSuffix = "/WaterSarface.prefab";
    private const string MistPrefix = "TutiMist";

    public static readonly string[] Scenes =
    {
        "Assets/Scenes/Ohirunebeya_tuti_1.unity",
        "Assets/Scenes/Ohirunebeya_tuti_2.unity",
        "Assets/Scenes/Ohirunebeya_tuti_3.unity",
        "Assets/Scenes/Ohirunebeya_tuti_4.unity",
        "Assets/Scenes/Ohirunebeya_tuti_5.unity",
        "Assets/Scenes/Ohirunebeya_tuti_6_boss_1.unity",
        "Assets/Scenes/Ohirunebeya_tuti_6_boss_2.unity",
        "Assets/Scenes/Ohirunebeya_tuti_6_boss_3.unity",
    };

    /** ボス戦とイベントの部屋。ライトと Volume だけ置く */
    private static readonly HashSet<string> EventScenes = new HashSet<string>
    {
        "Ohirunebeya_tuti_6_boss_1",
        "Ohirunebeya_tuti_6_boss_2",
        "Ohirunebeya_tuti_6_boss_3",
    };

    /** 各シーンのカメラ ortho(シーンの CMCamera と合わせる)。粒子の発生範囲の計算に使う */
    private static readonly Dictionary<string, float> OrthoSizes = new Dictionary<string, float>
    {
        { "Ohirunebeya_tuti_1", 6f },
        { "Ohirunebeya_tuti_2", 6f },  // 2026-09-07 に 5 から 6 へ(tuti_1 と同じ見え方にするため)
        { "Ohirunebeya_tuti_3", 7.5f },
        { "Ohirunebeya_tuti_4", 7.6f },
        { "Ohirunebeya_tuti_5", 5f },
        { "Ohirunebeya_tuti_6_boss_1", 6f },
        { "Ohirunebeya_tuti_6_boss_2", 6f },
        { "Ohirunebeya_tuti_6_boss_3", 6f },
    };

    private const float ViewAspect = 16f / 9f;

    // ---------------------------------------------------------------- 松明・焚き火

    /** 炎の位置(プレハブのローカル座標)。#657 と同じ値 */
    private struct FlameSpec { public string suffix; public Vector3 offset; }
    private static readonly FlameSpec[] Flames =
    {
        new FlameSpec { suffix = "Torch.prefab", offset = new Vector3(0f, 0.63f, 0f) },
        new FlameSpec { suffix = "Campfire 01.prefab", offset = new Vector3(-0.07f, 1.0f, 0f) },
    };

    // ---------------------------------------------------------------- 粒子

    /** 落ちる土粒・胞子は画面上端の少し外(天井付近)から降らせる */
    private const float DebrisSpawnMarginY = 1.0f;
    private const float DebrisSpawnHeight = 0.5f;
    /** 発生範囲の横幅は画面幅より少し広くする(カメラが動いても途切れないように) */
    private const float SpawnMarginX = 4f;

    // ---------------------------------------------------------------- 霧

    /**
     * 低い靄。Tilemap(tag Ground)の床面(上が空いているタイルの連続区間)をすべて拾い、各区間に帯を置く(#657 と同じ拾い方)。
     * 永遠エリアと違い高さ制限は付けず、足場の上にも置く(霧を多めにするため)。minCells 未満の短い足場は飛ばし、
     * 小さな穴(mergeGapCells 以下)はまたいで 1 本の帯にする
     */
    private struct MistArea { public float maxFloorY; public int minCells; public int mergeGapCells; }
    private static readonly Dictionary<string, MistArea> MistAreas = new Dictionary<string, MistArea>
    {
        { "Ohirunebeya_tuti_1", new MistArea { maxFloorY = float.MaxValue, minCells = 4, mergeGapCells = 6 } },
        { "Ohirunebeya_tuti_2", new MistArea { maxFloorY = float.MaxValue, minCells = 4, mergeGapCells = 6 } },
        { "Ohirunebeya_tuti_3", new MistArea { maxFloorY = float.MaxValue, minCells = 4, mergeGapCells = 6 } },
        { "Ohirunebeya_tuti_4", new MistArea { maxFloorY = float.MaxValue, minCells = 4, mergeGapCells = 6 } },
        { "Ohirunebeya_tuti_5", new MistArea { maxFloorY = float.MaxValue, minCells = 4, mergeGapCells = 6 } },
    };

    private struct MistSpec { public float x, y, width; }

    /** 靄の帯の濃さ(#657 の永遠は 0.2) */
    private const float MistAlpha = 0.3f;
    /** カメラ追従の中景の霧の濃さ(プレハブ既定は 0.22) */
    private const float FogAlpha = 0.2f;

    // ---------------------------------------------------------------- 確認用のカメラ位置

    private struct CameraSpot { public float x, y; }

    /** プレイモードで Cinemachine を切ってカメラを置く位置(スクリーンショット用)。1 番目はスポーン地点のカメラ位置 */
    private static readonly Dictionary<string, CameraSpot[]> CameraSpots = new Dictionary<string, CameraSpot[]>
    {
        {
            "Ohirunebeya_tuti_1", new[]
            {
                new CameraSpot { x = 0f, y = 1f },      // スポーン地点
                new CameraSpot { x = 48f, y = 0f },     // 中盤
                new CameraSpot { x = 110f, y = -6f },   // 終盤
            }
        },
        {
            "Ohirunebeya_tuti_2", new[]
            {
                new CameraSpot { x = 9.3f, y = -2.58f }, // スポーン地点
                new CameraSpot { x = 30f, y = -3f },     // 水面
                new CameraSpot { x = 75f, y = -3f },     // 後半の水面
            }
        },
        {
            "Ohirunebeya_tuti_3", new[]
            {
                new CameraSpot { x = 12.07f, y = 1.02f }, // スポーン地点(固定カメラ)
                new CameraSpot { x = 12.5f, y = 11f },    // 上の部屋
            }
        },
        {
            "Ohirunebeya_tuti_4", new[]
            {
                new CameraSpot { x = 13.46f, y = 0.77f }, // スポーン地点(固定カメラ)
                new CameraSpot { x = 12.5f, y = 11f },    // 上の部屋
            }
        },
        {
            "Ohirunebeya_tuti_5", new[]
            {
                new CameraSpot { x = 9.43f, y = 8.77f },  // スポーン地点(焚き火・松明)
                new CameraSpot { x = 12.5f, y = 13.5f },  // 部屋の上側(y 13 より上は何も無い)
            }
        },
        { "Ohirunebeya_tuti_6_boss_1", new[] { new CameraSpot { x = 9.59f, y = 5.6f } } },
        { "Ohirunebeya_tuti_6_boss_2", new[] { new CameraSpot { x = 9.5f, y = 8.2f } } },
        { "Ohirunebeya_tuti_6_boss_3", new[] { new CameraSpot { x = 9.59f, y = 5.6f } } },
    };

    // ---------------------------------------------------------------- menu

    [MenuItem("Tools/Issue659/Apply Tuti Atmosphere To All Scenes", false, 3480)]
    public static void ApplyToAllScenes()
    {
        if (HasDirtyScene())
        {
            Debug.LogError("[Issue659] 未保存のシーンがあるので中断(保存確認のモーダルを避けるため)。保存か破棄をしてから再実行する");
            return;
        }
        foreach (var path in Scenes)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            Apply(scene);
            EditorSceneManager.SaveScene(scene);
        }
        Debug.Log($"[Issue659] {Scenes.Length} シーンに土の環境演出を適用して保存した");
    }

    [MenuItem("Tools/Issue659/Apply Tuti Atmosphere To Open Scene", false, 3481)]
    public static void ApplyToOpenScene()
    {
        var scene = SceneManager.GetActiveScene();
        Apply(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"[Issue659] {scene.name} に適用した(未保存)");
    }

    [MenuItem("Tools/Issue659/Remove Tuti Atmosphere From Open Scene", false, 3482)]
    public static void RemoveFromOpenScene()
    {
        var scene = SceneManager.GetActiveScene();
        Remove(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"[Issue659] {scene.name} から環境演出を外した(未保存)");
    }

    [MenuItem("Tools/Issue659/Debug/Camera Spot 1", false, 3490)]
    public static void CameraSpot1() => MoveCameraToSpot(0);

    [MenuItem("Tools/Issue659/Debug/Camera Spot 2", false, 3491)]
    public static void CameraSpot2() => MoveCameraToSpot(1);

    [MenuItem("Tools/Issue659/Debug/Camera Spot 3", false, 3492)]
    public static void CameraSpot3() => MoveCameraToSpot(2);

    [MenuItem("Tools/Issue659/Debug/Restore Camera Follow", false, 3493)]
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
            Debug.LogWarning("[Issue659] Camera Spot はプレイモード中だけ使える");
            return;
        }
        var scene = SceneManager.GetActiveScene();
        if (!CameraSpots.TryGetValue(scene.name, out var spots) || index >= spots.Length)
        {
            Debug.LogWarning($"[Issue659] {scene.name} にカメラ位置 {index + 1} は無い");
            return;
        }
        var brain = Object.FindObjectOfType<Cinemachine.CinemachineBrain>(true);
        if (brain != null) brain.enabled = false;
        var camera = Camera.main;
        if (camera == null)
        {
            Debug.LogWarning("[Issue659] Main Camera が無い");
            return;
        }
        var position = camera.transform.position;
        camera.transform.position = new Vector3(spots[index].x, spots[index].y, position.z);
        Debug.Log($"[Issue659] camera -> ({spots[index].x}, {spots[index].y})");
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

        var isEvent = EventScenes.Contains(scene.name);
        var ortho = OrthoSizes.TryGetValue(scene.name, out var o) ? o : 6f;
        var atmospherePrefab = Load<GameObject>(AmbientAtmosphereBuilder.AtmosphereVariantPath("Tuti"));
        var pointLightPrefab = Load<GameObject>(AmbientAtmosphereBuilder.PointLightPath);

        // 1. 既存の白いグローバルライトを消し、エリアの空気を置く
        var removedLights = 0;
        foreach (var light in CollectLegacyGlobalLights(scene))
        {
            Object.DestroyImmediate(light.gameObject);
            removedLights++;
        }

        var atmosphere = (GameObject)PrefabUtility.InstantiatePrefab(atmospherePrefab, scene);
        atmosphere.name = AtmosphereName;
        atmosphere.transform.SetAsLastSibling();
        var dust = atmosphere.transform.Find("Dust").gameObject;
        var debris = atmosphere.transform.Find("FallingDebris").gameObject;
        if (isEvent)
        {
            dust.SetActive(false);
            debris.SetActive(false);
        }
        else
        {
            ConfigureDust(dust, ortho);
            ConfigureDebris(debris, ortho);
            ConfigureFog(atmosphere.transform.Find("Fog").gameObject, ortho);
        }

        // 2. 松明・焚き火の点光源
        var lights = 0;
        foreach (var t in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray())
        {
            if (!PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject)) continue;
            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.gameObject);
            if (string.IsNullOrEmpty(path)) continue;
            foreach (var flame in Flames)
            {
                if (!path.EndsWith(flame.suffix)) continue;
                var light = (GameObject)PrefabUtility.InstantiatePrefab(pointLightPrefab, t);
                light.name = PointLightName;
                light.transform.localPosition = flame.offset;
                lights++;
            }
        }

        // 3. 既存の埃を無効にする(ボス戦は触らない)
        var legacyDusts = 0;
        if (!isEvent)
        {
            foreach (var go in CollectLegacyDusts(scene))
            {
                if (!go.activeSelf) continue;
                go.SetActive(false);
                EditorUtility.SetDirty(go);
                legacyDusts++;
            }
        }

        // 4. 水面の揺らぎ
        var waters = 0;
        if (!isEvent)
        {
            foreach (var water in CollectWaterSurfaces(scene))
            {
                if (water.GetComponent<WaterSurfaceSway>() != null) continue;
                water.AddComponent<WaterSurfaceSway>();
                EditorUtility.SetDirty(water);
                waters++;
            }
        }

        // 5. マテリアル差し替え(グローバルライトを置いた後に行う)
        var replaced = AmbientMaterialReplacer.Replace(scene, requireGlobalLight: true);

        // 6. 床に固定する低い靄
        var mists = 0;
        if (!isEvent && MistAreas.TryGetValue(scene.name, out var mistArea))
        {
            var fogPrefab = Load<GameObject>(AmbientAtmosphereBuilder.FogPath);
            var mistSpecs = CollectMistSpecs(scene, mistArea);
            for (var i = 0; i < mistSpecs.Count; i++)
            {
                PlaceMist(scene, fogPrefab, mistSpecs[i], $"{MistPrefix} ({i})");
                mists++;
            }
        }

        Debug.Log($"[Issue659] {scene.name}: removedGlobalLights={removedLights} lights={lights} legacyDusts={legacyDusts} waters={waters} replaced={replaced} mists={mists} event={isEvent}");
    }

    /** カメラ追従の中景の霧。画面全体を覆うよう発生範囲を ortho に合わせ、有効にする */
    private static void ConfigureFog(GameObject fog, float ortho)
    {
        fog.SetActive(true);
        var ps = fog.GetComponent<ParticleSystem>();
        var shape = ps.shape;
        var scale = shape.scale;
        var width = Mathf.Max(scale.x, ortho * 2f * ViewAspect + SpawnMarginX);
        var height = Mathf.Max(scale.y, ortho * 2f);
        shape.scale = new Vector3(width, height, 0f);
        var main = ps.main;
        main.maxParticles = Mathf.Max(main.maxParticles, Mathf.CeilToInt(width * 0.6f));
        var emission = ps.emission;
        emission.rateOverTime = Mathf.Max(emission.rateOverTime.constant, width / 60f);
        SetAlphaOverLifetime(ps, FogAlpha);
    }

    private static List<MistSpec> CollectMistSpecs(Scene scene, MistArea area)
    {
        var specs = new List<MistSpec>();
        foreach (var tilemap in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>(true)))
        {
            if (!tilemap.CompareTag("Ground")) continue;
            var bounds = tilemap.cellBounds;
            for (var y = bounds.yMin; y < bounds.yMax; y++)
            {
                var runs = new List<(int start, int end)>();
                var start = int.MinValue;
                for (var x = bounds.xMin; x <= bounds.xMax; x++)
                {
                    var floor = x < bounds.xMax
                        && tilemap.HasTile(new Vector3Int(x, y, 0))
                        && !tilemap.HasTile(new Vector3Int(x, y + 1, 0));
                    if (floor && start == int.MinValue) start = x;
                    if (!floor && start != int.MinValue)
                    {
                        if (x - start >= area.minCells) runs.Add((start, x));
                        start = int.MinValue;
                    }
                }

                // 小さな穴はまたぐ
                var merged = new List<(int start, int end)>();
                foreach (var run in runs)
                {
                    if (merged.Count > 0 && run.start - merged[merged.Count - 1].end <= area.mergeGapCells)
                    {
                        merged[merged.Count - 1] = (merged[merged.Count - 1].start, run.end);
                    }
                    else
                    {
                        merged.Add(run);
                    }
                }

                foreach (var run in merged)
                {
                    var left = tilemap.CellToWorld(new Vector3Int(run.start, y + 1, 0));
                    var right = tilemap.CellToWorld(new Vector3Int(run.end, y + 1, 0));
                    if (left.y > area.maxFloorY) continue;
                    specs.Add(new MistSpec
                    {
                        x = (left.x + right.x) * 0.5f,
                        y = left.y + 1.0f,
                        width = right.x - left.x,
                    });
                }
            }
        }
        return specs;
    }

    /** 床に固定した低い靄の帯。#657 の PlaceMist より粒を多く・濃くしてある */
    private static void PlaceMist(Scene scene, GameObject fogPrefab, MistSpec spec, string name)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(fogPrefab, scene);
        go.name = name;
        go.transform.position = new Vector3(spec.x, spec.y, 0f);

        // カメラ追従を切り、床に固定する
        var follow = go.GetComponent<FollowMainCamera>();
        if (follow != null)
        {
            follow.enabled = false;
        }

        var ps = go.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(14f, 22f);
        main.startSizeX = new ParticleSystem.MinMaxCurve(10f, 14f);
        main.startSizeY = new ParticleSystem.MinMaxCurve(3.5f);
        // 幅に応じて粒数を決める(寿命 14〜22 秒 × 発生率 width/25)
        main.maxParticles = Mathf.Clamp(Mathf.CeilToInt(spec.width * 0.8f) + 3, 6, 30);
        main.startColor = new Color(0.9f, 0.88f, 0.82f, 1f);

        var emission = ps.emission;
        emission.rateOverTime = Mathf.Max(0.25f, spec.width / 25f);

        var shape = ps.shape;
        shape.scale = new Vector3(Mathf.Max(spec.width - 4f, 2f), 1.5f, 0f);

        var velocity = ps.velocityOverLifetime;
        velocity.x = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.01f, 0.01f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        SetAlphaOverLifetime(ps, MistAlpha);

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sortingLayerName = "Effect";
        renderer.sortingOrder = -5;
    }

    /** 寿命の 25〜75% を alpha で保ち、両端はゼロへ */
    private static void SetAlphaOverLifetime(ParticleSystem ps, float alpha)
    {
        var color = ps.colorOverLifetime;
        color.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(alpha, 0.25f),
                new GradientAlphaKey(alpha, 0.75f),
                new GradientAlphaKey(0f, 1f),
            });
        color.color = new ParticleSystem.MinMaxGradient(gradient);
    }

    /** 漂う埃。画面全体を覆うよう発生範囲を ortho に合わせる(プレハブの既定 20x12 より小さくはしない) */
    private static void ConfigureDust(GameObject dust, float ortho)
    {
        var ps = dust.GetComponent<ParticleSystem>();
        var shape = ps.shape;
        var scale = shape.scale;
        var width = Mathf.Max(scale.x, ortho * 2f * ViewAspect + 2f);
        var height = Mathf.Max(scale.y, ortho * 2f + 2f);
        shape.scale = new Vector3(width, height, 0f);
    }

    /** 落ちる土粒・胞子。画面上端の少し外(天井付近)から、画面幅より少し広い範囲で降らせる */
    private static void ConfigureDebris(GameObject debris, float ortho)
    {
        var ps = debris.GetComponent<ParticleSystem>();
        var shape = ps.shape;
        shape.scale = new Vector3(ortho * 2f * ViewAspect + SpawnMarginX, DebrisSpawnHeight, 0f);
        SetFollowOffset(debris.GetComponent<FollowMainCamera>(), new Vector2(0f, ortho + DebrisSpawnMarginY));
    }

    private static void SetFollowOffset(FollowMainCamera follow, Vector2 offset)
    {
        if (follow == null) return;
        var so = new SerializedObject(follow);
        so.FindProperty("offset").vector2Value = offset;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    /** AreaAtmosphere 以外にあるグローバルライト(シーン直置きの "Light 2D") */
    private static List<Light2D> CollectLegacyGlobalLights(Scene scene)
    {
        return scene.GetRootGameObjects()
            .Where(r => r.name != AtmosphereName)
            .SelectMany(r => r.GetComponentsInChildren<Light2D>(true))
            .Where(l => l.lightType == Light2D.LightType.Global)
            .ToList();
    }

    /** Core の CMCamera 配下にある既存の埃(DangeonParticle・DangeonParticle_1) */
    private static List<GameObject> CollectLegacyDusts(Scene scene)
    {
        return scene.GetRootGameObjects()
            .SelectMany(r => r.GetComponentsInChildren<Transform>(true))
            .Where(t => t.name.StartsWith(LegacyDustPrefix) && t.GetComponent<ParticleSystem>() != null)
            .Select(t => t.gameObject)
            .ToList();
    }

    /** 水面プレハブのインスタンスルート */
    private static List<GameObject> CollectWaterSurfaces(Scene scene)
    {
        var result = new List<GameObject>();
        foreach (var t in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)))
        {
            if (!PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject)) continue;
            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.gameObject);
            if (!string.IsNullOrEmpty(path) && path.EndsWith(WaterSurfacePrefabSuffix)) result.Add(t.gameObject);
        }
        return result;
    }

    // ---------------------------------------------------------------- remove

    /**
     * 生成物を消し、既存の埃を有効に戻し、水面の揺らぎを外し、マテリアルを Sprites-Default に戻す。
     * グローバルライトが無くなる場合は、元と同じ白いグローバルライト "Light 2D" をルートに置き直す
     */
    public static void Remove(Scene scene)
    {
        var roots = scene.GetRootGameObjects();
        var toDestroy = new List<GameObject>();
        foreach (var root in roots)
        {
            if (root.name == AtmosphereName || root.name.StartsWith(MistPrefix))
            {
                toDestroy.Add(root);
                continue;
            }
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == PointLightName) toDestroy.Add(t.gameObject);
            }
        }
        foreach (var go in toDestroy)
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        // ルートを消した後なので取り直す
        foreach (var go in CollectLegacyDusts(scene))
        {
            if (go.activeSelf) continue;
            go.SetActive(true);
            EditorUtility.SetDirty(go);
        }
        foreach (var sway in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<WaterSurfaceSway>(true)).ToArray())
        {
            var go = sway.gameObject;
            Object.DestroyImmediate(sway);
            EditorUtility.SetDirty(go);
        }

        if (CollectLegacyGlobalLights(scene).Count == 0)
        {
            var lightGo = new GameObject(LegacyGlobalLightName);
            SceneManager.MoveGameObjectToScene(lightGo, scene);
            var light = lightGo.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.color = Color.white;
            light.intensity = 1f;
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

        var lit = AssetDatabase.LoadAssetAtPath<Material>(AmbientAtmosphereBuilder.LitMaterialPath);
        var builtin = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        if (lit == null) return;
        foreach (var renderer in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Renderer>(true)))
        {
            var materials = renderer.sharedMaterials;
            var changed = false;
            for (var i = 0; i < materials.Length; i++)
            {
                if (materials[i] == lit)
                {
                    materials[i] = builtin;
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
