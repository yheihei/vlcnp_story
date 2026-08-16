using System;
using System.Collections.Generic;
using System.Linq;
using Fungus;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VLCNP.Movie;
using VLCNP.SceneManagement;

/**
 * Kaze3_boss_2 のボス撃破後に遷移する「VLナルカミ加入イベント」シーン Kaze3_boss3 を組み立てるエディタ拡張。
 *   1. Kaze3_boss を複製して Kaze3_boss3 を作り、カルマ闇落ちイベント関連オブジェクトを削除
 *   2. Yami5F-3 から VLMitamaJoinedGameEvent(会話 Flowchart)と GameEventTransition を移植
 *   3. 会話を仮置きのナルカミ加入会話に差し替え(SetFlag は VLNarukamiJoined、話者は NarukamiCharacter)
 *   4. Kaze3_boss3 を Build Settings に追加し、Kaze3_boss_2 の撃破遷移先を Kaze3_boss3 に変更
 * Yami5F-3 は保存せずに閉じるため元シーンは変更されない。Kaze3_boss3 が既に存在する場合は何もしない。
 */
public static class Kaze3Boss3SceneBuilder
{
    private const string BaseScenePath = "Assets/Scenes/Kaze3_boss.unity";
    private const string TargetScenePath = "Assets/Scenes/Kaze3_boss3.unity";
    private const string SourceScenePath = "Assets/Scenes/Yami5F-3.unity";
    private const string BossScenePath = "Assets/Scenes/Kaze3_boss_2.unity";

    // 複製後に削除するカルマイベント関連のルートオブジェクト
    private static readonly string[] KarmaEventRoots =
    {
        "Flowchart",
        "NPCKarma",
        "NPCVLDarkNarukamiVariant",
        "DarkRainbowSeed",
        "Kaze3BossTransitionEvent",
    };

    // カルマイベント用に Core 配下へ移植されていたバーチャルカメラ
    private static readonly string[] KarmaEventCoreChildren = { "CMCamera_1", "CMCamera_2" };

    // 仮置きの加入会話。最後の 1 行はナレーション(話者なし)
    private static readonly string[] PlaceholderSayTexts =
    {
        "う、うう……　わしは　いったい……",
        "そうか……　カルマに　あやつられとったんか……\nたすけてくれて　おおきに！",
        "おまえさんらの　たびに　わしも　つれていってくれ！",
        "VLナルカミが　なかまに　なった！",
    };

    [MenuItem("Tools/VLCNP/Event/Build Kaze3_boss3 Narukami Joined Scene", false, 3222)]
    public static void Build()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TargetScenePath) != null)
        {
            Debug.LogError($"{TargetScenePath} は既に存在します。作成済みとみなして中断します。");
            return;
        }
        if (!AssetDatabase.CopyAsset(BaseScenePath, TargetScenePath))
        {
            Debug.LogError($"{BaseScenePath} の複製に失敗しました。");
            return;
        }

        var target = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
        RemoveKarmaEventObjects(target);

        var narukami = FindRoot(target, "VLNarukami");
        var spawn = FindRoot(target, "TransitionSpawnPoint");
        if (narukami == null || spawn == null)
        {
            Debug.LogError("Kaze3_boss3 に VLNarukami または TransitionSpawnPoint が見つかりません。");
            return;
        }

        AddEventCameraCenteredOn(target, narukami);

        var source = EditorSceneManager.OpenScene(SourceScenePath, OpenSceneMode.Additive);
        try
        {
            var joinedEvent = FindRoot(source, "VLMitamaJoinedGameEvent");
            var doorTransition = FindRoot(source, "GameEventTransition");
            if (joinedEvent == null || doorTransition == null)
            {
                Debug.LogError("Yami5F-3 に VLMitamaJoinedGameEvent または GameEventTransition が見つかりません。");
                return;
            }

            SceneManager.MoveGameObjectToScene(joinedEvent, target);
            SceneManager.MoveGameObjectToScene(doorTransition, target);
            var moved = new List<GameObject> { joinedEvent, doorTransition };

            RewriteFlowchart(joinedEvent, narukami);
            RemapSceneReferences(moved, source, target);

            joinedEvent.name = "VLNarukamiJoinedGameEvent";
            doorTransition.name = "VLNarukamiJoinedTransitionEvent";
            // 会話終了時に ExecuteTransition を直接呼ぶだけなので、扉トリガーはプレイ範囲外に置く
            doorTransition.transform.position = spawn.transform.position + new Vector3(0, -30, 0);
        }
        finally
        {
            if (source.IsValid() && source.isLoaded) EditorSceneManager.CloseScene(source, true);
        }

        EditorSceneManager.MarkSceneDirty(target);
        EditorSceneManager.SaveScene(target);

        int buildIndex = AddToBuildSettings(TargetScenePath);
        PointBossDefeatTransitionTo(buildIndex);

        Debug.Log($"Kaze3_boss3 を作成しました(buildIndex={buildIndex})。撃破遷移先も更新済みです。");
    }

    private static void RemoveKarmaEventObjects(Scene target)
    {
        foreach (var name in KarmaEventRoots)
        {
            var go = FindRoot(target, name);
            if (go == null)
            {
                Debug.LogWarning($"Kaze3_boss3 に {name} が見つかりません。スキップします。");
                continue;
            }
            UnityEngine.Object.DestroyImmediate(go);
        }
        var core = FindRoot(target, "Core");
        if (core == null)
        {
            Debug.LogWarning("Kaze3_boss3 に Core が見つかりません。CMCamera の削除をスキップします。");
            return;
        }
        foreach (var name in KarmaEventCoreChildren)
        {
            var child = core.transform.Find(name);
            if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
    }

    /**
     * イベント中 VLNarukami が画面中央になるよう、追従なしの固定バーチャルカメラを
     * Core 配下に追加する(通常の追従カメラ priority=10 より高い 20 で常時勝たせる)。
     */
    private static void AddEventCameraCenteredOn(Scene target, GameObject narukami)
    {
        var core = FindRoot(target, "Core");
        var baseCam = core != null ? core.transform.Find("CMCamera") : null;
        var baseVcam = baseCam != null ? baseCam.GetComponent<Cinemachine.CinemachineVirtualCamera>() : null;
        if (baseVcam == null)
        {
            Debug.LogWarning("Core/CMCamera が見つからないため、イベントカメラの追加をスキップします。");
            return;
        }
        var go = new GameObject("CMCamera_1");
        go.transform.SetParent(core.transform, false);
        var p = narukami.transform.position;
        go.transform.position = new Vector3(p.x, p.y, -10f);
        var vcam = go.AddComponent<Cinemachine.CinemachineVirtualCamera>();
        vcam.m_Lens = baseVcam.m_Lens;
        vcam.Priority = 20;
    }

    /**
     * 移植した Chat の Flowchart をナルカミ加入用の仮会話へ組み替える。
     * ミタマ専用の NPCController 操作(SetDirection/Shake/Hatena)は削除し、
     * Bikkuri は VLNarukami の Emotion へ付け替える。
     */
    private static void RewriteFlowchart(GameObject joinedEvent, GameObject narukami)
    {
        var chat = joinedEvent.transform.Find("Chat");
        var block = chat != null ? chat.GetComponent<Block>() : null;
        if (block == null)
        {
            Debug.LogError("移植した VLMitamaJoinedGameEvent/Chat に Block が見つかりません。");
            return;
        }

        var emotion = narukami.GetComponentInChildren<Emotion>(true);
        var narukamiCharacter = FindCharacterInScene(joinedEvent.scene, "NarukamiCharacter");
        if (emotion == null) Debug.LogWarning("VLNarukami に Emotion が見つかりません。Bikkuri は削除します。");
        if (narukamiCharacter == null) Debug.LogWarning("AllFaces に NarukamiCharacter が見つかりません。話者は未設定になります。");

        Command stopAll = null, bikkuri = null, muteSe = null, unmuteSe = null, setFlag = null, execTransition = null;
        Command fadeScreen = null, playSound = null;
        var says = new List<Say>();
        var waits = new List<Wait>();

        foreach (var cmd in block.CommandList)
        {
            if (cmd == null) continue;
            if (cmd is Say say) { says.Add(say); continue; }
            if (cmd is Wait wait) { waits.Add(wait); continue; }
            if (cmd is FadeScreen) { fadeScreen ??= cmd; continue; }
            if (cmd is PlaySound) { playSound ??= cmd; continue; }
            if (cmd is InvokeMethod)
            {
                var method = new SerializedObject(cmd).FindProperty("targetMethod").stringValue;
                switch (method)
                {
                    case "StopAll": stopAll = cmd; break;
                    case "Bikkuri": bikkuri ??= cmd; break;
                    case "MuteForSE": muteSe = cmd; break;
                    case "UnmuteForSE": unmuteSe = cmd; break;
                    case "SetFlag": setFlag = cmd; break;
                    case "ExecuteTransition": execTransition = cmd; break;
                }
            }
        }

        if (says.Count < PlaceholderSayTexts.Length || waits.Count < 3 || setFlag == null || execTransition == null)
        {
            Debug.LogError("移植した Flowchart に想定するコマンドが足りません。組み替えを中断します。");
            return;
        }

        // Bikkuri を VLNarukami の Emotion へ付け替え
        if (bikkuri != null && emotion != null)
        {
            var so = new SerializedObject(bikkuri);
            so.FindProperty("targetObject").objectReferenceValue = emotion.gameObject;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        else
        {
            bikkuri = null;
        }

        // SetFlag のパラメータを VLNarukamiJoined に変更
        {
            var so = new SerializedObject(setFlag);
            so.FindProperty("methodParameters.Array.data[0].objValue.intValue").intValue =
                (int)VLCNP.Core.Flag.VLNarukamiJoined;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // 仮会話の Say を設定(最後の 1 行はナレーション)
        for (int i = 0; i < PlaceholderSayTexts.Length; i++)
        {
            var so = new SerializedObject(says[i]);
            so.FindProperty("storyText").stringValue = PlaceholderSayTexts[i];
            bool isNarration = i == PlaceholderSayTexts.Length - 1;
            so.FindProperty("character").objectReferenceValue = isNarration ? null : narukamiCharacter;
            so.FindProperty("portrait").objectReferenceValue = null;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        var newList = new List<Command>();
        void Add(Command cmd) { if (cmd != null) newList.Add(cmd); }
        Add(stopAll);
        Add(fadeScreen);
        Add(waits[0]);
        Add(bikkuri);
        Add(waits[1]);
        Add(says[0]);
        Add(says[1]);
        Add(says[2]);
        Add(muteSe);
        Add(playSound);
        Add(waits[2]);
        Add(unmuteSe);
        Add(says[3]);
        Add(setFlag);
        Add(execTransition);

        var removed = block.CommandList.Where(c => c != null && !newList.Contains(c)).ToList();
        block.CommandList.Clear();
        block.CommandList.AddRange(newList);
        foreach (var cmd in removed)
        {
            UnityEngine.Object.DestroyImmediate(cmd);
        }
        EditorUtility.SetDirty(block);
    }

    private static Character FindCharacterInScene(Scene scene, string name)
    {
        var allFaces = FindRoot(scene, "AllFaces");
        var child = allFaces != null ? allFaces.transform.Find(name) : null;
        return child != null ? child.GetComponent<Character>() : null;
    }

    private static int AddToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        if (!scenes.Any(s => s.path == scenePath))
        {
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
        // ビルドインデックスは有効なシーンのみを数える
        int index = 0;
        foreach (var s in scenes)
        {
            if (s.path == scenePath) return index;
            if (s.enabled) index++;
        }
        return -1;
    }

    private static void PointBossDefeatTransitionTo(int buildIndex)
    {
        var boss = EditorSceneManager.OpenScene(BossScenePath, OpenSceneMode.Single);
        var eventGo = FindRoot(boss, "VLNarukamiDefeatedTransitionEvent");
        var transition = eventGo != null ? eventGo.GetComponent<TransitionEvent>() : null;
        if (transition == null)
        {
            Debug.LogError("Kaze3_boss_2 に VLNarukamiDefeatedTransitionEvent (TransitionEvent) が見つかりません。");
            return;
        }
        var so = new SerializedObject(transition);
        so.FindProperty("sceneToLoad").intValue = buildIndex;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(boss);
        EditorSceneManager.SaveScene(boss);
    }

    /**
     * 移植したオブジェクトが持つ「Yami5F-3 側に残ったオブジェクト」への参照を、
     * Kaze3_boss3 内の同じヒエラルキーパスのオブジェクトへ張り替える。
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
                                + $"の参照先 '{path}' が Kaze3_boss3 に無いため解決できません。手動で設定してください。");
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
                }
                if (changed) so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }

    private static Component FindSameComponent(GameObject sourceGo, GameObject targetGo, Component sourceComponent)
    {
        var type = sourceComponent.GetType();
        var sourceComponents = sourceGo.GetComponents(type);
        int index = Array.IndexOf(sourceComponents, sourceComponent);
        var targetComponents = targetGo.GetComponents(type);
        if (index < 0 || index >= targetComponents.Length) return targetComponents.FirstOrDefault();
        return targetComponents[index];
    }

    private static GameObject FindRoot(Scene scene, string name)
    {
        return scene.GetRootGameObjects().FirstOrDefault(g => g.name == name);
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
