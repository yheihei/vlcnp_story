using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cinemachine;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VLCNP.Core;
using VLCNP.Effects;
using VLCNP.Stats;

/**
 * #652 Ohirunebeya_tuti系シーンのCoreフォーク統合。
 *   1. main Player.prefab に PoisonStatus / PoisonEffectController(Head子)を移植
 *   2. main Core.prefab に CameraPositionChanger を追加
 *   3. 9シーンのローカルCore参照を main Core に一括張り替え(オーバーライド維持)
 * ローカル側の値: poisonDuration=8, PoisonEffect.prefab, Head位置(-0.61,-0.66,0), moveAmount=4
 */
public static class Issue652CoreUnification
{
    const string PlayerPrefabPath = "Assets/Game/Characters/Player.prefab";
    const string CorePrefabPath = "Assets/Game/Core/Core.prefab";
    const string LocalCorePrefabPath = "Assets/Scenes/Ohirunebeya_tuti_1/Core.prefab";
    const string PoisonEffectPrefabPath = "Assets/Game/Effect/PoisonEffect.prefab";

    static readonly string[] TargetScenePaths =
    {
        "Assets/Scenes/Ohirunebeya_tuti_1.unity",
        "Assets/Scenes/Ohirunebeya_tuti_2.unity",
        "Assets/Scenes/Ohirunebeya_tuti_3.unity",
        "Assets/Scenes/Ohirunebeya_tuti_4.unity",
        "Assets/Scenes/Ohirunebeya_tuti_5.unity",
        "Assets/Scenes/Ohirunebeya_tuti_6_boss_1.unity",
        "Assets/Scenes/Ohirunebeya_tuti_6_boss_2.unity",
        "Assets/Scenes/Ohirunebeya_tuti_6_boss_3.unity",
        "Assets/Scenes/TrialEnd2.unity",
    };

    [MenuItem("Tools/Issue652/1 Add Poison To Player")]
    public static void AddPoisonToPlayer()
    {
        GameObject effectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            PoisonEffectPrefabPath
        );
        if (effectPrefab == null)
        {
            Debug.LogError($"[Issue652] PoisonEffect.prefab が見つかりません: {PoisonEffectPrefabPath}");
            return;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
        try
        {
            PoisonStatus status = root.GetComponent<PoisonStatus>();
            if (status == null)
            {
                status = root.AddComponent<PoisonStatus>();
            }
            SerializedObject statusSo = new(status);
            statusSo.FindProperty("poisonDuration").floatValue = 8f;
            statusSo.ApplyModifiedPropertiesWithoutUndo();

            Transform head = root.transform.Find("Head");
            if (head == null)
            {
                GameObject headObject = new("Head");
                headObject.transform.SetParent(root.transform, false);
                head = headObject.transform;
            }
            head.localPosition = new Vector3(-0.61f, -0.66f, 0f);

            PoisonEffectController controller = head.GetComponent<PoisonEffectController>();
            if (controller == null)
            {
                controller = head.gameObject.AddComponent<PoisonEffectController>();
            }
            controller.poisonEffectPrefab = effectPrefab;
            controller.effectParent = head;

            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            Debug.Log("[Issue652] Player.prefab に PoisonStatus / PoisonEffectController を移植しました");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    [MenuItem("Tools/Issue652/2 Add CameraPositionChanger To Core")]
    public static void AddCameraPositionChangerToCore()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(CorePrefabPath);
        try
        {
            CinemachineVirtualCamera virtualCamera =
                root.GetComponentInChildren<CinemachineVirtualCamera>(true);
            if (virtualCamera == null)
            {
                Debug.LogError("[Issue652] Core.prefab 内に CinemachineVirtualCamera が見つかりません");
                return;
            }

            Transform existing = root.transform.Find("CameraPositionChanger");
            GameObject changerObject =
                existing != null ? existing.gameObject : new GameObject("CameraPositionChanger");
            changerObject.transform.SetParent(root.transform, false);
            changerObject.tag = "CameraPositionChanger";

            CameraPositionChanger changer = changerObject.GetComponent<CameraPositionChanger>();
            if (changer == null)
            {
                changer = changerObject.AddComponent<CameraPositionChanger>();
            }
            SerializedObject so = new(changer);
            so.FindProperty("virtualCamera").objectReferenceValue = virtualCamera;
            so.FindProperty("moveAmount").floatValue = 4f;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, CorePrefabPath);
            Debug.Log("[Issue652] Core.prefab に CameraPositionChanger を追加しました");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    [MenuItem("Tools/Issue652/3 Convert Scenes")]
    public static void ConvertScenes()
    {
        GameObject localCore = AssetDatabase.LoadAssetAtPath<GameObject>(LocalCorePrefabPath);
        GameObject mainCore = AssetDatabase.LoadAssetAtPath<GameObject>(CorePrefabPath);
        if (localCore == null || mainCore == null)
        {
            Debug.LogError("[Issue652] Core プレハブのロードに失敗しました");
            return;
        }

        StringBuilder report = new();
        foreach (string scenePath in TargetScenePaths)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            GameObject instance = scene
                .GetRootGameObjects()
                .FirstOrDefault(go =>
                    PrefabUtility.GetCorrespondingObjectFromSource(go) != null
                    && AssetDatabase.GetAssetPath(
                        PrefabUtility.GetCorrespondingObjectFromSource(go)
                    ) == LocalCorePrefabPath
                );
            if (instance == null)
            {
                report.AppendLine($"[SKIP] {scenePath}: ローカルCoreインスタンスが見つかりません");
                continue;
            }

            Dictionary<string, string> before = SnapshotCoreState(instance);

            PrefabReplacingSettings settings = new()
            {
                changeRootNameToAssetName = false,
                logInfo = false,
                objectMatchMode = ObjectMatchMode.ByHierarchy,
                prefabOverridesOptions = PrefabOverridesOptions.KeepAllPossibleOverrides,
            };
            PrefabUtility.ReplacePrefabAssetOfPrefabInstance(
                instance,
                mainCore,
                settings,
                InteractionMode.AutomatedAction
            );

            Dictionary<string, string> after = SnapshotCoreState(instance);
            EditorSceneManager.SaveScene(scene);

            report.AppendLine($"[DONE] {scenePath}");
            foreach (KeyValuePair<string, string> pair in before)
            {
                string afterValue = after.TryGetValue(pair.Key, out string v) ? v : "(消失)";
                if (afterValue != pair.Value)
                {
                    report.AppendLine($"  [DIFF] {pair.Key}: '{pair.Value}' -> '{afterValue}'");
                }
            }
            foreach (string key in after.Keys.Where(k => !before.ContainsKey(k)))
            {
                report.AppendLine($"  [NEW] {key}: '{after[key]}'");
            }
        }
        Debug.Log($"[Issue652] シーン変換レポート\n{report}");
    }

    // 旧ローカルCoreで削除オーバーライドされていた FramingTransposer を再削除する対象
    static readonly string[] TransposerRemovedScenePaths =
    {
        "Assets/Scenes/Ohirunebeya_tuti_3.unity",
        "Assets/Scenes/Ohirunebeya_tuti_4.unity",
        "Assets/Scenes/Ohirunebeya_tuti_5.unity",
        "Assets/Scenes/Ohirunebeya_tuti_6_boss_1.unity",
        "Assets/Scenes/Ohirunebeya_tuti_6_boss_2.unity",
        "Assets/Scenes/Ohirunebeya_tuti_6_boss_3.unity",
        "Assets/Scenes/TrialEnd2.unity",
    };

    [MenuItem("Tools/Issue652/4 Reapply Transposer Removal")]
    public static void ReapplyTransposerRemoval()
    {
        StringBuilder report = new();
        foreach (string scenePath in TransposerRemovedScenePaths)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            GameObject instance = scene
                .GetRootGameObjects()
                .FirstOrDefault(go =>
                    PrefabUtility.GetCorrespondingObjectFromSource(go) != null
                    && AssetDatabase.GetAssetPath(
                        PrefabUtility.GetCorrespondingObjectFromSource(go)
                    ) == CorePrefabPath
                );
            if (instance == null)
            {
                report.AppendLine($"[SKIP] {scenePath}: main Coreインスタンスが見つかりません");
                continue;
            }

            CinemachineFramingTransposer[] transposers =
                instance.GetComponentsInChildren<CinemachineFramingTransposer>(true);
            if (transposers.Length == 0)
            {
                report.AppendLine($"[SKIP] {scenePath}: FramingTransposer は既にありません");
                continue;
            }
            foreach (CinemachineFramingTransposer transposer in transposers)
            {
                Object.DestroyImmediate(transposer);
            }
            EditorSceneManager.SaveScene(scene);
            report.AppendLine($"[DONE] {scenePath}: FramingTransposer x{transposers.Length} を削除");
        }
        Debug.Log($"[Issue652] Transposer削除レポート\n{report}");
    }

    [MenuItem("Tools/Issue652/5 Verify Scenes")]
    public static void VerifyScenes()
    {
        StringBuilder report = new();
        foreach (string scenePath in TargetScenePaths)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            int missingScripts = 0;
            int missingPrefabs = 0;
            bool hasCore = false;
            bool hasTransposer = false;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (
                    Transform child in root.GetComponentsInChildren<Transform>(true)
                )
                {
                    missingScripts +=
                        GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                            child.gameObject
                        );
                    if (PrefabUtility.IsPrefabAssetMissing(child.gameObject))
                    {
                        missingPrefabs++;
                    }
                }
                GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(root);
                if (
                    source != null
                    && AssetDatabase.GetAssetPath(source) == CorePrefabPath
                )
                {
                    hasCore = true;
                    hasTransposer =
                        root.GetComponentInChildren<CinemachineFramingTransposer>(true)
                        != null;
                }
            }
            report.AppendLine(
                $"{scenePath}: mainCore={hasCore} transposer={hasTransposer} "
                    + $"missingScripts={missingScripts} missingPrefabs={missingPrefabs}"
            );
        }
        Debug.Log($"[Issue652] シーン検証レポート\n{report}");
    }

    // 変換前後で保持されるべき状態のスナップショット
    static Dictionary<string, string> SnapshotCoreState(GameObject coreInstance)
    {
        Dictionary<string, string> state = new();

        Camera camera = coreInstance.GetComponentInChildren<Camera>(true);
        if (camera != null)
        {
            state["Camera.backgroundColor"] = camera.backgroundColor.ToString();
            state["Camera.orthographicSize"] = camera.orthographicSize.ToString();
        }

        CinemachineVirtualCamera virtualCamera =
            coreInstance.GetComponentInChildren<CinemachineVirtualCamera>(true);
        if (virtualCamera != null)
        {
            state["VCam.OrthographicSize"] = virtualCamera.m_Lens.OrthographicSize.ToString();
        }

        foreach (TMP_Text text in coreInstance.GetComponentsInChildren<TMP_Text>(true))
        {
            if (!string.IsNullOrEmpty(text.text))
            {
                state[$"Text.{GetPath(text.transform, coreInstance.transform)}"] = text.text;
            }
        }

        foreach (AudioSource audio in coreInstance.GetComponentsInChildren<AudioSource>(true))
        {
            if (audio.clip != null)
            {
                state[$"Audio.{GetPath(audio.transform, coreInstance.transform)}"] =
                    audio.clip.name;
            }
        }

        return state;
    }

    static string GetPath(Transform target, Transform root)
    {
        List<string> names = new();
        for (Transform current = target; current != null && current != root; current = current.parent)
        {
            names.Insert(0, current.name);
        }
        return string.Join("/", names);
    }
}
