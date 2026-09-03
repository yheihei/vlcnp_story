using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Fungus;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VLCNP.Control;
using VLCNP.Core;
using VLCNP.Movie;

/**
 * Issue #641: イベント「VLヤーマ襲来&VLCNPチーム全員ごみ捨て部屋に落とされる」のセットアップ。
 * Tools/Issue641/Setup Yama Revenge Scene で実行する。
 * - VeryLongFarm_1_boss を複製して VeryLongFarm_Yama_Revenge を作る(既にあれば複製しない)
 * - Build Settings 末尾に追加する(途中挿入は既存の sceneToLoad がずれるため禁止)
 * - AllFaces に「カルマ」の顔グラ Character を追加する
 * - ボス戦の残り物(シモーヌ関連、旧 StartEvent 等)を削除し、NPC を配置し直す
 * - シーン遷移直後に自動発動する会話 Flowchart(YamaRevengeEvent)を構築する
 * 再実行可能(生成物は毎回作り直す。NPC の位置もこのスクリプトが正)。
 */
public static class Issue641YamaRevengeSetup
{
    public const string ScenePath = "Assets/Scenes/VeryLongFarm_Yama_Revenge.unity";
    const string SourceScenePath = "Assets/Scenes/VeryLongFarm_1_boss.unity";
    const string EventObjectName = "YamaRevengeEvent";
    const string BlockName = "Revenge";
    const string AllFacesPrefabPath = "Assets/Game/Characters/Face/AllFaces.prefab";
    const string NpcPrefabPath = "Assets/Game/Characters/Npc/NPCWithController.prefab";
    const string KarmaPrefabPath = "Assets/Game/Characters/Npc/NPCKarma.prefab";
    const string OrochiPrefabPath = "Assets/Game/Characters/Npc/NPCOrochiWithControllerVariant.prefab";
    const string MitamaPrefabPath = "Assets/Game/Characters/Npc/NPCVLMitamaWithController.prefab";
    const string MillcoSpritePath = "Assets/Game/Characters/Sprite/millco.png";
    const string MillcoAnimatorPath = "Assets/Game/Characters/Animations/MillcoAnimator.overrideController";
    const string NarukamiSpritePath = "Assets/Game/Characters/Sprite/VLNarukami.png";
    const string NarukamiAnimatorPath = "Assets/Game/Characters/Npc/NPCVLNarukamiController.controller";
    const string KarmaFacePath = "Assets/Game/Characters/Sprite/KarmaFace.png";
    const string PunchSePath = "Assets/Game/SE/punch2a.mp3";

    // 生成する NPC の名前
    const string MillcoName = "MillcoNPCWithController";
    const string YamaName = "YamaNPCWithController";
    const string KarmaName = "KarmaNPC";
    const string UnikiName = "UnikiSp_0";
    const string LeeleeName = "NPCLeelee";
    const string AkimName = "NPCAkim";
    const string OrochiName = "NPCOrochi";
    const string MitamaName = "NPCVLMitama";
    const string NarukamiName = "VLNarukamiNPC";
    const string YamaCameraName = "CMCameraYama";
    const int YamaCameraPriority = 20;
    const int LeeleeCameraIdlePriority = 5;
    const int LeeleeCameraPanPriority = 30;
    const string AnchorName = "CameraAnchorUniki";
    const string AnchorCameraName = "CMCameraAnchor";
    const int AnchorCameraPanPriority = 40;
    const float AnchorX = (YamaX + LeeleeStopX) / 2f;

    // 配置(x)。カメラは開始時ヤーマ(x=-8.3)を中心に映しているので、
    // 画面内は x≈-17〜+0.6。右側の画面外から歩いて入ってくる。
    const float GroundY = -16.23f;
    const float MillcoX = -11.0f;
    const float YamaX = -8.3f;
    const float KarmaX = -6.2f;
    const float UnikiStartX = 6.0f;
    const float UnikiStopX = -4.0f;
    const float LeeleeStartX = 7.3f;
    const float LeeleeStopX = -1.2f;
    const float PartyGap = 1.1f;
    const float NarukamiY = -13.3f;

    // ボス戦の残り物で今回使わないもの
    static readonly string[] SourceLeftovers =
    {
        "DragNPCWithController", "DragCharacter", "MillcoCharacter", "Millco", "CMCamera3",
        "StartEvent", "YamaCharacter", "UniCharacterWithFace", "LeeleeCharacterWithFace 1",
    };

    [MenuItem("Tools/Issue641/Setup Yama Revenge Scene")]
    public static void Setup()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        if (!EnsureSceneCopied()) return;
        EnsureBuildSetting();
        EnsureKarmaCharacter();

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        RemoveGenerated(scene);
        RemoveLeftovers(scene);

        Npcs npcs = SetupNpcs(scene);
        if (npcs == null) return;
        GameObject cameraGo = SetupCamera(scene, npcs);
        SetupEvent(scene, npcs, cameraGo);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Issue641] ヤーマ襲来シーンのセットアップが完了しました。");
    }

    static bool EnsureSceneCopied()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null) return true;
        if (!AssetDatabase.CopyAsset(SourceScenePath, ScenePath))
        {
            Debug.LogError($"[Issue641] シーンの複製に失敗しました: {SourceScenePath} -> {ScenePath}");
            return false;
        }
        AssetDatabase.Refresh();
        Debug.Log($"[Issue641] {ScenePath} を作成しました。");
        return true;
    }

    public static int GetBuildIndex()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i].path == ScenePath) return i;
        }
        return -1;
    }

    static void EnsureBuildSetting()
    {
        if (GetBuildIndex() >= 0) return;
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log($"[Issue641] Build Settings 末尾(index {scenes.Count - 1})に追加しました。");
    }

    // AllFaces に「カルマ」(正体表示版)の Character を追加する。秘匿版 KarmaSecretCharacter の設定を流用
    static void EnsureKarmaCharacter()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(AllFacesPrefabPath);
        try
        {
            if (FindCharacter(root, "KarmaCharacter") != null) return;
            Character secret = FindCharacter(root, "KarmaSecretCharacter");
            if (secret == null)
            {
                Debug.LogError("[Issue641] AllFaces に KarmaSecretCharacter がありません。");
                return;
            }
            GameObject go = new GameObject("KarmaCharacter");
            go.transform.SetParent(root.transform, false);
            Character karma = go.AddComponent<Character>();
            EditorUtility.CopySerialized(secret, karma);
            Sprite face = AssetDatabase.LoadAssetAtPath<Sprite>(KarmaFacePath);
            SerializedObject so = new SerializedObject(karma);
            so.FindProperty("nameText").stringValue = "カルマ";
            SerializedProperty portraits = so.FindProperty("portraits");
            portraits.arraySize = 1;
            portraits.GetArrayElementAtIndex(0).objectReferenceValue = face;
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, AllFacesPrefabPath);
            Debug.Log("[Issue641] AllFaces に KarmaCharacter を追加しました。");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static void RemoveGenerated(UnityEngine.SceneManagement.Scene scene)
    {
        foreach (string name in new[] { EventObjectName, MillcoName, KarmaName, OrochiName, MitamaName, NarukamiName, YamaCameraName, AnchorName, AnchorCameraName })
        {
            GameObject go = FindInScene(scene, name);
            if (go != null) Object.DestroyImmediate(go);
        }
    }

    static void RemoveLeftovers(UnityEngine.SceneManagement.Scene scene)
    {
        foreach (string name in SourceLeftovers)
        {
            GameObject go = FindInScene(scene, name);
            if (go != null) Object.DestroyImmediate(go);
        }
    }

    class Npcs
    {
        public GameObject core, party, cameraShaker, millco, yama, karma, uniki, leelee, akim, orochi, mitama, narukami;
    }

    static Npcs SetupNpcs(UnityEngine.SceneManagement.Scene scene)
    {
        Npcs n = new Npcs
        {
            core = FindInScene(scene, "Core"),
            party = FindInScene(scene, "Party"),
            cameraShaker = FindInScene(scene, "CameraShaker"),
            yama = FindInScene(scene, YamaName),
            uniki = FindInScene(scene, UnikiName),
            leelee = FindInScene(scene, LeeleeName),
            akim = FindInScene(scene, AkimName),
        };
        if (n.core == null || n.party == null || n.cameraShaker == null || n.yama == null || n.uniki == null || n.leelee == null || n.akim == null)
        {
            Debug.LogError($"[Issue641] 複製元のオブジェクトが見つかりません。core={n.core} party={n.party} cameraShaker={n.cameraShaker} yama={n.yama} uniki={n.uniki} leelee={n.leelee} akim={n.akim}");
            return null;
        }

        // Millco: NPCWithController にスプライトとアニメを上書き(boss_5 と同じ)
        n.millco = InstantiatePrefab(scene, NpcPrefabPath, MillcoName);
        OverrideLook(n.millco, FirstSprite(MillcoSpritePath), AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(MillcoAnimatorPath));
        Place(n.millco, MillcoX, GroundY, faceLeft: false);

        Place(n.yama, YamaX, n.yama.transform.position.y, faceLeft: true);

        n.karma = InstantiatePrefab(scene, KarmaPrefabPath, KarmaName);
        Place(n.karma, KarmaX, GroundY, faceLeft: true);

        // ウニキは複製元では ArrivedVLFarm で非表示になる設定なので外す(画面外に居るだけでよい)
        VisibilityFlagManager visibility = n.uniki.GetComponent<VisibilityFlagManager>();
        if (visibility != null) Object.DestroyImmediate(visibility);
        Place(n.uniki, UnikiStartX, n.uniki.transform.position.y, faceLeft: true);

        Place(n.leelee, LeeleeStartX, n.leelee.transform.position.y, faceLeft: true);
        Place(n.akim, LeeleeStartX + PartyGap, n.akim.transform.position.y, faceLeft: true);
        n.orochi = InstantiatePrefab(scene, OrochiPrefabPath, OrochiName);
        Place(n.orochi, LeeleeStartX + PartyGap * 2, GroundY, faceLeft: true);
        n.mitama = InstantiatePrefab(scene, MitamaPrefabPath, MitamaName);
        Place(n.mitama, LeeleeStartX + PartyGap * 3, GroundY, faceLeft: true);

        n.narukami = CreateNarukami(scene);
        Place(n.narukami, LeeleeStartX + PartyGap * 2, NarukamiY, faceLeft: true);
        return n;
    }

    // ナルカミは NPC プレハブが無いので Kaze3_boss3 と同じ構成をシーン直組みする。
    // TODO(#641): 飛行アニメーションは未作成。静止スプライト(翼を広げたコマ)を空中に置いて平行移動させている。
    static GameObject CreateNarukami(UnityEngine.SceneManagement.Scene scene)
    {
        GameObject go = new GameObject(NarukamiName);
        MoveToScene(go, scene);
        go.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(NarukamiSpritePath).OfType<Sprite>()
            .FirstOrDefault(s => s.name == "VLNarukami_0");
        renderer.sprite = sprite;
        SpriteRenderer reference = AssetDatabase.LoadAssetAtPath<GameObject>(NpcPrefabPath).GetComponent<SpriteRenderer>();
        renderer.sortingLayerID = reference.sortingLayerID;
        renderer.sortingOrder = 2;
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
        collider.offset = new Vector2(1.28f, -1.28f);
        collider.size = new Vector2(2.56f, 2.56f);
        go.AddComponent<NPCController>();
        Animator animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(NarukamiAnimatorPath);
        if (sprite == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogError("[Issue641] ナルカミのスプライトまたはアニメーターが見つかりません。");
        }
        return go;
    }

    static void MoveToScene(GameObject go, UnityEngine.SceneManagement.Scene scene)
    {
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
    }

    static void OverrideLook(GameObject go, Sprite sprite, RuntimeAnimatorController controller)
    {
        if (go == null) return;
        if (sprite == null || controller == null)
        {
            Debug.LogError($"[Issue641] {go.name} のスプライトまたはアニメーターが見つかりません。");
            return;
        }
        go.GetComponent<SpriteRenderer>().sprite = sprite;
        go.GetComponent<Animator>().runtimeAnimatorController = controller;
    }

    static Sprite FirstSprite(string path)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).FirstOrDefault();
    }

    // NPCController の向きは localScale.x の符号(正=左向き)
    static void Place(GameObject go, float x, float y, bool faceLeft)
    {
        if (go == null) return;
        go.transform.position = new Vector3(x, y, go.transform.position.z);
        Vector3 scale = go.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (faceLeft ? 1 : -1);
        go.transform.localScale = scale;
    }

    // 開始時はヤーマ追従の CMCameraYama(優先度 20)がプレイヤー追従の CMCamera(10)に勝つ。
    // CMCamera2 はリーリー追従にしておき、一行到着時に SetPriority(30) でパンする
    static GameObject SetupCamera(UnityEngine.SceneManagement.Scene scene, Npcs npcs)
    {
        GameObject cameraGo = FindInScene(scene, "CMCamera2");
        if (cameraGo == null)
        {
            Debug.LogError("[Issue641] CMCamera2 が見つかりません。");
            return null;
        }
        SetVirtualCamera(cameraGo, npcs.leelee.transform, LeeleeCameraIdlePriority);

        GameObject yamaCameraGo = Object.Instantiate(cameraGo, cameraGo.transform.parent);
        yamaCameraGo.name = YamaCameraName;
        SetVirtualCamera(yamaCameraGo, npcs.yama.transform, YamaCameraPriority);

        // 固定位置用の空アンカー(ヤーマとリーリーの中間)を追従させる。誰かを追う訳ではない
        GameObject anchor = new GameObject(AnchorName);
        MoveToScene(anchor, scene);
        anchor.transform.position = new Vector3(AnchorX, GroundY, 0);
        GameObject anchorCameraGo = Object.Instantiate(cameraGo, cameraGo.transform.parent);
        anchorCameraGo.name = AnchorCameraName;
        SetVirtualCamera(anchorCameraGo, anchor.transform, LeeleeCameraIdlePriority);
        return cameraGo;
    }

    static void SetVirtualCamera(GameObject cameraGo, Transform follow, int priority)
    {
        CinemachineVirtualCamera vcam = cameraGo.GetComponent<CinemachineVirtualCamera>();
        SerializedObject so = new SerializedObject(vcam);
        so.FindProperty("m_Follow").objectReferenceValue = follow;
        so.FindProperty("m_Priority").intValue = priority;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetupEvent(UnityEngine.SceneManagement.Scene scene, Npcs n, GameObject cameraGo)
    {
        GameObject eventGo = new GameObject(EventObjectName);
        MoveToScene(eventGo, scene);
        Flowchart flowchart = eventGo.AddComponent<Flowchart>();
        eventGo.AddComponent<FlowchartStopAllGuard>();
        GameEvent gameEvent = eventGo.AddComponent<GameEvent>();
        gameEvent.flowChart = flowchart;
        gameEvent.InformationText = "";
        SerializedObject eventSo = new SerializedObject(gameEvent);
        SerializedProperty list = eventSo.FindProperty("flagToBlockName");
        list.arraySize = 1;
        SerializedProperty entry = list.GetArrayElementAtIndex(0);
        entry.FindPropertyRelative("flag").intValue = (int)Flag.None;
        entry.FindPropertyRelative("blockName").stringValue = BlockName;
        entry.FindPropertyRelative("isAutoStart").boolValue = true;
        entry.FindPropertyRelative("isCollisionStart").boolValue = false;
        eventSo.ApplyModifiedPropertiesWithoutUndo();

        // 顔グラ
        GameObject allFaces = InstantiatePrefab(scene, AllFacesPrefabPath, "AllFaces");
        allFaces.transform.SetParent(eventGo.transform, false);
        Character millco = FindCharacter(allFaces, "MillcoCharacter");
        Character yama = FindCharacter(allFaces, "YamaCharacter");
        Character karma = FindCharacter(allFaces, "KarmaCharacter");
        Character uniki = FindCharacter(allFaces, "UniCharacterWithFace");
        Character leelee = FindCharacter(allFaces, "LeeleeCharacter");
        if (millco == null || yama == null || karma == null || uniki == null || leelee == null)
        {
            Debug.LogError($"[Issue641] AllFaces 内の会話キャラが見つかりません。millco={millco} yama={yama} karma={karma} uniki={uniki} leelee={leelee}");
            return;
        }
        SayDialog sayDialog = FindSceneSayDialog(scene);
        foreach (Character c in new[] { millco, yama, karma, uniki, leelee })
        {
            SetSayDialog(c, sayDialog);
        }

        GameObject karmaEmotion = n.karma.transform.Find("Emotion")?.gameObject;
        if (karmaEmotion == null) Debug.LogError("[Issue641] KarmaNPC に Emotion 子がありません。");
        GameObject anchorCameraGo = FindInScene(scene, AnchorCameraName);
        float yamaY = n.yama.transform.position.y;
        AudioClip punchSe = AssetDatabase.LoadAssetAtPath<AudioClip>(PunchSePath);

        Block block = flowchart.CreateBlock(new Vector2(50, 50));
        block.BlockName = BlockName;

        // 開始: プレイヤーを隠す(ボスシーンと同じ)
        AddInvokeMethod(flowchart, block, n.party, typeof(PartyCongroller), "SetVisibility", false);
        AddWait(flowchart, block, 2.3f);

        // Millco と VLYama の対峙
        AddSay(flowchart, block, millco, "あんたたち なんなのよ！\n暴力反対ー");
        AddSay(flowchart, block, yama, "ふむ。何度も言うように\nこちらの要求に従っていただければ\n問題はないのだが");
        AddSay(flowchart, block, millco, "だから虹の種なんて あたし「は」\nよく知らねーんだって。\n別の村で聞いてみれば？");
        AddSay(flowchart, block, yama, "で、あるか\nならば");
        AddWait(flowchart, block, 1f);
        AddInvokeMethod(flowchart, block, n.yama, typeof(NPCController), "SetSpeed", 10f);
        AddPunch(flowchart, block, n.yama, YamaX, punchSe);
        AddWait(flowchart, block, 0.4f);
        AddPunch(flowchart, block, n.yama, YamaX, punchSe);
        AddWait(flowchart, block, 2f);
        AddInvokeMethod(flowchart, block, n.millco, typeof(NPCController), "Shake", 0.5f, 0.1f);
        AddSay(flowchart, block, millco, "もー なんであたしばっか\nこんなに殴られないと\nいけないわけー？");
        AddSay(flowchart, block, millco, "クラゲだからってボコボコボコボコ\n殴っていいわけじゃないからね？\nしぬときは死ぬからね？");
        AddSay(flowchart, block, yama, "丈夫で何より。\nギリギリまでいけるというわけだ");
        AddSay(flowchart, block, millco, "やだーもー ギリギリとかいわないでー\n野蛮すぎるー");

        // ウニキが走ってくる(ボスシーンと同じ)
        AddSay(flowchart, block, uniki, "Millcoさーーーーーーーん！");
        AddInvokeMethod(flowchart, block, n.yama, typeof(NPCController), "SetDirection", NPCController.Direction.Right);
        AddInvokeMethod(flowchart, block, n.karma, typeof(NPCController), "SetDirection", NPCController.Direction.Right);
        AddInvokeMethod(flowchart, block, n.uniki, typeof(NPCController), "MoveToPositionEvent", new Vector3(UnikiStopX, 0, 0), 0f, false);
        AddSay(flowchart, block, uniki, "てめMillcoさんに何やってくれちゃ...\nってヤーマ様じゃないですかあ〜。");
        AddSay(flowchart, block, uniki, "なんですかもー、来るなら来るって\n言ってくださればー");
        AddSay(flowchart, block, millco, "あ！ やっと来た！\nいっつもおせーんだよー！");
        AddSay(flowchart, block, millco, "もー なんとか言ってよ\nこの人たちにさー");
        AddSay(flowchart, block, uniki, "コラっ！ Millcoさんコラっ！\n国王に「この人たち」はほんともう\n常識が終わってるから！ コラっ！");
        AddSay(flowchart, block, uniki, "へっへっへ、すいませんね...もうどうもね。\n学がね。ないもんでね");
        AddSay(flowchart, block, millco, "あぁん！？");

        // 目にも止まらぬ早さで Millco とウニキを殴る(瞬間移動 + 殴り + 倒れる)
        // 1発目は右を向いているので keepDirection=false で Millco 側(左)へ向き直る
        AddPunch(flowchart, block, n.yama, YamaX, punchSe, keepDirection: false);
        AddInvokeMethod(flowchart, block, n.millco, typeof(NPCController), "Defeated", 90f);
        AddInvokeMethod(flowchart, block, n.cameraShaker, typeof(CinemachineImpulseSource), "GenerateImpulseWithForce", 0.5f);
        AddInvokeMethod(flowchart, block, n.yama, typeof(NPCController), "SetPosition", new Vector3(UnikiStopX + 2.4f, yamaY, 0));
        AddInvokeMethod(flowchart, block, n.yama, typeof(NPCController), "SetDirection", NPCController.Direction.Left);
        AddPunch(flowchart, block, n.yama, UnikiStopX + 2.4f, punchSe);
        AddInvokeMethod(flowchart, block, n.uniki, typeof(NPCController), "Defeated", 90f);
        AddInvokeMethod(flowchart, block, n.cameraShaker, typeof(CinemachineImpulseSource), "GenerateImpulseWithForce", 0.5f);
        AddWait(flowchart, block, 0.4f);
        AddInvokeMethod(flowchart, block, n.yama, typeof(NPCController), "SetPosition", new Vector3(YamaX, yamaY, 0));
        AddInvokeMethod(flowchart, block, n.yama, typeof(NPCController), "SetDirection", NPCController.Direction.Left);
        if (karmaEmotion != null) AddInvokeMethod(flowchart, block, karmaEmotion, typeof(Emotion), "Bikkuri");
        AddWait(flowchart, block, 3f);

        AddSay(flowchart, block, yama, "双方ともずいぶんと騒がしく\n礼儀を知らぬ口ぶり。やはり辺境の\n村々の文化は肌にあわぬな。");
        AddSay(flowchart, block, yama, "カルマ、連れて行け。\nVLパレスでじっくりと尋問してやろう");
        AddInvokeMethod(flowchart, block, n.karma, typeof(NPCController), "SetDirection", NPCController.Direction.Left);
        AddSay(flowchart, block, karma, "はっ！");
        AddSay(flowchart, block, karma, "(これはなかなかどうして...\nやはりCNPの力は伊達じゃないようだな。\nおもしろくなりそうだ...)");

        // ウニキが震えて立ち上がる
        AddInvokeMethod(flowchart, block, n.uniki, typeof(NPCController), "Shake", 1.5f, 0.05f);
        AddWait(flowchart, block, 1f);
        AddSay(flowchart, block, uniki, "うふふふふふ。。。ちょっと、ぷすす。\nちょっと待ってくださいよぉ。。。ぷす。");
        AddInvokeMethod(flowchart, block, n.yama, typeof(NPCController), "SetDirection", NPCController.Direction.Right);
        AddInvokeMethod(flowchart, block, n.karma, typeof(NPCController), "SetDirection", NPCController.Direction.Right);
        AddInvokeMethod(flowchart, block, n.uniki, typeof(NPCController), "Defeated", -90f);
        // 倒れている間に当たり判定が回転して沈むので、起き上がりで地面の高さに戻す
        AddInvokeMethod(flowchart, block, n.uniki, typeof(NPCController), "SetPosition", new Vector3(UnikiStopX, GroundY, 0));
        AddWait(flowchart, block, 0.5f);
        AddSay(flowchart, block, uniki, "もしかしてぇ〜 今日はシモーヌさんは\nいない感じですかぁ？");
        AddSay(flowchart, block, yama, "ふむ。そちらについても聞きたい。\nどうもあの日以来姿が見えんのだ。");
        AddSay(flowchart, block, yama, "うぬらが良からぬ気を起こしては\nいまいかとな。そのあたりもよーーく\n聞き取りすることになるだろう");
        AddSay(flowchart, block, uniki, "へ、へえ〜。。。シモーヌさん\n会いたかったな〜　いないんだ〜");
        AddSay(flowchart, block, uniki, "ですって！ リーリーさん！！");

        // リーリー一行が近づいてくる → カメラをリーリーへパン
        AddInvokeMethod(flowchart, block, n.leelee, typeof(NPCController), "MoveToPositionEvent", new Vector3(LeeleeStopX, 0, 0), 0f, false);
        AddInvokeMethod(flowchart, block, n.akim, typeof(NPCController), "MoveToPositionEvent", new Vector3(LeeleeStopX + PartyGap, 0, 0), 0f, false);
        AddInvokeMethod(flowchart, block, n.orochi, typeof(NPCController), "MoveToPositionEvent", new Vector3(LeeleeStopX + PartyGap * 2, 0, 0), 0f, false);
        AddInvokeMethod(flowchart, block, n.mitama, typeof(NPCController), "MoveToPositionEvent", new Vector3(LeeleeStopX + PartyGap * 3, 0, 0), 0f, false);
        AddInvokeMethod(flowchart, block, n.narukami, typeof(NPCController), "MoveToPositionEvent", new Vector3(LeeleeStopX + PartyGap * 2, 0, 0), 0f, false);
        AddWait(flowchart, block, 0.5f);
        if (cameraGo != null)
        {
            AddInvokeMethod(flowchart, block, cameraGo, typeof(CMCameraSetFollow), "SetPriority", LeeleeCameraPanPriority);
        }
        AddWait(flowchart, block, 2.5f);
        AddSay(flowchart, block, leelee, "聞いたで。つまり千載一遇の\nチャンスってわけやな？ ウニ");
        AddInvokeMethod(flowchart, block, n.yama, typeof(NPCController), "SetDirection", NPCController.Direction.Right);
        AddSay(flowchart, block, yama, "おまえは......！");
        // ヤーマとリーリーが同時に映るよう、ウニキ付近の固定位置(誰も追従しない)へパン
        if (anchorCameraGo != null)
        {
            AddInvokeMethod(flowchart, block, anchorCameraGo, typeof(CMCameraSetFollow), "SetPriority", AnchorCameraPanPriority);
        }
        AddSay(flowchart, block, leelee, "よーう ヤーマ。1万年ぶりってか？\nつってもこちとら記憶は\nおぼろげやがなあ");
        AddComment(flowchart, block, "TODO(#641): ここから先(WIP)はシナリオ確定後に追加する。最後はごみ捨て部屋への落下 → Coming Soon");
    }

    // 殴り: 攻撃者が 1.3 だけ左へ踏み込み、SE を鳴らして元の x へ戻る(ボスシーンの定型)
    static void AddPunch(Flowchart flowchart, Block block, GameObject attacker, float originX, AudioClip se, bool keepDirection = true)
    {
        AddInvokeMethod(flowchart, block, attacker, typeof(NPCController), "MoveToRelativePositionEvent", new Vector3(-1.3f, 0, 0), 0.3f, keepDirection);
        AddWait(flowchart, block, 0.3f);
        AddPlaySound(flowchart, block, se, 0.7f);
        AddInvokeMethod(flowchart, block, attacker, typeof(NPCController), "MoveToPositionEvent", new Vector3(originX, 0, 0), 0f, true);
    }

    // ---- Fungus コマンド生成ヘルパー ----

    static void AddSay(Flowchart flowchart, Block block, Character character, string storyText)
    {
        Say say = flowchart.gameObject.AddComponent<Say>();
        say.ItemId = flowchart.NextItemId();
        say.ParentBlock = block;
        SerializedObject so = new SerializedObject(say);
        so.FindProperty("storyText").stringValue = storyText;
        so.FindProperty("character").objectReferenceValue = character;
        Sprite portrait = character != null && character.Portraits != null && character.Portraits.Count > 0
            ? character.Portraits[0] : null;
        so.FindProperty("portrait").objectReferenceValue = portrait;
        so.ApplyModifiedPropertiesWithoutUndo();
        block.CommandList.Add(say);
    }

    static void AddWait(Flowchart flowchart, Block block, float duration)
    {
        Wait wait = flowchart.gameObject.AddComponent<Wait>();
        wait.ItemId = flowchart.NextItemId();
        wait.ParentBlock = block;
        SerializedObject so = new SerializedObject(wait);
        so.FindProperty("_duration").FindPropertyRelative("floatVal").floatValue = duration;
        so.ApplyModifiedPropertiesWithoutUndo();
        block.CommandList.Add(wait);
    }

    static void AddPlaySound(Flowchart flowchart, Block block, AudioClip clip, float volume)
    {
        PlaySound sound = flowchart.gameObject.AddComponent<PlaySound>();
        sound.ItemId = flowchart.NextItemId();
        sound.ParentBlock = block;
        SerializedObject so = new SerializedObject(sound);
        so.FindProperty("soundClip").objectReferenceValue = clip;
        so.FindProperty("volume").floatValue = volume;
        so.FindProperty("waitUntilFinished").boolValue = false;
        so.ApplyModifiedPropertiesWithoutUndo();
        block.CommandList.Add(sound);
    }

    static void AddComment(Flowchart flowchart, Block block, string text)
    {
        Comment comment = flowchart.gameObject.AddComponent<Comment>();
        comment.ItemId = flowchart.NextItemId();
        comment.ParentBlock = block;
        SerializedObject so = new SerializedObject(comment);
        so.FindProperty("commentText").stringValue = text;
        so.ApplyModifiedPropertiesWithoutUndo();
        block.CommandList.Add(comment);
    }

    // InvokeMethod。引数は bool / int / float / Vector3 / enum に対応(Fungus の ObjectValue と同じ持ち方)
    static void AddInvokeMethod(Flowchart flowchart, Block block, GameObject targetObject, System.Type componentType, string methodName, params object[] args)
    {
        if (targetObject == null || targetObject.GetComponent(componentType) == null)
        {
            Debug.LogError($"[Issue641] InvokeMethod の対象がありません: {targetObject?.name}.{componentType.Name}.{methodName}");
            return;
        }
        InvokeMethod invoke = flowchart.gameObject.AddComponent<InvokeMethod>();
        invoke.ItemId = flowchart.NextItemId();
        invoke.ParentBlock = block;
        SerializedObject so = new SerializedObject(invoke);
        so.FindProperty("targetObject").objectReferenceValue = targetObject;
        so.FindProperty("targetComponentAssemblyName").stringValue = componentType.AssemblyQualifiedName;
        so.FindProperty("targetComponentFullname").stringValue = "UnityEngine.Component[]";
        so.FindProperty("targetComponentText").stringValue = componentType.Name;
        so.FindProperty("targetMethod").stringValue = methodName;
        string paramText = string.Join(", ", args.Select(a => ParameterTypeName(a.GetType())));
        so.FindProperty("targetMethodText").stringValue = $"{methodName} ({paramText}): Void";
        so.FindProperty("returnValueType").stringValue = "System.Void";
        SerializedProperty parameters = so.FindProperty("methodParameters");
        parameters.arraySize = args.Length;
        for (int i = 0; i < args.Length; i++)
        {
            SerializedProperty objValue = parameters.GetArrayElementAtIndex(i).FindPropertyRelative("objValue");
            System.Type type = args[i].GetType();
            objValue.FindPropertyRelative("typeAssemblyname").stringValue = type.AssemblyQualifiedName;
            objValue.FindPropertyRelative("typeFullname").stringValue = type.FullName;
            switch (args[i])
            {
                case bool b: objValue.FindPropertyRelative("boolValue").boolValue = b; break;
                case int n: objValue.FindPropertyRelative("intValue").intValue = n; break;
                case float f: objValue.FindPropertyRelative("floatValue").floatValue = f; break;
                case Vector3 v: objValue.FindPropertyRelative("vector3Value").vector3Value = v; break;
                case System.Enum e: objValue.FindPropertyRelative("intValue").intValue = System.Convert.ToInt32(e); break;
                default:
                    Debug.LogError($"[Issue641] 未対応の引数型です: {type}");
                    break;
            }
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        block.CommandList.Add(invoke);
    }

    static string ParameterTypeName(System.Type type)
    {
        if (type == typeof(bool)) return "Boolean";
        if (type == typeof(int)) return "Int32";
        if (type == typeof(float)) return "Single";
        return type.Name;
    }

    // ---- 共通ユーティリティ ----

    static GameObject InstantiatePrefab(UnityEngine.SceneManagement.Scene scene, string prefabPath, string name)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[Issue641] プレハブが見つかりません: {prefabPath}");
            return null;
        }
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        instance.name = name;
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

    // プレイモード検証用: シーンを開く(エディットモード)
    [MenuItem("Tools/Issue641/Debug/Open Yama Revenge Scene")]
    public static void DebugOpenScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }
}
