using Fungus;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VLCNP.Control;
using VLCNP.Core;

/**
 * Issue #641: イベント「VLファームに帰ってきた瞬間」のセットアップ。
 * Tools/Issue641/Setup Farm Return Event で実行する(シーンは自動で開く)。
 * - VeryLongFarm_1: 3人加入済み & ReturnedToVLFarm=false でロード直後に自動発動する会話イベントを追加
 * - VeryLongFarm_1: TutorialNPC を3人加入済みで非表示にする
 * - BlockChainRoom: UniGameEvent (3) を ReturnedToVLFarm で非表示にする
 * 再実行可能(既存の生成物は作り直し・設定済みならスキップ)。
 */
public static class Issue641FarmReturnEventSetup
{
    const string EventObjectName = "FarmReturnEvent";
    const string BlockName = "Return";
    const string FarmScenePath = "Assets/Scenes/VeryLongFarm_1.unity";
    const string BlockChainScenePath = "Assets/Scenes/BlockChainRoom.unity";
    const string AllFacesPrefabPath = "Assets/Game/Characters/Face/AllFaces.prefab";
    const string LeeleeCharacterPrefabPath = "Assets/Game/Characters/Face/LeeleeCharacter.prefab";

    [MenuItem("Tools/Issue641/Setup Farm Return Event")]
    public static void Setup()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        var farmScene = EditorSceneManager.OpenScene(FarmScenePath, OpenSceneMode.Single);
        SetupFarmReturnEvent(farmScene);
        SetupTutorialNpcVisibility(farmScene);
        EditorSceneManager.MarkSceneDirty(farmScene);
        EditorSceneManager.SaveScene(farmScene);

        var blockChainScene = EditorSceneManager.OpenScene(BlockChainScenePath, OpenSceneMode.Single);
        SetupUniGameEventVisibility(blockChainScene);
        EditorSceneManager.MarkSceneDirty(blockChainScene);
        EditorSceneManager.SaveScene(blockChainScene);

        Debug.Log("[Issue641] セットアップが完了しました。");
    }

    static void SetupFarmReturnEvent(UnityEngine.SceneManagement.Scene scene)
    {
        // 再実行できるよう前回の生成物を削除
        GameObject old = FindInScene(scene, EventObjectName);
        if (old != null) Object.DestroyImmediate(old);

        GameObject eventGo = new GameObject(EventObjectName);
        Flowchart flowchart = eventGo.AddComponent<Flowchart>();
        eventGo.AddComponent<FlowchartStopAllGuard>();
        MultiFlagGameEvent gameEvent = eventGo.AddComponent<MultiFlagGameEvent>();
        gameEvent.flowChart = flowchart;
        gameEvent.InformationText = "";

        // 会話キャラ(顔グラ)を用意
        Character leelee = EnsureCharacter(scene, eventGo, LeeleeCharacterPrefabPath, "LeeleeCharacter");
        GameObject allFacesGo = EnsurePrefabInstance(scene, eventGo, AllFacesPrefabPath, "AllFaces");
        Character orochi = FindCharacter(allFacesGo, "OrochiCharacter");
        Character mitama = FindCharacter(allFacesGo, "MitamaCharacter");
        Character narukami = FindCharacter(allFacesGo, "NarukamiCharacter");
        if (leelee == null || orochi == null || mitama == null || narukami == null)
        {
            Debug.LogError($"[Issue641] 会話キャラが見つかりません。leelee={leelee} orochi={orochi} mitama={mitama} narukami={narukami}");
            return;
        }
        SayDialog sayDialog = FindSceneSayDialog(scene);
        SetSayDialog(leelee, sayDialog);
        SetSayDialog(orochi, sayDialog);
        SetSayDialog(mitama, sayDialog);
        SetSayDialog(narukami, sayDialog);

        // 会話ブロック
        Block block = flowchart.CreateBlock(new Vector2(50, 50));
        block.BlockName = BlockName;
        AddSay(flowchart, block, leelee, "おっし、これで\nCNP全員集合ってわけや！");
        AddSay(flowchart, block, orochi, "{size=28}えぇと、誰かを殴るんだっけ？{/size}");
        AddSay(flowchart, block, leelee, "せや！ ワシらを封印した\nヤーマのやろうをガツンとやりにいくで！");
        AddSay(flowchart, block, mitama, "そんなに簡単に\nいくといいですがねえ...");
        AddSay(flowchart, block, narukami, "左様。敵を知り己を知れば百戦危うからず。\nまずは情報を集めることが肝要。");
        AddSay(flowchart, block, leelee, "ゴタゴタとうるさい奴らやのう。。。\nまあでも一理あるわな。\nいったんウニに報告しに行こか");
        AddSetFlag(flowchart, block, Flag.ReturnedToVLFarm, true);

        // 発動条件: 3人加入済み かつ 未帰還 で自動発動
        SerializedObject so = new SerializedObject(gameEvent);
        SerializedProperty list = so.FindProperty("flagConditionToBlockName");
        list.arraySize = 1;
        SerializedProperty element = list.GetArrayElementAtIndex(0);
        SerializedProperty conditions = element.FindPropertyRelative("conditions");
        conditions.arraySize = 4;
        SetCondition(conditions.GetArrayElementAtIndex(0), Flag.VLOrochiJoined, true);
        SetCondition(conditions.GetArrayElementAtIndex(1), Flag.VLMitamaJoined, true);
        SetCondition(conditions.GetArrayElementAtIndex(2), Flag.VLNarukamiJoined, true);
        SetCondition(conditions.GetArrayElementAtIndex(3), Flag.ReturnedToVLFarm, false);
        element.FindPropertyRelative("blockName").stringValue = BlockName;
        element.FindPropertyRelative("isAutoStart").boolValue = true;
        element.FindPropertyRelative("isCollisionStart").boolValue = false;
        so.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log("[Issue641] FarmReturnEvent を作成しました。");
    }

    static void SetupTutorialNpcVisibility(UnityEngine.SceneManagement.Scene scene)
    {
        GameObject npc = FindInScene(scene, "TutorialNPC");
        if (npc == null)
        {
            Debug.LogError("[Issue641] TutorialNPC が見つかりません。");
            return;
        }
        VisibilityFlagManager manager = npc.GetComponent<VisibilityFlagManager>();
        if (manager == null) manager = npc.AddComponent<VisibilityFlagManager>();
        SerializedObject so = new SerializedObject(manager);
        SerializedProperty list = so.FindProperty("visibilityFlags");
        // 既存の非表示エントリ(3人加入済みAND)があれば更新、なければ末尾に追加(後勝ち)
        int index = -1;
        for (int i = 0; i < list.arraySize; i++)
        {
            SerializedProperty entry = list.GetArrayElementAtIndex(i);
            if (entry.FindPropertyRelative("flag").intValue == (int)Flag.VLOrochiJoined
                && !entry.FindPropertyRelative("isVisible").boolValue)
            {
                index = i;
                break;
            }
        }
        if (index < 0)
        {
            index = list.arraySize;
            list.arraySize = index + 1;
        }
        SerializedProperty element = list.GetArrayElementAtIndex(index);
        element.FindPropertyRelative("flag").intValue = (int)Flag.VLOrochiJoined;
        SerializedProperty additional = element.FindPropertyRelative("additionalFlags");
        additional.arraySize = 2;
        additional.GetArrayElementAtIndex(0).intValue = (int)Flag.VLMitamaJoined;
        additional.GetArrayElementAtIndex(1).intValue = (int)Flag.VLNarukamiJoined;
        element.FindPropertyRelative("isVisible").boolValue = false;
        so.ApplyModifiedPropertiesWithoutUndo();
        Debug.Log("[Issue641] TutorialNPC に3人加入済み→非表示を設定しました。");
    }

    static void SetupUniGameEventVisibility(UnityEngine.SceneManagement.Scene scene)
    {
        GameObject target = FindInScene(scene, "UniGameEvent (3)");
        if (target == null)
        {
            Debug.LogError("[Issue641] UniGameEvent (3) が見つかりません。");
            return;
        }
        VisibilityFlagManager manager = target.GetComponent<VisibilityFlagManager>();
        if (manager == null) manager = target.AddComponent<VisibilityFlagManager>();
        SerializedObject so = new SerializedObject(manager);
        SerializedProperty list = so.FindProperty("visibilityFlags");
        for (int i = 0; i < list.arraySize; i++)
        {
            SerializedProperty entry = list.GetArrayElementAtIndex(i);
            if (entry.FindPropertyRelative("flag").intValue == (int)Flag.ReturnedToVLFarm
                && !entry.FindPropertyRelative("isVisible").boolValue)
            {
                Debug.Log("[Issue641] UniGameEvent (3) は設定済みです。");
                return;
            }
        }
        int index = list.arraySize;
        list.arraySize = index + 1;
        SerializedProperty element = list.GetArrayElementAtIndex(index);
        element.FindPropertyRelative("flag").intValue = (int)Flag.ReturnedToVLFarm;
        element.FindPropertyRelative("additionalFlags").arraySize = 0;
        element.FindPropertyRelative("isVisible").boolValue = false;
        so.ApplyModifiedPropertiesWithoutUndo();
        Debug.Log("[Issue641] UniGameEvent (3) に ReturnedToVLFarm→非表示を設定しました。");
    }

    static Character EnsureCharacter(UnityEngine.SceneManagement.Scene scene, GameObject parent, string prefabPath, string name)
    {
        GameObject go = EnsurePrefabInstance(scene, parent, prefabPath, name);
        return go != null ? go.GetComponentInChildren<Character>(true) : null;
    }

    static GameObject EnsurePrefabInstance(UnityEngine.SceneManagement.Scene scene, GameObject parent, string prefabPath, string name)
    {
        GameObject existing = FindInScene(scene, name);
        if (existing != null) return existing;
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[Issue641] プレハブが見つかりません: {prefabPath}");
            return null;
        }
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        instance.transform.SetParent(parent.transform, false);
        return instance;
    }

    static Character FindCharacter(GameObject root, string name)
    {
        if (root == null) return null;
        foreach (Character character in root.GetComponentsInChildren<Character>(true))
        {
            if (character.gameObject.name == name) return character;
        }
        return null;
    }

    static SayDialog FindSceneSayDialog(UnityEngine.SceneManagement.Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            SayDialog dialog = root.GetComponentInChildren<SayDialog>(true);
            if (dialog != null) return dialog;
        }
        return null;
    }

    static void SetSayDialog(Character character, SayDialog sayDialog)
    {
        if (character == null || sayDialog == null) return;
        SerializedObject so = new SerializedObject(character);
        so.FindProperty("setSayDialog").objectReferenceValue = sayDialog;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void AddSay(Flowchart flowchart, Block block, Character character, string storyText)
    {
        Say say = flowchart.gameObject.AddComponent<Say>();
        say.ItemId = flowchart.NextItemId();
        say.ParentBlock = block;
        SerializedObject so = new SerializedObject(say);
        so.FindProperty("storyText").stringValue = storyText;
        so.FindProperty("character").objectReferenceValue = character;
        Sprite portrait = character.Portraits != null && character.Portraits.Count > 0 ? character.Portraits[0] : null;
        so.FindProperty("portrait").objectReferenceValue = portrait;
        so.ApplyModifiedPropertiesWithoutUndo();
        block.CommandList.Add(say);
    }

    static void AddSetFlag(Flowchart flowchart, Block block, Flag flag, bool value)
    {
        FlagManagerSetFlagCommand command = flowchart.gameObject.AddComponent<FlagManagerSetFlagCommand>();
        command.ItemId = flowchart.NextItemId();
        command.ParentBlock = block;
        SerializedObject so = new SerializedObject(command);
        so.FindProperty("flag").intValue = (int)flag;
        so.FindProperty("value").boolValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
        block.CommandList.Add(command);
    }

    static void SetCondition(SerializedProperty condition, Flag flag, bool expectedValue)
    {
        condition.FindPropertyRelative("flag").intValue = (int)flag;
        condition.FindPropertyRelative("expectedValue").boolValue = expectedValue;
    }

    static GameObject FindInScene(UnityEngine.SceneManagement.Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == name) return root;
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name) return child.gameObject;
            }
        }
        return null;
    }

    // プレイモード検証用: 3人加入フラグを立てる
    [MenuItem("Tools/Issue641/Debug/Set All Joined Flags")]
    public static void DebugSetAllJoined()
    {
        FlagManager flagManager = Object.FindObjectOfType<FlagManager>();
        if (flagManager == null)
        {
            Debug.LogError("[Issue641] FlagManagerが見つかりません。");
            return;
        }
        flagManager.SetFlag(Flag.VLOrochiJoined, true);
        flagManager.SetFlag(Flag.VLMitamaJoined, true);
        flagManager.SetFlag(Flag.VLNarukamiJoined, true);
        Debug.Log("[Issue641] 3人の加入フラグをONにしました。");
    }
}
