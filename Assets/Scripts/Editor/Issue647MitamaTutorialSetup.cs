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
 * Issue #647: ウニキ選択メニューにVLミタマのチュートリアル(スライド3枚)を追加する。
 * VeryLongFarm_1 を開いた状態で Tools/Issue647/Setup Mitama Tutorial を実行する。
 * 再実行可能(既存のMitamaブロック・選択肢・条件は作り直す)。
 */
public static class Issue647MitamaTutorialSetup
{
    const string MitamaFacePath = "Assets/Game/Characters/Sprite/VLMitamaFace.png";
    const string OnibiVideoPath = "Assets/Game/Tutorial/MitamaOnibi.mp4";
    const string FloatVideoPath = "Assets/Game/Tutorial/MitamaFloat.mp4";

    [MenuItem("Tools/Issue647/Setup Mitama Tutorial")]
    public static void Setup()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "VeryLongFarm_1")
        {
            Debug.LogError("[Issue647] VeryLongFarm_1を開いた状態で実行してください。現在: " + scene.name);
            return;
        }

        GameObject chatGo = FindInScene(scene, "UnikiTutorialChat");
        GameObject npcGo = FindInScene(scene, "UnikiTutorialNPC");
        if (chatGo == null || npcGo == null)
        {
            Debug.LogError("[Issue647] UnikiTutorialChat / UnikiTutorialNPC が見つかりません。");
            return;
        }
        Flowchart flowchart = chatGo.GetComponent<Flowchart>();

        Block messageBlock = flowchart.FindBlock("Message");
        Block orochiBlock = flowchart.FindBlock("Orochi");
        if (messageBlock == null || orochiBlock == null)
        {
            Debug.LogError("[Issue647] Message / Orochi ブロックが見つかりません。");
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
            Debug.LogError("[Issue647] OrochiブロックにSay/ShowTutorialSlidesが見つかりません。");
            return;
        }
        SerializedObject templateSaySo = new SerializedObject(templateSay);
        Object character = templateSaySo.FindProperty("character").objectReferenceValue;
        Object portrait = templateSaySo.FindProperty("portrait").objectReferenceValue;
        SerializedObject templateSlidesSo = new SerializedObject(templateSlides);
        Object slideShow = templateSlidesSo.FindProperty("slideShow").objectReferenceValue;

        // 再実行できるよう前回の生成物を削除
        RemoveOldMitamaSetup(flowchart, messageBlock);

        // 素材ロード
        Sprite mitamaFace = LoadFirstSprite(MitamaFacePath);
        VideoClip onibiVideo = AssetDatabase.LoadAssetAtPath<VideoClip>(OnibiVideoPath);
        VideoClip floatVideo = AssetDatabase.LoadAssetAtPath<VideoClip>(FloatVideoPath);
        if (mitamaFace == null || onibiVideo == null || floatVideo == null)
        {
            Debug.LogError($"[Issue647] 素材が見つかりません。face={mitamaFace} onibi={onibiVideo} float={floatVideo}");
            return;
        }

        // Mitamaブロック
        Block mitamaBlock = flowchart.CreateBlock(new Vector2(200, 150));
        mitamaBlock.BlockName = "Mitama";
        AddSay(flowchart, mitamaBlock, "VLミタマだね\nこの人は不思議な力を使うんだ", character, portrait);
        AddTutorialSlides(flowchart, mitamaBlock, slideShow, new List<SlideData>
        {
            new SlideData { sprite = mitamaFace, title = "VLミタマ", caption = "鬼火と 浮遊が とくいな CNP" },
            new SlideData { video = onibiVideo, title = "鬼火", caption = "攻撃ボタンで 鬼火を展開 方向を決めて 発射" },
            new SlideData { video = floatVideo, title = "浮遊", caption = "ジャンプ中に ジャンプボタンを押しっぱなしで しばらく浮ける" },
        });
        AddSay(flowchart, mitamaBlock, "君に使いこなせるかなあ...？", character, portrait);

        // Messageブロックの「やめとく」の直前に選択肢を挿入
        int insertIndex = messageBlock.CommandList.FindIndex(c => c is Menu && !(c is FlagMenuCommand));
        if (insertIndex < 0) insertIndex = messageBlock.CommandList.Count;
        InsertFlagMenu(flowchart, messageBlock, insertIndex, "VLミタマ", mitamaBlock, Flag.VLMitamaJoined);

        // NPCのMultiFlagGameEventにVLMitamaJoined→Messageの条件を追加
        AddMultiFlagCondition(npcGo, Flag.VLMitamaJoined, "Message");

        // 可視条件の確認(#646でVLMitamaJoined→表示は設定済みのはず)
        CheckVisibilityFlag(npcGo, Flag.VLMitamaJoined);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Issue647] VLミタマのチュートリアルを追加しました。");
    }

    static void RemoveOldMitamaSetup(Flowchart flowchart, Block messageBlock)
    {
        for (int i = messageBlock.CommandList.Count - 1; i >= 0; i--)
        {
            if (messageBlock.CommandList[i] is FlagMenuCommand menu)
            {
                SerializedObject so = new SerializedObject(menu);
                if (so.FindProperty("text").stringValue == "VLミタマ")
                {
                    messageBlock.CommandList.RemoveAt(i);
                    Object.DestroyImmediate(menu);
                }
            }
        }
        Block oldBlock = flowchart.FindBlock("Mitama");
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
            Debug.LogError("[Issue647] UnikiTutorialNPCにMultiFlagGameEventがありません。");
            return;
        }
        foreach (var entry in gameEvent.flagConditionToBlockName)
        {
            if (entry != null && entry.ContainsFlag(flag))
            {
                Debug.Log($"[Issue647] MultiFlagGameEventには既に{flag}の条件があります。スキップします。");
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
        Debug.Log($"[Issue647] MultiFlagGameEventに条件を追加しました: {flag} -> {blockName}");
    }

    static void CheckVisibilityFlag(GameObject npcGo, Flag flag)
    {
        VisibilityFlagManager manager = npcGo.GetComponent<VisibilityFlagManager>();
        if (manager == null)
        {
            Debug.LogWarning("[Issue647] UnikiTutorialNPCにVisibilityFlagManagerがありません。");
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
                Debug.Log($"[Issue647] VisibilityFlagManagerに{flag}→表示は設定済みです。");
                return;
            }
        }
        Debug.LogWarning($"[Issue647] VisibilityFlagManagerに{flag}→表示がありません。手動確認が必要です。");
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
