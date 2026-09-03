using System.Collections.Generic;
using Fungus;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using VLCNP.Actions;
using VLCNP.Core;
using VLCNP.DebugSctipt;
using VLCNP.UI;
using Menu = Fungus.Menu;

/**
 * Issue #646: ウニキのチュートリアルNPC(選択メニュー+スライド表示)のセットアップ用ユーティリティ。
 *   1. Tools/Issue646/Build TutorialSlideShow Prefab — スライドUIプレハブを生成
 *   2. Tools/Issue646/Setup SampleScene — SampleSceneへウニキNPC・会話・デバッグフラグを配置
 */
public static class Issue646TutorialSetup
{
    const string RenderTexturePath = "Assets/Game/Tutorial/TutorialSlideRT.renderTexture";
    const string PrefabPath = "Assets/Game/UI/TutorialSlideShow.prefab";
    const string FontBlackPath = "Assets/Game/Fonts/NotoSansJP-Black.ttf";
    const string FontRegularPath = "Assets/Game/Fonts/NotoSansJP-Regular.ttf";
    const string NpcPrefabPath = "Assets/Game/Characters/Npc/NPC.prefab";
    const string UnikiCharacterPrefabPath = "Assets/Game/Characters/Face/UniCharacterWithFace.prefab";
    const string UnikiSpritePath = "Assets/Game/Characters/Unikisp.png";
    const string OrochiFacePath = "Assets/Game/Characters/Sprite/OrochiFace.png";
    const string PoisonBallVideoPath = "Assets/Game/Tutorial/OrochiPoisonBall.mp4";
    const string PoisonStatusVideoPath = "Assets/Game/Tutorial/OrochiPoisonStatus.mp4";

    [MenuItem("Tools/Issue646/Build TutorialSlideShow Prefab")]
    public static void BuildPrefab()
    {
        RenderTexture rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(RenderTexturePath);
        if (rt == null)
        {
            rt = new RenderTexture(640, 360, 0);
            AssetDatabase.CreateAsset(rt, RenderTexturePath);
        }

        Font fontBlack = AssetDatabase.LoadAssetAtPath<Font>(FontBlackPath);
        Font fontRegular = AssetDatabase.LoadAssetAtPath<Font>(FontRegularPath);

        GameObject root = new GameObject("TutorialSlideShow");
        try
        {
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            VideoPlayer videoPlayer = root.AddComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = rt;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            videoPlayer.isLooping = true;

            GameObject panel = CreateUIObject("Panel", root.transform);
            Stretch(panel);
            Image dim = panel.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.7f);

            GameObject window = CreateUIObject("Window", panel.transform);
            SetRect(window, Vector2.zero, new Vector2(1240, 840));
            Image windowImage = window.AddComponent<Image>();
            windowImage.color = new Color(0.07f, 0.08f, 0.12f, 0.98f);

            Text title = CreateText("Title", window.transform, fontBlack, 60, TextAnchor.MiddleCenter);
            SetRect(title.gameObject, new Vector2(0, 350), new Vector2(1100, 80));
            title.color = Color.white;
            title.text = "タイトル";

            GameObject accent = CreateUIObject("TitleAccent", window.transform);
            SetRect(accent, new Vector2(0, 305), new Vector2(240, 6));
            Image accentImage = accent.AddComponent<Image>();
            accentImage.color = new Color(0.9f, 0.3f, 0.3f, 1f);

            GameObject mediaFrame = CreateUIObject("MediaFrame", window.transform);
            SetRect(mediaFrame, new Vector2(0, 10), new Vector2(960, 540));
            Image frameImage = mediaFrame.AddComponent<Image>();
            frameImage.color = Color.black;

            GameObject videoImageGo = CreateUIObject("VideoImage", mediaFrame.transform);
            Stretch(videoImageGo);
            RawImage videoImage = videoImageGo.AddComponent<RawImage>();
            videoImage.texture = rt;

            GameObject spriteImageGo = CreateUIObject("SpriteImage", mediaFrame.transform);
            Stretch(spriteImageGo);
            Image spriteImage = spriteImageGo.AddComponent<Image>();
            spriteImage.preserveAspect = true;

            Text caption = CreateText("Caption", window.transform, fontRegular, 38, TextAnchor.MiddleCenter);
            SetRect(caption.gameObject, new Vector2(0, -330), new Vector2(1100, 110));
            caption.color = Color.white;
            caption.text = "説明";

            Text page = CreateText("Page", window.transform, fontRegular, 30, TextAnchor.MiddleCenter);
            SetRect(page.gameObject, new Vector2(0, -395), new Vector2(300, 40));
            page.color = new Color(0.7f, 0.7f, 0.75f, 1f);
            page.text = "1 / 3";

            Text guide = CreateText("Guide", window.transform, fontRegular, 26, TextAnchor.MiddleRight);
            SetRect(guide.gameObject, new Vector2(440, -395), new Vector2(320, 40));
            guide.color = new Color(0.7f, 0.7f, 0.75f, 1f);
            guide.text = "決定: つぎへ";

            Text leftArrow = CreateText("LeftArrow", window.transform, fontRegular, 70, TextAnchor.MiddleCenter);
            SetRect(leftArrow.gameObject, new Vector2(-560, 10), new Vector2(90, 90));
            leftArrow.color = new Color(1f, 0.85f, 0.3f, 1f);
            leftArrow.text = "◀";

            Text rightArrow = CreateText("RightArrow", window.transform, fontRegular, 70, TextAnchor.MiddleCenter);
            SetRect(rightArrow.gameObject, new Vector2(560, 10), new Vector2(90, 90));
            rightArrow.color = new Color(1f, 0.85f, 0.3f, 1f);
            rightArrow.text = "▶";

            TutorialSlideShow slideShow = root.AddComponent<TutorialSlideShow>();
            SerializedObject so = new SerializedObject(slideShow);
            so.FindProperty("panelRoot").objectReferenceValue = panel;
            so.FindProperty("videoPlayer").objectReferenceValue = videoPlayer;
            so.FindProperty("videoImage").objectReferenceValue = videoImage;
            so.FindProperty("spriteImage").objectReferenceValue = spriteImage;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("captionText").objectReferenceValue = caption;
            so.FindProperty("pageText").objectReferenceValue = page;
            so.FindProperty("guideText").objectReferenceValue = guide;
            so.FindProperty("leftArrow").objectReferenceValue = leftArrow.gameObject;
            so.FindProperty("rightArrow").objectReferenceValue = rightArrow.gameObject;
            so.ApplyModifiedPropertiesWithoutUndo();

            panel.SetActive(false);

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Debug.Log($"[Issue646] プレハブを保存しました: {PrefabPath}");
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/Issue646/Setup SampleScene")]
    public static void SetupSampleScene()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "SampleScene")
        {
            Debug.LogError("[Issue646] SampleSceneを開いた状態で実行してください。現在: " + scene.name);
            return;
        }

        // 再実行できるよう前回の生成物を削除
        foreach (string name in new[] { "TutorialSlideShow", "UnikiTutorialNPC", "UnikiTutorialChat", "UnikiTutorialCharacter", "Issue646DebugFlag" })
        {
            GameObject old = GameObject.Find(name);
            if (old == null)
            {
                foreach (GameObject rootGo in scene.GetRootGameObjects())
                {
                    if (rootGo.name == name) { old = rootGo; break; }
                }
            }
            if (old != null) Object.DestroyImmediate(old);
        }

        GameObject slideShowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (slideShowPrefab == null)
        {
            Debug.LogError("[Issue646] 先に Build TutorialSlideShow Prefab を実行してください。");
            return;
        }
        GameObject slideShowGo = (GameObject)PrefabUtility.InstantiatePrefab(slideShowPrefab);
        TutorialSlideShow slideShow = slideShowGo.GetComponent<TutorialSlideShow>();

        // ウニキのFungus Character
        GameObject characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(UnikiCharacterPrefabPath);
        GameObject characterGo = (GameObject)PrefabUtility.InstantiatePrefab(characterPrefab);
        characterGo.name = "UnikiTutorialCharacter";
        Character unikiCharacter = characterGo.GetComponentInChildren<Character>();
        Sprite unikiPortrait = (unikiCharacter != null && unikiCharacter.Portraits != null && unikiCharacter.Portraits.Count > 0)
            ? unikiCharacter.Portraits[0] : null;

        // 会話Flowchart
        GameObject chatGo = new GameObject("UnikiTutorialChat");
        Flowchart flowchart = chatGo.AddComponent<Flowchart>();

        Block messageBlock = flowchart.CreateBlock(new Vector2(50, 50));
        messageBlock.BlockName = "Message";
        Block orochiBlock = flowchart.CreateBlock(new Vector2(50, 150));
        orochiBlock.BlockName = "Orochi";
        Block endBlock = flowchart.CreateBlock(new Vector2(50, 250));
        endBlock.BlockName = "End";

        // Message: 導入 → 選択メニュー
        AddSay(flowchart, messageBlock, "CNPを なかまに できたみたいだね", unikiCharacter, unikiPortrait);
        AddSay(flowchart, messageBlock, "それぞれの CNPの とくちょうを しりたいかい？ おしえるよ", unikiCharacter, unikiPortrait);
        AddFlagMenu(flowchart, messageBlock, "VLオロチ", orochiBlock, Flag.VLOrochiJoined);
        AddMenu(flowchart, messageBlock, "やめとく", endBlock);

        // Orochi: スライド表示
        AddSay(flowchart, orochiBlock, "VLオロチの とくちょうを おしえるよ", unikiCharacter, unikiPortrait);
        VideoClip poisonBall = AssetDatabase.LoadAssetAtPath<VideoClip>(PoisonBallVideoPath);
        VideoClip poisonStatus = AssetDatabase.LoadAssetAtPath<VideoClip>(PoisonStatusVideoPath);
        Sprite orochiFace = AssetDatabase.LoadAssetAtPath<Sprite>(OrochiFacePath);
        AddTutorialSlides(flowchart, orochiBlock, slideShow, new List<SlideData>
        {
            new SlideData { sprite = orochiFace, title = "VLオロチ", caption = "どく攻撃が とくいな CNP" },
            new SlideData { video = poisonBall, title = "ポイズンボール", caption = "攻撃ボタンで どくの弾を はく" },
            new SlideData { video = poisonStatus, title = "どく状態", caption = "どくになった敵は 移動速度が 半減する" },
        });
        AddSay(flowchart, orochiBlock, "つかいこなして あげてね", unikiCharacter, unikiPortrait);

        // End
        AddSay(flowchart, endBlock, "しりたくなったら また こえを かけてね", unikiCharacter, unikiPortrait);

        // ウニキNPC本体
        GameObject npcPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(NpcPrefabPath);
        GameObject npc = (GameObject)PrefabUtility.InstantiatePrefab(npcPrefab);
        npc.name = "UnikiTutorialNPC";
        Sprite unikiSprite = LoadFirstSprite(UnikiSpritePath);
        SpriteRenderer renderer = npc.GetComponent<SpriteRenderer>();
        renderer.sprite = unikiSprite;
        renderer.drawMode = SpriteDrawMode.Simple;
        Chat chat = npc.GetComponent<Chat>();
        chat.flowChart = flowchart;
        chat.BlockName = "Message";
        chat.InformationText = "(↑) はなす";

        GameObject player = GameObject.FindWithTag("Player");
        Vector3 basePosition = player != null ? player.transform.position : Vector3.zero;
        npc.transform.position = basePosition + new Vector3(3f, 0f, 0f);
        chatGo.transform.position = npc.transform.position;

        // 検証用: オロチ加入フラグを立てるデバッグオブジェクト
        GameObject debugGo = new GameObject("Issue646DebugFlag");
        DebugFlagManager debugFlagManager = debugGo.AddComponent<DebugFlagManager>();
        debugFlagManager.trueFlags = new[] { Flag.VLOrochiJoined };

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Issue646] SampleSceneのセットアップが完了しました。");
    }

    // ---- ここからプレイモード検証用のデバッグメニュー ----

    [MenuItem("Tools/Issue646/Debug/Start Chat")]
    public static void DebugStartChat()
    {
        GameObject npc = GameObject.Find("UnikiTutorialNPC");
        if (npc == null) { Debug.LogError("[Issue646] UnikiTutorialNPCが見つかりません"); return; }
        npc.GetComponent<Chat>().Execute();
        Debug.Log("[Issue646] Chat.Executeを呼び出しました");
    }

    [MenuItem("Tools/Issue646/Debug/Advance Dialog")]
    public static void DebugAdvanceDialog()
    {
        DialogInput input = Object.FindObjectOfType<DialogInput>();
        if (input == null) { Debug.Log("[Issue646] DialogInputが見つかりません(会話枠が出ていない)"); return; }
        input.SetNextLineFlag();
        Debug.Log("[Issue646] SetNextLineFlagを呼び出しました");
    }

    [MenuItem("Tools/Issue646/Debug/Select Option 1")]
    public static void DebugSelectOption1() { DebugSelectOption(0); }

    [MenuItem("Tools/Issue646/Debug/Select Option 2")]
    public static void DebugSelectOption2() { DebugSelectOption(1); }

    static void DebugSelectOption(int index)
    {
        MenuDialog menuDialog = MenuDialog.ActiveMenuDialog;
        if (menuDialog == null || !menuDialog.gameObject.activeSelf)
        {
            Debug.LogError("[Issue646] アクティブなMenuDialogがありません");
            return;
        }
        var buttons = new List<Button>();
        foreach (Button button in menuDialog.CachedButtons)
        {
            if (button != null && button.gameObject.activeSelf) buttons.Add(button);
        }
        if (index >= buttons.Count)
        {
            Debug.LogError($"[Issue646] 選択肢{index + 1}がありません(表示数: {buttons.Count})");
            return;
        }
        Text label = buttons[index].GetComponentInChildren<Text>();
        Debug.Log($"[Issue646] 選択肢を選びます: {(label != null ? label.text : "?")}");
        buttons[index].onClick.Invoke();
    }

    [MenuItem("Tools/Issue646/Debug/Slide Next")]
    public static void DebugSlideNext()
    {
        TutorialSlideShow slideShow = Object.FindObjectOfType<TutorialSlideShow>();
        if (slideShow == null || !slideShow.IsOpen) { Debug.LogError("[Issue646] スライドが開いていません"); return; }
        var type = typeof(TutorialSlideShow);
        var indexField = type.GetField("currentIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var slidesField = type.GetField("slides", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var showMethod = type.GetMethod("ShowSlide", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        int current = (int)indexField.GetValue(slideShow);
        var slides = (IList<TutorialSlide>)slidesField.GetValue(slideShow);
        if (current >= slides.Count - 1) { Debug.Log("[Issue646] 最終スライドです"); return; }
        indexField.SetValue(slideShow, current + 1);
        showMethod.Invoke(slideShow, new object[] { current + 1 });
        Debug.Log($"[Issue646] スライドを進めました: {current + 2}枚目");
    }

    [MenuItem("Tools/Issue646/Debug/Slide Close")]
    public static void DebugSlideClose()
    {
        TutorialSlideShow slideShow = Object.FindObjectOfType<TutorialSlideShow>();
        if (slideShow == null || !slideShow.IsOpen) { Debug.LogError("[Issue646] スライドが開いていません"); return; }
        var closeMethod = typeof(TutorialSlideShow).GetMethod("Close", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        closeMethod.Invoke(slideShow, null);
        Debug.Log("[Issue646] スライドを閉じました");
    }

    class SlideData
    {
        public VideoClip video;
        public Sprite sprite;
        public string title;
        public string caption;
    }

    static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void Stretch(GameObject go)
    {
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static void SetRect(GameObject go, Vector2 anchoredPosition, Vector2 size)
    {
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    static Text CreateText(string name, Transform parent, Font font, int fontSize, TextAnchor anchor)
    {
        GameObject go = CreateUIObject(name, parent);
        Text text = go.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    static void AddSay(Flowchart flowchart, Block block, string storyText, Character character, Sprite portrait)
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

    static void AddMenu(Flowchart flowchart, Block block, string text, Block targetBlock)
    {
        Menu menu = flowchart.gameObject.AddComponent<Menu>();
        menu.ItemId = flowchart.NextItemId();
        menu.ParentBlock = block;
        SetMenuProperties(menu, text, targetBlock);
        block.CommandList.Add(menu);
    }

    static void AddFlagMenu(Flowchart flowchart, Block block, string text, Block targetBlock, Flag requiredFlag)
    {
        FlagMenuCommand menu = flowchart.gameObject.AddComponent<FlagMenuCommand>();
        menu.ItemId = flowchart.NextItemId();
        menu.ParentBlock = block;
        SetMenuProperties(menu, text, targetBlock);
        SerializedObject so = new SerializedObject(menu);
        so.FindProperty("requiredFlag").intValue = (int)requiredFlag;
        so.ApplyModifiedPropertiesWithoutUndo();
        block.CommandList.Add(menu);
    }

    static void SetMenuProperties(Menu menu, string text, Block targetBlock)
    {
        SerializedObject so = new SerializedObject(menu);
        so.FindProperty("text").stringValue = text;
        so.FindProperty("targetBlock").objectReferenceValue = targetBlock;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void AddTutorialSlides(Flowchart flowchart, Block block, TutorialSlideShow slideShow, List<SlideData> slides)
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
