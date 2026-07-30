using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * Kaze3_boss に Yami5F と同じ「Karma が VL の CNP を闇落ちさせる」イベントを移植するエディタ拡張。
 * Yami5F を追加ロードし、イベント関連のオブジェクトを Kaze3_boss へ移した上で
 *   1. TransitionSpawnPoint 基準の相対座標を維持した再配置
 *   2. Mitama 系オブジェクトを Narukami 名義へリネーム(スプライトは後で差し替える前提)
 *   3. Yami5F 側に残るオブジェクト(Core/BGMWrapper など)への参照を Kaze3_boss の同じパスへ張り替え
 * を行う。Yami5F は保存せずに閉じるため元シーンは変更されない。
 * 既にイベントが移植済み(Flowchart が存在する)の場合は何もしない。
 */
public static class Kaze3BossKarmaEventBuilder
{
    private const string TargetScenePath = "Assets/Scenes/Kaze3_boss.unity";
    private const string SourceScenePath = "Assets/Scenes/Yami5F.unity";
    private const string SpawnPointName = "TransitionSpawnPoint";

    // Yami5F のルート直下から移植するオブジェクト
    private static readonly string[] RootObjects =
    {
        "NPCKarma",
        "VLMitama",
        "NPCVLDarkMitamaVariant",
        "DarkRainbowSeed",
        "NPCAkim",
        "NPCLeelee Variant",
        "AllFaces",
        "Flowchart",
        "Yami5FToBossTransitionEvent",
    };

    // Core 配下から移植するオブジェクト(イベント用のバーチャルカメラ)
    private static readonly string[] CoreChildren = { "CMCamera_1", "CMCamera_2" };

    // 闇落ち対象は VLNarukami(未実装)。当面は Mitama のアセットを流用し、名前だけ Narukami にする。
    private static readonly Dictionary<string, string> Renames = new Dictionary<string, string>
    {
        { "VLMitama", "VLNarukami" },
        { "NPCVLDarkMitamaVariant", "NPCVLDarkNarukamiVariant" },
        { "Yami5FToBossTransitionEvent", "Kaze3BossTransitionEvent" },
    };

    [MenuItem("Tools/VLCNP/Event/Port Yami5F Karma Event To Kaze3_boss", false, 3221)]
    public static void Port()
    {
        var target = SceneManager.GetActiveScene();
        if (target.path != TargetScenePath)
        {
            Debug.LogError($"アクティブシーンが {TargetScenePath} ではありません: {target.path}");
            return;
        }
        if (FindRoot(target, "Flowchart") != null)
        {
            Debug.LogError("Kaze3_boss に既に Flowchart があります。移植済みとみなして中断します。");
            return;
        }

        var targetSpawn = FindRoot(target, SpawnPointName);
        var targetCore = FindRoot(target, "Core");
        if (targetSpawn == null || targetCore == null)
        {
            Debug.LogError("Kaze3_boss に TransitionSpawnPoint または Core が見つかりません。");
            return;
        }

        var source = EditorSceneManager.OpenScene(SourceScenePath, OpenSceneMode.Additive);
        try
        {
            var sourceSpawn = FindRoot(source, SpawnPointName);
            var sourceCore = FindRoot(source, "Core");
            if (sourceSpawn == null || sourceCore == null)
            {
                Debug.LogError("Yami5F に TransitionSpawnPoint または Core が見つかりません。");
                return;
            }

            Vector3 shift = targetSpawn.transform.position - sourceSpawn.transform.position;
            var moved = new List<GameObject>();
            // Yami5F では HUD が CMCamera_1 の子に置かれている。Kaze3_boss は自前の HUD を持つため、
            // そのまま持ち込むと HUD が二重に表示される。
            bool targetHasHud = FindAnyByName(target, "HUD") != null;

            foreach (var name in RootObjects)
            {
                var go = FindRoot(source, name);
                if (go == null)
                {
                    Debug.LogWarning($"Yami5F に {name} が見つかりません。スキップします。");
                    continue;
                }
                Vector3 world = go.transform.position;
                SceneManager.MoveGameObjectToScene(go, target);
                go.transform.position = Relocate(world, shift);
                moved.Add(go);
            }

            foreach (var name in CoreChildren)
            {
                var child = sourceCore.transform.Find(name);
                if (child == null)
                {
                    Debug.LogWarning($"Yami5F の Core に {name} が見つかりません。スキップします。");
                    continue;
                }
                Vector3 world = child.position;
                child.SetParent(null, true);
                SceneManager.MoveGameObjectToScene(child.gameObject, target);
                child.SetParent(targetCore.transform, true);
                child.position = Relocate(world, shift);
                moved.Add(child.gameObject);

                if (targetHasHud)
                {
                    var hud = child.Find("HUD");
                    if (hud != null)
                    {
                        UnityEngine.Object.DestroyImmediate(hud.gameObject);
                        Debug.Log($"{name} が持ち込んだ HUD を削除しました(Kaze3_boss 側の HUD を使う)。");
                    }
                }
            }

            RemapSceneReferences(moved, source, target);

            foreach (var pair in Renames)
            {
                var go = moved.FirstOrDefault(g => g.name == pair.Key);
                if (go == null) continue;
                go.name = pair.Value;
            }

            AlignToSpawnHorizontally(moved, targetSpawn);

            ClearTransitionDestination(moved);
        }
        finally
        {
            // Yami5F は保存せずに閉じる(移動によって dirty になっているだけで、ディスク上の内容は変わらない)
            if (source.IsValid() && source.isLoaded) EditorSceneManager.CloseScene(source, true);
        }

        EditorSceneManager.MarkSceneDirty(target);
        EditorSceneManager.SaveScene(target);
        Debug.Log("Yami5F の Karma 闇落ちイベントを Kaze3_boss へ移植しました。");
    }

    /**
     * 闇落ち対象(VLNarukami)が TransitionSpawnPoint の真上に来るよう、イベント一式を横方向にまとめて寄せる。
     * VLNarukami は宙に浮くため、Y を合わせると地面に立つキャラが床に埋まる。よって X だけを合わせる。
     */
    private static void AlignToSpawnHorizontally(List<GameObject> moved, GameObject targetSpawn)
    {
        var anchor = moved.FirstOrDefault(g => g.name == Renames["VLMitama"]);
        if (anchor == null)
        {
            Debug.LogWarning("VLNarukami が見つからないため横位置の調整をスキップしました。");
            return;
        }
        float dx = targetSpawn.transform.position.x - anchor.transform.position.x;
        foreach (var go in moved)
        {
            var p = go.transform.position;
            if (new Vector2(p.x, p.y).sqrMagnitude < 0.0001f) continue;
            go.transform.position = new Vector3(p.x + dx, p.y, p.z);
        }
    }

    /**
     * ワールド原点に置かれているオブジェクト(Flowchart など見た目を持たないもの)は原点のまま、
     * それ以外は TransitionSpawnPoint からの相対位置を保って移す。
     */
    private static Vector3 Relocate(Vector3 world, Vector3 shift)
    {
        if (new Vector2(world.x, world.y).sqrMagnitude < 0.0001f) return world;
        return new Vector3(world.x + shift.x, world.y + shift.y, world.z);
    }

    /**
     * 移植したオブジェクトが持つ「Yami5F 側に残ったオブジェクト」への参照を、
     * Kaze3_boss 内の同じヒエラルキーパスのオブジェクトへ張り替える。
     * シーンをまたいだ参照は保存時に null になるため、閉じる前に必ず解決しておく。
     */
    private static void RemapSceneReferences(List<GameObject> moved, Scene source, Scene target)
    {
        foreach (var root in moved)
        {
            foreach (var component in root.GetComponentsInChildren<Component>(true))
            {
                if (component == null) continue;
                var so = new SerializedObject(component);
                var prop = so.GetIterator();
                bool changed = false;
                while (prop.Next(true))
                {
                    if (prop.propertyType != SerializedPropertyType.ObjectReference) continue;
                    var value = prop.objectReferenceValue;
                    if (value == null) continue;

                    var sourceGo = value as GameObject ?? (value as Component)?.gameObject;
                    if (sourceGo == null) continue;
                    if (sourceGo.scene != source) continue;

                    var path = HierarchyPath(sourceGo);
                    var replacementGo = FindByPath(target, path);
                    if (replacementGo == null)
                    {
                        Debug.LogWarning(
                            $"{HierarchyPath(component.gameObject)}.{component.GetType().Name}.{prop.propertyPath} "
                                + $"の参照先 '{path}' が Kaze3_boss に無いため解決できません。手動で設定してください。");
                        continue;
                    }

                    UnityEngine.Object replacement = value is GameObject
                        ? replacementGo
                        : FindSameComponent(sourceGo, replacementGo, (Component)value);
                    if (replacement == null)
                    {
                        Debug.LogWarning(
                            $"{HierarchyPath(component.gameObject)}.{component.GetType().Name}.{prop.propertyPath} "
                                + $"の参照先コンポーネント {value.GetType().Name} が '{path}' に無いため解決できません。");
                        continue;
                    }

                    prop.objectReferenceValue = replacement;
                    changed = true;
                    Debug.Log(
                        $"参照を張り替えました: {HierarchyPath(component.gameObject)}.{component.GetType().Name}"
                            + $".{prop.propertyPath} -> {path} ({replacement.GetType().Name})");
                }
                if (changed) so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }

    /**
     * 同名オブジェクトに同じ型のコンポーネントが複数ある場合も取り違えないよう、
     * 元オブジェクト内での出現順(インデックス)を合わせて取得する。
     */
    private static Component FindSameComponent(GameObject sourceGo, GameObject targetGo, Component sourceComponent)
    {
        var type = sourceComponent.GetType();
        var sourceComponents = sourceGo.GetComponents(type);
        int index = Array.IndexOf(sourceComponents, sourceComponent);
        var targetComponents = targetGo.GetComponents(type);
        if (index < 0 || index >= targetComponents.Length) return targetComponents.FirstOrDefault();
        return targetComponents[index];
    }

    private static void ClearTransitionDestination(List<GameObject> moved)
    {
        var go = moved.FirstOrDefault(g => g.name == "Yami5FToBossTransitionEvent");
        if (go == null) return;
        var transition = go.GetComponent("TransitionEvent") as MonoBehaviour;
        if (transition == null) return;
        var so = new SerializedObject(transition);
        // 遷移先シーンは未定。-1(未設定)にしておき、後からインスペクタで設定する。
        so.FindProperty("sceneToLoad").intValue = -1;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject FindRoot(Scene scene, string name)
    {
        return scene.GetRootGameObjects().FirstOrDefault(g => g.name == name);
    }

    private static GameObject FindAnyByName(Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var hit = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
            if (hit != null) return hit.gameObject;
        }
        return null;
    }

    private static GameObject FindByPath(Scene scene, string path)
    {
        var segments = path.Split('/');
        var root = FindRoot(scene, segments[0]);
        if (root == null || segments.Length == 1) return root;
        var child = root.transform.Find(string.Join("/", segments.Skip(1)));
        return child == null ? null : child.gameObject;
    }

    private static string HierarchyPath(GameObject go)
    {
        var names = new List<string>();
        for (var t = go.transform; t != null; t = t.parent) names.Add(t.name);
        names.Reverse();
        return string.Join("/", names);
    }
}
