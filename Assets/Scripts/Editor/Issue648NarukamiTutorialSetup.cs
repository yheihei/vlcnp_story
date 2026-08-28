using System.Collections.Generic;
using Fungus;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Video;
using VLCNP.Core;
using VLCNP.UI;
using Menu = Fungus.Menu;

/**
 * Issue #648: ウニキ選択メニューにVLナルカミのチュートリアル(スライド3枚)を追加する。
 * VeryLongFarm_1 を開いた状態で Tools/Issue648/Setup Narukami Tutorial を実行する。
 * 再実行可能(既存のNarukamiブロック・選択肢・条件は作り直す)。
 */
public static class Issue648NarukamiTutorialSetup
{
    const string NarukamiFacePath = "Assets/Game/Characters/Sprite/narukami.png";
    const string CarryVideoPath = "Assets/Game/Tutorial/NarukamiCarry.mp4";
    const string FlyVideoPath = "Assets/Game/Tutorial/NarukamiFly.mp4";

    [MenuItem("Tools/Issue648/Setup Narukami Tutorial")]
    public static void Setup()
    {
        // SampleSceneは#646のプロトタイプ環境。検証用に同じセットアップを許可する
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "VeryLongFarm_1" && scene.name != "SampleScene")
        {
            Debug.LogError("[Issue648] VeryLongFarm_1かSampleSceneを開いた状態で実行してください。現在: " + scene.name);
            return;
        }

        GameObject chatGo = FindInScene(scene, "UnikiTutorialChat");
        GameObject npcGo = FindInScene(scene, "UnikiTutorialNPC");
        if (chatGo == null || npcGo == null)
        {
            Debug.LogError("[Issue648] UnikiTutorialChat / UnikiTutorialNPC が見つかりません。");
            return;
        }
        Flowchart flowchart = chatGo.GetComponent<Flowchart>();

        Block messageBlock = flowchart.FindBlock("Message");
        Block orochiBlock = flowchart.FindBlock("Orochi");
        if (messageBlock == null || orochiBlock == null)
        {
            Debug.LogError("[Issue648] Message / Orochi ブロックが見つかりません。");
            return;
        }

        // 既存のOrochiブロックからキャラ参照・スライドUI参照をコピーする
        Say templateSay = null;
        ShowTutorialSlidesCommand templateSlides = null;
        foreach (Command command in orochiBlock.CommandList)
        {
            if (templateSay == null && command is Say say) templateSay = say;
            if (templateSlides == null && command is ShowTutorialSlidesCommand slides) templateSlides = slides;
        }
        if (templateSay == null || templateSlides == null)
        {
            Debug.LogError("[Issue648] OrochiブロックにSay/ShowTutorialSlidesが見つかりません。");
            return;
        }
        SerializedObject templateSaySo = new SerializedObject(templateSay);
        Object character = templateSaySo.FindProperty("character").objectReferenceValue;
        Object portrait = templateSaySo.FindProperty("portrait").objectReferenceValue;
        SerializedObject templateSlidesSo = new SerializedObject(templateSlides);
        Object slideShow = templateSlidesSo.FindProperty("slideShow").objectReferenceValue;

        // 再実行できるよう前回の生成物を削除
        RemoveOldNarukamiSetup(flowchart, messageBlock);

        // 素材ロード
        Sprite narukamiFace = LoadFirstSprite(NarukamiFacePath);
        VideoClip carryVideo = AssetDatabase.LoadAssetAtPath<VideoClip>(CarryVideoPath);
        VideoClip flyVideo = AssetDatabase.LoadAssetAtPath<VideoClip>(FlyVideoPath);
        if (narukamiFace == null || carryVideo == null || flyVideo == null)
        {
            Debug.LogError($"[Issue648] 素材が見つかりません。face={narukamiFace} carry={carryVideo} fly={flyVideo}");
            return;
        }

        // Narukamiブロック
        Block narukamiBlock = flowchart.CreateBlock(new Vector2(200, 250));
        narukamiBlock.BlockName = "Narukami";
        AddSay(flowchart, narukamiBlock, "VLナルカミだね\nこの人の力でみんなが空を飛べるんだ", character, portrait);
        AddTutorialSlides(flowchart, narukamiBlock, slideShow, new List<SlideData>
        {
            new SlideData { sprite = narukamiFace, title = "VLナルカミ", caption = "みんなに 飛行の力を さずける CNP" },
            new SlideData { video = carryVideo, title = "飛行", caption = "ナルカミの力で みんなが 空を飛べるようになる" },
            new SlideData { video = flyVideo, title = "飛行の操作", caption = "空中で 方向キーを押しながら ジャンプボタンで 飛行" },
        });
        AddSay(flowchart, narukamiBlock, "ナルカミ本人は操作できないから\n気をつけてね", character, portrait);

        // Messageブロックの「やめとく」の直前に選択肢を挿入
        int insertIndex = messageBlock.CommandList.FindIndex(c => c is Menu && !(c is FlagMenuCommand));
        if (insertIndex < 0) insertIndex = messageBlock.CommandList.Count;
        InsertFlagMenu(flowchart, messageBlock, insertIndex, "VLナルカミ", narukamiBlock, Flag.VLNarukamiJoined);

        // NPCのMultiFlagGameEventにVLNarukamiJoined→Messageの条件を追加
        AddMultiFlagCondition(npcGo, Flag.VLNarukamiJoined, "Message");

        // 可視条件の確認(#646でVLNarukamiJoined→表示は設定済みのはず)
        CheckVisibilityFlag(npcGo, Flag.VLNarukamiJoined);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Issue648] VLナルカミのチュートリアルを追加しました。");
    }

    // プレイモード検証用: ナルカミ加入フラグを立てる
    [MenuItem("Tools/Issue648/Debug/Set Narukami Joined Flag")]
    public static void DebugSetNarukamiJoined()
    {
        FlagManager flagManager = Object.FindObjectOfType<FlagManager>();
        if (flagManager == null)
        {
            Debug.LogError("[Issue648] FlagManagerが見つかりません。");
            return;
        }
        flagManager.SetFlag(Flag.VLNarukamiJoined, true);
        Debug.Log("[Issue648] VLNarukamiJoinedをONにしました。");
    }

    // プレイモード検証用: ウニキNPCの会話イベントを起動する(話しかけ相当)
    [MenuItem("Tools/Issue648/Debug/Start Uniki Event")]
    public static void DebugStartUnikiEvent()
    {
        GameObject npc = GameObject.Find("UnikiTutorialNPC");
        if (npc == null)
        {
            Debug.LogError("[Issue648] UnikiTutorialNPCが見つかりません。");
            return;
        }
        npc.GetComponent<MultiFlagGameEvent>().Execute();
        Debug.Log("[Issue648] MultiFlagGameEvent.Executeを呼び出しました。");
    }

    static void RemoveOldNarukamiSetup(Flowchart flowchart, Block messageBlock)
    {
        for (int i = messageBlock.CommandList.Count - 1; i >= 0; i--)
        {
            if (messageBlock.CommandList[i] is FlagMenuCommand menu)
            {
                SerializedObject so = new SerializedObject(menu);
                if (so.FindProperty("text").stringValue == "VLナルカミ")
                {
                    messageBlock.CommandList.RemoveAt(i);
                    Object.DestroyImmediate(menu);
                }
            }
        }
        Block oldBlock = flowchart.FindBlock("Narukami");
        if (oldBlock == null) return;
        foreach (Command command in oldBlock.CommandList)
        {
            if (command != null) Object.DestroyImmediate(command);
        }
        Object.DestroyImmediate(oldBlock);
    }

    static void AddMultiFlagCondition(GameObject npcGo, Flag flag, string blockName)
    {
        MultiFlagGameEvent gameEvent = npcGo.GetComponent<MultiFlagGameEvent>();
        if (gameEvent == null)
        {
            Debug.LogWarning("[Issue648] UnikiTutorialNPCにMultiFlagGameEventがありません。スキップします(SampleSceneではChat駆動)。");
            return;
        }
        foreach (var entry in gameEvent.flagConditionToBlockName)
        {
            if (entry != null && entry.ContainsFlag(flag))
            {
                Debug.Log($"[Issue648] MultiFlagGameEventには既に{flag}の条件があります。スキップします。");
                return;
            }
        }
        SerializedObject so = new SerializedObject(gameEvent);
        SerializedProperty list = so.FindProperty("flagConditionToBlockName");
        int index = list.arraySize;
        list.arraySize = index + 1;
        SerializedProperty element = list.GetArrayElementAtIndex(index);
        SerializedProperty conditions = element.FindPropertyRelative("conditions");
        conditions.arraySize = 1;
        SerializedProperty condition = conditions.GetArrayElementAtIndex(0);
        condition.FindPropertyRelative("flag").intValue = (int)flag;
        condition.FindPropertyRelative("expectedValue").boolValue = true;
        element.FindPropertyRelative("blockName").stringValue = blockName;
        element.FindPropertyRelative("isAutoStart").boolValue = false;
        element.FindPropertyRelative("isCollisionStart").boolValue = false;
        so.ApplyModifiedPropertiesWithoutUndo();
        Debug.Log($"[Issue648] MultiFlagGameEventに条件を追加しました: {flag} -> {blockName}");
    }

    static void CheckVisibilityFlag(GameObject npcGo, Flag flag)
    {
        VisibilityFlagManager manager = npcGo.GetComponent<VisibilityFlagManager>();
        if (manager == null)
        {
            Debug.LogWarning("[Issue648] UnikiTutorialNPCにVisibilityFlagManagerがありません。");
            return;
        }
        SerializedObject so = new SerializedObject(manager);
        SerializedProperty list = so.FindProperty("visibilityFlags");
        for (int i = 0; i < list.arraySize; i++)
        {
            SerializedProperty element = list.GetArrayElementAtIndex(i);
            if (element.FindPropertyRelative("flag").intValue == (int)flag
                && element.FindPropertyRelative("isVisible").boolValue)
            {
                Debug.Log($"[Issue648] VisibilityFlagManagerに{flag}→表示は設定済みです。");
                return;
            }
        }
        Debug.LogWarning($"[Issue648] VisibilityFlagManagerに{flag}→表示がありません。手動確認が必要です。");
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

    class SlideData
    {
        public VideoClip video;
        public Sprite sprite;
        public string title;
        public string caption;
    }

    static void AddSay(Flowchart flowchart, Block block, string storyText, Object character, Object portrait)
    {
        Say say = flowchart.gameObject.AddComponent<Say>();
        say.ItemId = flowchart.NextItemId();
        say.ParentBlock = block;
        SerializedObject so = new SerializedObject(say);
        so.FindProperty("storyText").stringValue = storyText;
        so.FindProperty("character").objectReferenceValue = character;
        so.FindProperty("portrait").objectReferenceValue = portrait;
        so.ApplyModifiedPropertiesWithoutUndo();
        block.CommandList.Add(say);
    }

    static void InsertFlagMenu(Flowchart flowchart, Block block, int index, string text, Block targetBlock, Flag requiredFlag)
    {
        FlagMenuCommand menu = flowchart.gameObject.AddComponent<FlagMenuCommand>();
        menu.ItemId = flowchart.NextItemId();
        menu.ParentBlock = block;
        SerializedObject so = new SerializedObject(menu);
        so.FindProperty("text").stringValue = text;
        so.FindProperty("targetBlock").objectReferenceValue = targetBlock;
        so.FindProperty("requiredFlag").intValue = (int)requiredFlag;
        so.ApplyModifiedPropertiesWithoutUndo();
        block.CommandList.Insert(index, menu);
    }

    static void AddTutorialSlides(Flowchart flowchart, Block block, Object slideShow, List<SlideData> slides)
    {
        ShowTutorialSlidesCommand command = flowchart.gameObject.AddComponent<ShowTutorialSlidesCommand>();
        command.ItemId = flowchart.NextItemId();
        command.ParentBlock = block;
        SerializedObject so = new SerializedObject(command);
        so.FindProperty("slideShow").objectReferenceValue = slideShow;
        SerializedProperty slidesProperty = so.FindProperty("slides");
        slidesProperty.arraySize = slides.Count;
        for (int i = 0; i < slides.Count; i++)
        {
            SerializedProperty element = slidesProperty.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("videoClip").objectReferenceValue = slides[i].video;
            element.FindPropertyRelative("sprite").objectReferenceValue = slides[i].sprite;
            element.FindPropertyRelative("title").stringValue = slides[i].title;
            element.FindPropertyRelative("caption").stringValue = slides[i].caption;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        block.CommandList.Add(command);
    }

    static Sprite LoadFirstSprite(string path)
    {
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            if (asset is Sprite sprite) return sprite;
        }
        return null;
    }
}
