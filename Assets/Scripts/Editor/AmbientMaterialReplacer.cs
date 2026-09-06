using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/**
 * #656 対象シーンのタイルマップと小物のマテリアルを、builtin Sprites-Default から
 * ライト対応の AmbientSpriteLit へ差し替えるエディタ拡張。
 *
 * 差し替えるのは SpriteRenderer と TilemapRenderer で、共有マテリアルが builtin Sprites-Default のものだけ。
 * Core(プレイヤー・UI)と Assets/Game/Characters 配下のプレハブ由来のもの、Fungus 由来のものは触らない。
 * ライト対応マテリアルは Global Light 2D が無いと黒く描かれるので、シーンにグローバルライトが無ければ中断する。
 */
public static class AmbientMaterialReplacer
{
    private static readonly string[] SkipPrefabPathPrefixes =
    {
        "Assets/Game/Core/",
        "Assets/Game/Characters/",
        "Assets/Fungus/",
    };

    [MenuItem("Tools/VLCNP/Ambient/Replace Sprites-Default In Open Scene (#656)", false, 3401)]
    public static void ReplaceInOpenScene()
    {
        var scene = SceneManager.GetActiveScene();
        var count = Replace(scene, requireGlobalLight: true);
        if (count > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }
        Debug.Log($"[AmbientMaterialReplacer] {scene.name}: {count} renderers replaced (未保存)");
    }

    /** シーンを開いて差し替え、保存する。差し替えた数を返す。 */
    public static int ReplaceInScene(string scenePath, bool requireGlobalLight = true)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var count = Replace(scene, requireGlobalLight);
        if (count > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        Debug.Log($"[AmbientMaterialReplacer] {scene.name}: {count} renderers replaced");
        return count;
    }

    public static int Replace(Scene scene, bool requireGlobalLight)
    {
        var target = AssetDatabase.LoadAssetAtPath<Material>(AmbientAtmosphereBuilder.LitMaterialPath);
        if (target == null)
        {
            Debug.LogError($"[AmbientMaterialReplacer] 環境用マテリアルが無い: {AmbientAtmosphereBuilder.LitMaterialPath}(先に Build Ambient Assets を実行)");
            return 0;
        }

        var builtinDefault = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        var roots = scene.GetRootGameObjects();

        if (requireGlobalLight)
        {
            var hasGlobalLight = roots
                .SelectMany(r => r.GetComponentsInChildren<Light2D>(true))
                .Any(l => l.lightType == Light2D.LightType.Global);
            if (!hasGlobalLight)
            {
                Debug.LogError($"[AmbientMaterialReplacer] {scene.name} に Global Light 2D が無いので中断(AreaAtmosphere の Variant を先に置く)");
                return 0;
            }
        }

        var count = 0;
        foreach (var renderer in roots.SelectMany(r => r.GetComponentsInChildren<Renderer>(true)))
        {
            if (!(renderer is SpriteRenderer) && !(renderer is TilemapRenderer)) continue;
            if (ShouldSkip(renderer.gameObject)) continue;

            var materials = renderer.sharedMaterials;
            var changed = false;
            for (var i = 0; i < materials.Length; i++)
            {
                if (materials[i] == builtinDefault)
                {
                    materials[i] = target;
                    changed = true;
                }
            }
            if (!changed) continue;

            Undo.RecordObject(renderer, "Replace ambient material");
            renderer.sharedMaterials = materials;
            EditorUtility.SetDirty(renderer);
            count++;
        }
        return count;
    }

    private static bool ShouldSkip(GameObject go)
    {
        var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
        if (!string.IsNullOrEmpty(prefabPath) && SkipPrefabPathPrefixes.Any(prefabPath.StartsWith))
        {
            return true;
        }

        // ネストしたプレハブの外側が Core などの場合も除外する
        for (var t = go.transform; t != null; t = t.parent)
        {
            var outer = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.gameObject);
            if (!string.IsNullOrEmpty(outer) && SkipPrefabPathPrefixes.Any(outer.StartsWith))
            {
                return true;
            }
        }
        return false;
    }
}
