using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VLCNP.Effects;

/**
 * #657 永遠エリア(Ohirunebeya_start〜5_2)に環境演出を適用するエディタ拡張。
 *
 * 各シーンに対して次を行う(再実行すると前回の生成物を消してから置き直す)。
 *   1. AreaAtmosphere_Eien(琥珀のグローバルライト・永遠の Volume・埃)をルートに置く
 *   2. 松明・焚き火・街灯(Cainos Village Props のインスタンス)に点光源を付ける。松明・焚き火には火の粉も付ける
 *   3. Ohirunebeya_2 と 4 の床付近に低い靄、前景の草を置く
 *   4. タイルマップと小物のマテリアルを AmbientSpriteLit に差し替える(AmbientMaterialReplacer)
 * イベントの部屋(3_leelee・5_boss)はライトと Volume だけにし、粒子は置かない。
 */
public static class Issue657EienAtmosphereSetup
{
    private const string AtmosphereName = "AreaAtmosphere_Eien";
    private const string PointLightName = "AmbientPointLight";
    private const string EmbersName = "Ambient_Embers";
    private const string MistPrefix = "Issue657Mist";
    private const string ForegroundPrefix = "Issue657Foreground";
    private const string PropsPrefabDir = "Assets/ExtraPackage/Cainos/Pixel Art Platformer - Village Props/Prefab/";

    public static readonly string[] Scenes =
    {
        "Assets/Scenes/Ohirunebeya_start.unity",
        "Assets/Scenes/Ohirunebeya_2.unity",
        "Assets/Scenes/Ohirunebeya_3_leelee.unity",
        "Assets/Scenes/Ohirunebeya_4.unity",
        "Assets/Scenes/Ohirunebeya_5.unity",
        "Assets/Scenes/Ohirunebeya_5_boss.unity",
        "Assets/Scenes/Ohirunebeya_5_2.unity",
    };

    /** イベントの部屋。ライトと Volume だけ置く */
    private static readonly HashSet<string> EventScenes = new HashSet<string> { "Ohirunebeya_3_leelee", "Ohirunebeya_5_boss" };

    /** 炎の位置(プレハブのローカル座標)。fire=false は街灯で、火の粉を付けない */
    private struct FlameSpec { public string suffix; public Vector3 offset; public bool fire; }
    private static readonly FlameSpec[] Flames =
    {
        new FlameSpec { suffix = "Torch.prefab", offset = new Vector3(0f, 0.63f, 0f), fire = true },
        new FlameSpec { suffix = "Campfire 01.prefab", offset = new Vector3(-0.07f, 1.0f, 0f), fire = true },
        new FlameSpec { suffix = "Road Lamp.prefab", offset = new Vector3(0.855f, 2.2f, 0f), fire = false },
    };

    /** 低い靄。床面(タイル上面)の少し上に、幅 width の帯を置く */
    private struct MistSpec { public float x, y, width; }
    private static readonly Dictionary<string, MistSpec[]> Mists = new Dictionary<string, MistSpec[]>
    {
        {
            "Ohirunebeya_2", new[]
            {
                new MistSpec { x = 1.5f, y = -5.5f, width = 9f },
                new MistSpec { x = 20.5f, y = -2.5f, width = 5f },
                new MistSpec { x = 30f, y = 8.5f, width = 12f },
            }
        },
        {
            "Ohirunebeya_4", new[]
            {
                new MistSpec { x = 1.5f, y = -5.5f, width = 9f },
                new MistSpec { x = 29f, y = -5.5f, width = 10f },
                new MistSpec { x = 64f, y = -5.5f, width = 16f },
                new MistSpec { x = 120.5f, y = -5.5f, width = 11f },
            }
        },
    };

    /** 前景の草。PlayerUpperObject 層に暗めの色で置く。足場・ジャンプ先の上には置かない */
    private struct ForegroundSpec { public string prefab; public float x, y, scale; public bool flip; }
    private static readonly Dictionary<string, ForegroundSpec[]> Foregrounds = new Dictionary<string, ForegroundSpec[]>
    {
        {
            "Ohirunebeya_2", new[]
            {
                new ForegroundSpec { prefab = "PF Village Props - Grass 04", x = -1.5f, y = -6f, scale = 1.5f, flip = false },
                new ForegroundSpec { prefab = "PF Village Props - Grass 02", x = 4.6f, y = -6f, scale = 1.4f, flip = true },
                new ForegroundSpec { prefab = "PF Village Props - Grass 04", x = 14.2f, y = -6f, scale = 1.5f, flip = true },
                new ForegroundSpec { prefab = "PF Village Props - Grass 02", x = 26.5f, y = 8f, scale = 1.4f, flip = false },
            }
        },
        {
            "Ohirunebeya_4", new[]
            {
                new ForegroundSpec { prefab = "PF Village Props - Grass 04", x = 3.2f, y = -6f, scale = 1.5f, flip = false },
                new ForegroundSpec { prefab = "PF Village Props - Grass 02", x = 30.5f, y = -6f, scale = 1.4f, flip = true },
                new ForegroundSpec { prefab = "PF Village Props - Grass 04", x = 50.5f, y = -6f, scale = 1.5f, flip = true },
                new ForegroundSpec { prefab = "PF Village Props - Grass 02", x = 66f, y = -6f, scale = 1.4f, flip = false },
                new ForegroundSpec { prefab = "PF Village Props - Grass 04", x = 110.5f, y = -6f, scale = 1.5f, flip = false },
            }
        },
    };

    private static readonly Color ForegroundTint = new Color(0.5f, 0.46f, 0.58f, 1f);

    [MenuItem("Tools/Issue657/Apply Eien Atmosphere To All Scenes", false, 3450)]
    public static void ApplyToAllScenes()
    {
        foreach (var path in Scenes)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            Apply(scene);
            EditorSceneManager.SaveScene(scene);
        }
        Debug.Log($"[Issue657] {Scenes.Length} シーンに永遠の環境演出を適用して保存した");
    }

    [MenuItem("Tools/Issue657/Apply Eien Atmosphere To Open Scene", false, 3451)]
    public static void ApplyToOpenScene()
    {
        var scene = SceneManager.GetActiveScene();
        Apply(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"[Issue657] {scene.name} に適用した(未保存)");
    }

    [MenuItem("Tools/Issue657/Remove Eien Atmosphere From Open Scene", false, 3452)]
    public static void RemoveFromOpenScene()
    {
        var scene = SceneManager.GetActiveScene();
        Remove(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"[Issue657] {scene.name} から環境演出を外した(未保存)");
    }

    public static void Apply(Scene scene)
    {
        Remove(scene);

        var isEvent = EventScenes.Contains(scene.name);
        var atmospherePrefab = Load<GameObject>(AmbientAtmosphereBuilder.AtmosphereVariantPath("Eien"));
        var pointLightPrefab = Load<GameObject>(AmbientAtmosphereBuilder.PointLightPath);
        var embersPrefab = Load<GameObject>(AmbientAtmosphereBuilder.EmbersPath);
        var fogPrefab = Load<GameObject>(AmbientAtmosphereBuilder.FogPath);

        // 1. エリアの空気
        var atmosphere = (GameObject)PrefabUtility.InstantiatePrefab(atmospherePrefab, scene);
        atmosphere.name = AtmosphereName;
        atmosphere.transform.SetAsLastSibling();
        if (isEvent)
        {
            atmosphere.transform.Find("Dust").gameObject.SetActive(false);
        }

        // 2. 松明・焚き火・街灯
        var lights = 0;
        foreach (var t in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray())
        {
            if (!PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject)) continue;
            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.gameObject);
            foreach (var flame in Flames)
            {
                if (!path.EndsWith(flame.suffix)) continue;
                var light = (GameObject)PrefabUtility.InstantiatePrefab(pointLightPrefab, t);
                light.name = PointLightName;
                light.transform.localPosition = flame.offset;
                if (flame.fire && !isEvent)
                {
                    var embers = (GameObject)PrefabUtility.InstantiatePrefab(embersPrefab, t);
                    embers.name = EmbersName;
                    embers.transform.localPosition = flame.offset;
                }
                lights++;
            }
        }

        // 3. 低い靄と前景
        var mists = 0;
        if (!isEvent && Mists.TryGetValue(scene.name, out var mistSpecs))
        {
            for (var i = 0; i < mistSpecs.Length; i++)
            {
                PlaceMist(scene, fogPrefab, mistSpecs[i], $"{MistPrefix} ({i})");
                mists++;
            }
        }

        var foregrounds = 0;
        if (!isEvent && Foregrounds.TryGetValue(scene.name, out var foregroundSpecs))
        {
            for (var i = 0; i < foregroundSpecs.Length; i++)
            {
                PlaceForeground(scene, foregroundSpecs[i], $"{ForegroundPrefix} ({i})");
                foregrounds++;
            }
        }

        // 4. マテリアル差し替え(グローバルライトを置いた後に行う)
        var replaced = AmbientMaterialReplacer.Replace(scene, requireGlobalLight: true);

        Debug.Log($"[Issue657] {scene.name}: lights={lights} mists={mists} foregrounds={foregrounds} replaced={replaced} event={isEvent}");
    }

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
        main.startSizeX = new ParticleSystem.MinMaxCurve(6f, 9f);
        main.startSizeY = new ParticleSystem.MinMaxCurve(1.6f);
        main.maxParticles = 8;
        main.startColor = new Color(0.85f, 0.85f, 0.95f, 1f);

        var emission = ps.emission;
        emission.rateOverTime = Mathf.Max(0.15f, spec.width / 40f);

        var shape = ps.shape;
        shape.scale = new Vector3(spec.width, 1.2f, 0f);

        var velocity = ps.velocityOverLifetime;
        velocity.x = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.01f, 0.01f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        var color = ps.colorOverLifetime;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.16f, 0.25f),
                new GradientAlphaKey(0.16f, 0.75f),
                new GradientAlphaKey(0f, 1f),
            });
        color.color = new ParticleSystem.MinMaxGradient(gradient);

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sortingLayerName = "Effect";
        renderer.sortingOrder = -5;
    }

    private static void PlaceForeground(Scene scene, ForegroundSpec spec, string name)
    {
        var prefab = Load<GameObject>(PropsPrefabDir + spec.prefab + ".prefab");
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

    /** 生成物を消し、マテリアルを Sprites-Default に戻す */
    public static void Remove(Scene scene)
    {
        var roots = scene.GetRootGameObjects();
        var toDestroy = new List<GameObject>();
        foreach (var root in roots)
        {
            if (root.name == AtmosphereName || root.name.StartsWith(MistPrefix) || root.name.StartsWith(ForegroundPrefix))
            {
                toDestroy.Add(root);
                continue;
            }
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == PointLightName || t.name == EmbersName)
                {
                    toDestroy.Add(t.gameObject);
                }
            }
        }
        foreach (var go in toDestroy)
        {
            if (go != null) Object.DestroyImmediate(go);
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
