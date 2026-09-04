using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Fungus;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
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

    // ---- 後半(召喚〜闇堕ち〜巨大リーリーパンチ〜落下)で使うもの ----
    const string Yami5FScenePath = "Assets/Scenes/Yami5F.unity";
    const string SeedName = "DarkRainbowSeed";
    const string GiantName = "DarkLeeleeGiant";
    const string GiantFallingName = "DarkLeeleeGiantFalling";
    const string ArmName = "DarkLeeleeArm";
    const string CrackName = "GroundCrack";
    const string HoleBackName = "HoleBack";
    const string FakeGroundName = "FakeGround";
    const string CutInName = "CutIn";
    const string GroundTilemapName = "Tilemap";
    const string DarkLeeleeCharacterName = "LeeleeDarkCharacter";
    const string GiantSpritePath = "Assets/Game/Characters/Sprite/dark_giant_leelee_512x640.png";
    const string DarkFacePath = "Assets/Game/Characters/Sprite/dark_leelee_face_1024x1024.png";
    const string ArmSpritePath = "Assets/Game/Characters/Sprite/dark_giant_leelee_arm_512x1280.png";
    const string CrackSpritePath = "Assets/Game/Effect/ground_crack_debris_640x192.png";
    const string HoleSpritePath = "Assets/Game/Effect/hole_black_4x4.png";
    const string YotyouBgmPath = "Assets/Game/BGM/MusMus-BGM-051_yotyou.mp3";
    const string HowanSePath = "Assets/Game/SE/howan.mp3";
    const string AbsorbSePath = "Assets/Game/SE/jump06.mp3";
    const string ChargeSePath = "Assets/Game/SE/エネルギー充填.mp3";
    const string CutInSePath = "Assets/Game/SE/keen.mp3";
    const string JumpSePath = "Assets/Game/SE/jump04.mp3";
    const string ImpactSePath = "Assets/Game/SE/destruction3.mp3";
    const string BombSePath = "Assets/Game/SE/bomb.mp3";
    const string HoverSePath = "Assets/Game/SE/thruster.wav";
    const string EmotionSePath = "Assets/Game/SE/emotion.mp3";
    const int LeeleeCameraCloseupPriority = 50;
    // 穴: 一行(x≈-0.1〜2.1)の足元。セル x=-1..3 を全行くり抜き、同じタイルを FakeGround に持たせる
    const int HoleCellMin = -1;
    const int HoleCellMax = 3;
    const float HoleCenterX = (HoleCellMin + HoleCellMax + 1) / 2f;
    const float HoleWidth = HoleCellMax - HoleCellMin + 1;
    // NPC(scale 1.667、BoxCollider 高さ 2.55)の当たり判定の下端 = NPC が立つ高さ
    const float NpcHalfHeight = 2.55f * 1.6666665f / 2f;
    const float GroundSurfaceY = GroundY - NpcHalfHeight;
    const float GiantScale = 0.8f; // 640px → 5.12 ユニット(ヤマタノオロチより一回り大きい)
    const float GiantHalfHeight = 6.4f * GiantScale / 2f;
    const float SeedOffsetY = 3.6f; // Yami5F と同じく対象の頭上
    const float OffscreenUp = 18f;
    const float FallDepth = 14f;
    const float SinkDepth = 1.2f;

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
    const string YamaAnchorName = "CameraAnchorYama";
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
        // 対象シーン自身が開いているときは作り直すだけなので保存確認を出さない
        // (Cinemachine がエディタ上でカメラ位置を書き換えて dirty になり、unicli 経由だとモーダルで止まるため)
        if (!IsOnlyTargetSceneOpen() && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        if (!EnsureSceneCopied()) return;
        EnsureBuildSetting();
        EnsureKarmaCharacter();
        EnsureSpriteImports();
        EnsureDarkLeeleeCharacter();

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        RemoveGenerated(scene);
        RemoveLeftovers(scene);

        Npcs npcs = SetupNpcs(scene);
        if (npcs == null) return;
        GameObject cameraGo = SetupCamera(scene, npcs);
        Props props = SetupProps(scene, npcs);
        SetupEvent(scene, npcs, props, cameraGo);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Issue641] ヤーマ襲来シーンのセットアップが完了しました。");
    }

    static bool IsOnlyTargetSceneOpen()
    {
        int count = UnityEngine.SceneManagement.SceneManager.sceneCount;
        for (int i = 0; i < count; i++)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).path != ScenePath) return false;
        }
        return count > 0;
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
        // 前回くり抜いたタイルを本物の Tilemap へ戻してから消す(再実行でタイルが消えないように)
        GameObject oldFake = FindInScene(scene, FakeGroundName);
        GameObject ground = FindInScene(scene, GroundTilemapName);
        if (oldFake != null && ground != null)
        {
            RestoreHole(oldFake.GetComponent<Tilemap>(), ground.GetComponent<Tilemap>());
        }
        foreach (string name in new[]
        {
            EventObjectName, MillcoName, KarmaName, OrochiName, MitamaName, NarukamiName, YamaCameraName, YamaAnchorName, AnchorName, AnchorCameraName,
            SeedName, GiantName, GiantFallingName, ArmName, CrackName, HoleBackName, FakeGroundName, CutInName,
        })
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

    // 開始時はヤーマの初期位置に置いた空アンカーを追う CMCameraYama(優先度 20)が
    // プレイヤー追従の CMCamera(10)に勝つ。ヤーマ本人を追うと殴りの踏み込みで画角が動くため。
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

        GameObject yamaAnchor = CreateAnchor(scene, YamaAnchorName, YamaX);
        GameObject yamaCameraGo = Object.Instantiate(cameraGo, cameraGo.transform.parent);
        yamaCameraGo.name = YamaCameraName;
        SetVirtualCamera(yamaCameraGo, yamaAnchor.transform, YamaCameraPriority);

        // 固定位置用の空アンカー(ヤーマとリーリーの中間)を追従させる。誰かを追う訳ではない
        GameObject anchor = CreateAnchor(scene, AnchorName, AnchorX);
        GameObject anchorCameraGo = Object.Instantiate(cameraGo, cameraGo.transform.parent);
        anchorCameraGo.name = AnchorCameraName;
        SetVirtualCamera(anchorCameraGo, anchor.transform, LeeleeCameraIdlePriority);
        return cameraGo;
    }

    // カメラの固定位置用の空アンカー(誰も追従しない画角を作る)
    static GameObject CreateAnchor(UnityEngine.SceneManagement.Scene scene, string name, float x)
    {
        GameObject anchor = new GameObject(name);
        MoveToScene(anchor, scene);
        anchor.transform.position = new Vector3(x, GroundY, 0);
        return anchor;
    }

    static void SetVirtualCamera(GameObject cameraGo, Transform follow, int priority)
    {
        CinemachineVirtualCamera vcam = cameraGo.GetComponent<CinemachineVirtualCamera>();
        SerializedObject so = new SerializedObject(vcam);
        so.FindProperty("m_Follow").objectReferenceValue = follow;
        so.FindProperty("m_Priority").intValue = priority;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetupEvent(UnityEngine.SceneManagement.Scene scene, Npcs n, Props p, GameObject cameraGo)
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
        Character leeleeDark = FindCharacter(allFaces, DarkLeeleeCharacterName);
        Character orochi = FindCharacter(allFaces, "OrochiCharacter");
        Character mitama = FindCharacter(allFaces, "MitamaCharacter");
        Character narukami = FindCharacter(allFaces, "NarukamiCharacter");
        if (millco == null || yama == null || karma == null || uniki == null || leelee == null
            || leeleeDark == null || orochi == null || mitama == null || narukami == null)
        {
            Debug.LogError($"[Issue641] AllFaces 内の会話キャラが見つかりません。millco={millco} yama={yama} karma={karma} uniki={uniki} leelee={leelee} leeleeDark={leeleeDark} orochi={orochi} mitama={mitama} narukami={narukami}");
            return;
        }
        SayDialog sayDialog = FindSceneSayDialog(scene);
        foreach (Character c in new[] { millco, yama, karma, uniki, leelee, leeleeDark, orochi, mitama, narukami })
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

        AddSecondHalf(flowchart, block, n, p, cameraGo,
            new Chars { yama = yama, karma = karma, uniki = uniki, leelee = leelee, leeleeDark = leeleeDark, orochi = orochi, mitama = mitama, narukami = narukami });
    }

    class Chars
    {
        public Character yama, karma, uniki, leelee, leeleeDark, orochi, mitama, narukami;
    }

    // 後半: 召喚 → 取り込み → 闇堕ち → 巨大リーリーパンチ → 落下 → 暗転
    static void AddSecondHalf(Flowchart flowchart, Block block, Npcs n, Props p, GameObject cameraGo, Chars c)
    {
        if (p == null) return;
        AudioClip howanSe = AssetDatabase.LoadAssetAtPath<AudioClip>(HowanSePath);
        AudioClip absorbSe = AssetDatabase.LoadAssetAtPath<AudioClip>(AbsorbSePath);
        AudioClip chargeSe = AssetDatabase.LoadAssetAtPath<AudioClip>(ChargeSePath);
        AudioClip jumpSe = AssetDatabase.LoadAssetAtPath<AudioClip>(JumpSePath);
        AudioClip impactSe = AssetDatabase.LoadAssetAtPath<AudioClip>(ImpactSePath);
        AudioClip bombSe = AssetDatabase.LoadAssetAtPath<AudioClip>(BombSePath);
        AudioClip hoverSe = AssetDatabase.LoadAssetAtPath<AudioClip>(HoverSePath);
        AudioClip yotyou = AssetDatabase.LoadAssetAtPath<AudioClip>(YotyouBgmPath);
        float akimX = LeeleeStopX + PartyGap;
        float orochiX = LeeleeStopX + PartyGap * 2;
        float surfaceY = p.tileSurfaceY;

        AddSay(flowchart, block, c.yama, "封印が解かれたと思ったら...\nこんなところに...");
        AddSay(flowchart, block, c.leelee, "あーん？ シモーヌから\n聞いてへんのかあ？？");
        AddSay(flowchart, block, c.leelee, "あ、部下にも逃げられる\nマヌケなんやったなあ？");
        AddSay(flowchart, block, c.yama, "くっ！ 言わせておけば...！");

        // カルマがヤーマの隣へ寄って耳打ちし、元の位置へ戻る
        AddInvokeMethod(flowchart, block, n.karma, typeof(NPCController), "MoveToPositionEvent", new Vector3(YamaX + 1.1f, 0, 0), 0f, false);
        AddWait(flowchart, block, 0.8f);
        AddSay(flowchart, block, c.karma, "ヤーマ様...");
        AddSay(flowchart, block, c.yama, "ほう...よかろう。\n好きにせい...");
        AddInvokeMethod(flowchart, block, n.karma, typeof(NPCController), "MoveToPositionEvent", new Vector3(KarmaX, 0, 0), 0f, false);
        AddWait(flowchart, block, 0.6f);
        AddInvokeMethod(flowchart, block, n.karma, typeof(NPCController), "SetDirection", NPCController.Direction.Right);
        AddInvokeMethod(flowchart, block, p.bgmWrapper, typeof(BGMWrapper), "FadeOut", 2f);
        AddSay(flowchart, block, c.yama, "リーリーよ 旧交を温めたいところだが\nあいにく余には時間がない。\n国王としての責務がある");
        AddSay(flowchart, block, c.leelee, "なーにが国王や！ 国王は...");
        AddSay(flowchart, block, c.yama, "おおっと そこまでだ。カルマ！");
        AddSay(flowchart, block, c.karma, "はっ！");

        // 召喚(Yami5F と同じ段取り): 構え 3 秒 → 発動 → リーリーへ寄る → 種が頭上に出て取り込まれる
        AddInvokeMethod(flowchart, block, n.karma, typeof(NPCController), "PlayPreMagicAnimation", true);
        AddWait(flowchart, block, 3f);
        AddInvokeMethod(flowchart, block, n.karma, typeof(NPCController), "PlayPreMagicAnimation", false);
        AddInvokeMethod(flowchart, block, n.karma, typeof(NPCController), "PlayMagicAnimation", true);
        AddWait(flowchart, block, 0.2f);
        AddPlaySound(flowchart, block, howanSe, 1f);
        AddWait(flowchart, block, 0.8f);
        if (cameraGo != null)
        {
            AddInvokeMethod(flowchart, block, cameraGo, typeof(CMCameraSetFollow), "SetPriority", LeeleeCameraCloseupPriority);
        }
        AddWait(flowchart, block, 1f);
        AddSetActive(flowchart, block, p.seed, true);
        AddWait(flowchart, block, 1f);
        AddInvokeMethod(flowchart, block, p.seed, typeof(Rigidbody2D), "AddForce", new Vector2(0, -100));
        AddWait(flowchart, block, 0.8f);
        AddInvokeMethod(flowchart, block, p.seed, typeof(ManualFadeObject), "FadeOut", 1.5f);
        AddWait(flowchart, block, 0.9f);
        AddPlaySound(flowchart, block, absorbSe, 1f);
        AddSetActive(flowchart, block, p.seed, false);
        AddInvokeMethod(flowchart, block, n.karma, typeof(NPCController), "PlayMagicAnimation", false);
        AddSay(flowchart, block, c.leelee, "なっ！");

        // 予兆の BGM に切り替え、リーリーが震え、3 人がビックリ
        AddInvokeMethod(flowchart, block, p.bgmWrapper, typeof(BGMWrapper), "Play", yotyou, 0.3f, 1f);
        AddInvokeMethod(flowchart, block, n.leelee, typeof(NPCController), "Shake", 3f, 0.08f);
        AddWait(flowchart, block, 0.5f);
        AddBikkuri(flowchart, block, n.orochi);
        AddBikkuri(flowchart, block, n.mitama);
        AddBikkuri(flowchart, block, n.narukami);
        AddWait(flowchart, block, 1f);
        AddSay(flowchart, block, c.orochi, "{size=28}やばい{/size}");
        AddSay(flowchart, block, c.mitama, "リ、リーリーさんが...");
        AddSay(flowchart, block, c.narukami, "闇堕ち...か");
        AddInvokeMethod(flowchart, block, n.leelee, typeof(NPCController), "Shake", 3f, 0.12f);
        AddSay(flowchart, block, c.leelee, "ぬわーーーーーー！");

        // 闇堕ち: 白フラッシュ + 揺れ + 通常リーリーと巨大リーリーのクロスフェード
        // 透明になったリーリーは CMCamera2 の追従先として残す(消すとカメラが迷子になる)ので、落ちないよう Kinematic にする
        AddPlaySound(flowchart, block, chargeSe, 0.8f);
        AddInvokeMethod(flowchart, block, n.leelee, typeof(Rigidbody2DSwitch), "SetKinematic", true);
        AddFadeScreen(flowchart, block, 0.1f, 1f, Color.white, true);
        AddInvokeMethod(flowchart, block, n.cameraShaker, typeof(CinemachineImpulseSource), "GenerateImpulseWithForce", 1f);
        AddInvokeMethod(flowchart, block, n.leelee, typeof(ManualFadeObject), "FadeOut", 1.5f);
        AddSetActive(flowchart, block, p.giant, true);
        AddInvokeMethod(flowchart, block, p.giant, typeof(ManualFadeObject), "FadeIn", 1.5f);
        AddFadeScreen(flowchart, block, 0.8f, 0f, Color.white, false);
        AddWait(flowchart, block, 1.6f);
        AddInvokeMethod(flowchart, block, p.giant, typeof(ShakeObject), "Shake", 0.06f);
        AddSay(flowchart, block, c.leeleeDark, "ぐぬぬぬぬんうんんぬ！！！！\nカラダが...！ あつい...！");
        AddSay(flowchart, block, c.leeleeDark, "みんな...にげ...ろ...！\nくらあああああああああああああ！！");
        AddInvokeMethod(flowchart, block, p.giant, typeof(ShakeObject), "Stop");

        // 垂直に跳んで画面外へ
        AddPlaySound(flowchart, block, jumpSe, 0.8f);
        AddMoveTo(flowchart, block, p.giant, new Vector3(LeeleeStopX, surfaceY + GiantHalfHeight + OffscreenUp, 0), 0.5f, iTween.EaseType.easeInQuad, false);
        AddWait(flowchart, block, 0.7f);
        AddSetActive(flowchart, block, p.giant, false);

        // カットイン → 巨大リーリーパンチ(一行の真上に拳が落ちて地面が割れる)
        AddInvokeMethod(flowchart, block, p.cutIn, typeof(CutIn), "Play");
        AddWait(flowchart, block, 1.1f);
        AddSetActive(flowchart, block, p.arm, true);
        AddMoveTo(flowchart, block, p.arm, new Vector3(orochiX, surfaceY + 0.1f, 0), 0.25f, iTween.EaseType.easeInQuart, true);
        AddFadeScreen(flowchart, block, 0.05f, 1f, Color.white, true);
        AddInvokeMethod(flowchart, block, n.cameraShaker, typeof(CinemachineImpulseSource), "GenerateImpulseWithForce", 2f);
        AddPlaySound(flowchart, block, impactSe, 1f);
        AddPlaySound(flowchart, block, bombSe, 0.8f);
        AddSetActive(flowchart, block, p.crack, true);
        AddFadeScreen(flowchart, block, 0.4f, 0f, Color.white, false);
        AddWait(flowchart, block, 0.5f);
        AddMoveTo(flowchart, block, p.arm, new Vector3(orochiX, surfaceY + OffscreenUp, 0), 0.5f, iTween.EaseType.easeInQuad, false);
        AddWait(flowchart, block, 0.3f);
        AddBikkuri(flowchart, block, n.akim);
        AddBikkuri(flowchart, block, n.orochi);
        AddBikkuri(flowchart, block, n.mitama);
        AddBikkuri(flowchart, block, n.narukami);
        AddBikkuri(flowchart, block, n.uniki);
        AddWait(flowchart, block, 0.8f);

        // 落下: 落ちる 4 人を Kinematic にして Transform で動かす(「掴んで踏ん張るが沈む」を作るため物理には任せない)
        foreach (GameObject go in new[] { n.akim, n.orochi, n.mitama, n.narukami })
        {
            AddInvokeMethod(flowchart, block, go, typeof(Rigidbody2DSwitch), "SetKinematic", true);
        }
        Vector3 akimPos = new Vector3(akimX, GroundY, 0);
        Vector3 orochiPos = new Vector3(orochiX, GroundY, 0);
        // 見た目の背丈は当たり判定より低い(約1.5ユニット)ので、掴む高さは相手の頭のすぐ上に寄せる
        Vector3 mitamaPos = new Vector3(orochiX, GroundY + 1.4f, 0);        // オロチの頭上で掴む
        Vector3 narukamiPos = new Vector3(akimX - 0.5f, NarukamiY - 1.3f, 0); // アキムの頭上で掴む(ナルカミの原点は左上)
        AddMoveTo(flowchart, block, n.mitama, mitamaPos, 0.4f, iTween.EaseType.easeOutQuad, false);
        AddMoveTo(flowchart, block, n.narukami, narukamiPos, 0.4f, iTween.EaseType.easeOutQuad, false);
        AddWait(flowchart, block, 0.5f);
        AddSetActive(flowchart, block, p.arm, false); // 引き上げ済みの腕を片付ける
        AddSetActive(flowchart, block, p.fakeGround, false);
        AddPlaySound(flowchart, block, hoverSe, 0.5f);
        Vector3 sink = new Vector3(0, -SinkDepth, 0);
        AddMoveTo(flowchart, block, n.akim, akimPos + sink, 1.5f, iTween.EaseType.easeInOutSine, false);
        AddMoveTo(flowchart, block, n.orochi, orochiPos + sink, 1.5f, iTween.EaseType.easeInOutSine, false);
        AddMoveTo(flowchart, block, n.mitama, mitamaPos + sink, 1.5f, iTween.EaseType.easeInOutSine, false);
        AddMoveTo(flowchart, block, n.narukami, narukamiPos + sink, 1.5f, iTween.EaseType.easeInOutSine, false);
        AddWait(flowchart, block, 1.6f);
        Vector3 drop = new Vector3(0, -SinkDepth - FallDepth, 0);
        AddMoveTo(flowchart, block, n.akim, akimPos + drop, 0.6f, iTween.EaseType.easeInQuad, false);
        AddMoveTo(flowchart, block, n.orochi, orochiPos + drop, 0.6f, iTween.EaseType.easeInQuad, false);
        AddMoveTo(flowchart, block, n.mitama, mitamaPos + drop, 0.6f, iTween.EaseType.easeInQuad, false);
        AddMoveTo(flowchart, block, n.narukami, narukamiPos + drop, 0.6f, iTween.EaseType.easeInQuad, false);
        AddWait(flowchart, block, 0.7f);
        foreach (GameObject go in new[] { n.akim, n.orochi, n.mitama, n.narukami })
        {
            AddSetActive(flowchart, block, go, false);
        }
        AddWait(flowchart, block, 0.8f);

        // 遅れて巨大リーリーが逆さで落ちてきて穴へ吸い込まれる
        AddSetActive(flowchart, block, p.giantFalling, true);
        AddMoveTo(flowchart, block, p.giantFalling, new Vector3(HoleCenterX, surfaceY - FallDepth, 0), 1.2f, iTween.EaseType.easeInQuad, true);
        AddSetActive(flowchart, block, p.giantFalling, false);
        AddWait(flowchart, block, 0.8f);

        AddSay(flowchart, block, c.yama, "クククククク...\nハーッハッハッハッハ...！");
        AddWait(flowchart, block, 0.5f);
        AddInvokeMethod(flowchart, block, p.bgmWrapper, typeof(BGMWrapper), "FadeOut", 2f);
        AddFadeScreen(flowchart, block, 2f, 1f, Color.black, true);
        AddComment(flowchart, block, "TODO(#641): 次シーン(Coming Soon / 体験版終了)への遷移は体験版終了の実装時に追加する");
    }

    // ---- 後半で使う小道具 ----

    class Props
    {
        public GameObject seed, giant, giantFalling, arm, crack, cutIn, fakeGround, bgmWrapper;
        public float tileSurfaceY;
    }

    static Props SetupProps(UnityEngine.SceneManagement.Scene scene, Npcs n)
    {
        Props p = new Props { bgmWrapper = FindInScene(scene, "BGMWrapper") };
        if (p.bgmWrapper == null)
        {
            Debug.LogError("[Issue641] BGMWrapper が見つかりません。");
            return null;
        }

        // 落ちる側と透明化するリーリーに物理切替を、リーリーにフェードを、直組み/変種の NPC にビックリを持たせる
        foreach (GameObject go in new[] { n.leelee, n.akim, n.orochi, n.mitama, n.narukami })
        {
            EnsureComponent<Rigidbody2DSwitch>(go);
        }
        EnsureComponent<ManualFadeObject>(n.leelee);
        foreach (GameObject go in new[] { n.akim, n.orochi, n.mitama, n.narukami, n.uniki })
        {
            EnsureEmotion(go);
        }

        p.fakeGround = CarveHole(scene, out p.tileSurfaceY);
        Debug.Log($"[Issue641] タイル上面 y={p.tileSurfaceY}、NPC の当たり判定下端 y={GroundSurfaceY}");

        SpriteRenderer npcRenderer = AssetDatabase.LoadAssetAtPath<GameObject>(NpcPrefabPath).GetComponent<SpriteRenderer>();
        int layer = npcRenderer.sortingLayerID;

        p.seed = CopySeedFromYami5F(scene);
        if (p.seed != null) p.seed.transform.position = new Vector3(LeeleeStopX, GroundY + SeedOffsetY, 0);

        // NPC の見た目の足元はタイル上面に一致する(当たり判定の下端はそれより下)ので、足元はタイル上面に合わせる
        p.giant = CreateSpriteObject(scene, GiantName, GiantSpritePath, layer, 3, new Vector3(LeeleeStopX, p.tileSurfaceY + GiantHalfHeight, 0), GiantScale);
        p.giant.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0); // FadeIn で現す
        p.giant.AddComponent<ManualFadeObject>();
        p.giant.AddComponent<ShakeObject>();
        p.giant.SetActive(false);

        p.giantFalling = CreateSpriteObject(scene, GiantFallingName, GiantSpritePath, layer, 3, new Vector3(HoleCenterX, p.tileSurfaceY + OffscreenUp, 0), GiantScale);
        p.giantFalling.transform.rotation = Quaternion.Euler(0, 0, 180); // 逆さ
        p.giantFalling.SetActive(false);

        float orochiX = LeeleeStopX + PartyGap * 2;
        p.arm = CreateSpriteObject(scene, ArmName, ArmSpritePath, layer, 5, new Vector3(orochiX, p.tileSurfaceY + OffscreenUp, 0), 1f); // 原点は拳の下端
        p.arm.SetActive(false);

        p.crack = CreateSpriteObject(scene, CrackName, CrackSpritePath, layer, -1, new Vector3(orochiX, p.tileSurfaceY + 0.35f, 0), 1f);
        p.crack.SetActive(false);

        p.cutIn = CreateCutIn(scene);
        return p;
    }

    // 穴のセルのタイルを本物の Tilemap から FakeGround へ移す。イベント中に FakeGround を消すと穴が開く
    static GameObject CarveHole(UnityEngine.SceneManagement.Scene scene, out float tileSurfaceY)
    {
        tileSurfaceY = GroundSurfaceY;
        GameObject groundGo = FindInScene(scene, GroundTilemapName);
        Tilemap ground = groundGo != null ? groundGo.GetComponent<Tilemap>() : null;
        if (ground == null)
        {
            Debug.LogError("[Issue641] Ground の Tilemap が見つかりません。");
            return null;
        }
        GameObject fakeGo = new GameObject(FakeGroundName);
        fakeGo.transform.SetParent(groundGo.transform.parent, false);
        fakeGo.tag = groundGo.tag;
        fakeGo.layer = groundGo.layer;
        Tilemap fake = fakeGo.AddComponent<Tilemap>();
        fake.tileAnchor = ground.tileAnchor;
        TilemapRenderer fakeRenderer = fakeGo.AddComponent<TilemapRenderer>();
        EditorUtility.CopySerialized(groundGo.GetComponent<TilemapRenderer>(), fakeRenderer);
        TilemapCollider2D fakeCollider = fakeGo.AddComponent<TilemapCollider2D>();
        EditorUtility.CopySerialized(groundGo.GetComponent<TilemapCollider2D>(), fakeCollider);

        BoundsInt bounds = ground.cellBounds;
        int moved = 0;
        int topY = int.MinValue;
        // 屋根や上空のブロックは残し、NPC が立つ高さより下の行だけをくり抜く
        int yMax = Mathf.Min(bounds.yMax, Mathf.FloorToInt(GroundY));
        for (int x = HoleCellMin; x <= HoleCellMax; x++)
        {
            for (int y = bounds.yMin; y < yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                TileBase tile = ground.GetTile(cell);
                if (tile == null) continue;
                fake.SetTile(cell, tile);
                fake.SetTransformMatrix(cell, ground.GetTransformMatrix(cell));
                ground.SetTile(cell, null);
                moved++;
                topY = Mathf.Max(topY, y);
            }
        }
        if (moved == 0)
        {
            Debug.LogError("[Issue641] 穴の位置にタイルがありません。");
            return fakeGo;
        }
        // Grid は原点・セル 1 ユニットなので、セルの上端 = y+1
        tileSurfaceY = topY + 1;
        Debug.Log($"[Issue641] 穴: {moved} タイルを {FakeGroundName} へ移しました。");

        // 穴の奥(黒)。タイルより後ろに描く
        GameObject back = CreateSpriteObject(scene, HoleBackName, HoleSpritePath, fakeRenderer.sortingLayerID, fakeRenderer.sortingOrder - 1,
            new Vector3(HoleCenterX, tileSurfaceY - FallDepth / 2f, 0), 1f);
        back.transform.localScale = new Vector3(HoleWidth, FallDepth, 1); // hole_black は 1 ユニット四方(PPU 4)
        return fakeGo;
    }

    static void RestoreHole(Tilemap fake, Tilemap ground)
    {
        if (fake == null || ground == null) return;
        int restored = 0;
        foreach (Vector3Int cell in fake.cellBounds.allPositionsWithin)
        {
            TileBase tile = fake.GetTile(cell);
            if (tile == null) continue;
            ground.SetTile(cell, tile);
            ground.SetTransformMatrix(cell, fake.GetTransformMatrix(cell));
            restored++;
        }
        Debug.Log($"[Issue641] 穴: {restored} タイルを Tilemap へ戻しました。");
    }

    // 種は Yami5F のシーン直組み(紫ティント + オーラ粒子 + FadeInSprite/ManualFadeObject)をそのまま複製する
    static GameObject CopySeedFromYami5F(UnityEngine.SceneManagement.Scene scene)
    {
        var source = EditorSceneManager.OpenScene(Yami5FScenePath, OpenSceneMode.Additive);
        try
        {
            GameObject original = source.GetRootGameObjects().FirstOrDefault(g => g.name == SeedName);
            if (original == null)
            {
                Debug.LogError($"[Issue641] Yami5F に {SeedName} がありません。");
                return null;
            }
            GameObject copy = Object.Instantiate(original);
            copy.name = SeedName;
            MoveToScene(copy, scene);
            return copy;
        }
        finally
        {
            EditorSceneManager.CloseScene(source, true);
        }
    }

    static GameObject CreateSpriteObject(UnityEngine.SceneManagement.Scene scene, string name, string spritePath, int sortingLayerID, int sortingOrder, Vector3 position, float scale)
    {
        GameObject go = new GameObject(name);
        MoveToScene(go, scene);
        go.transform.position = position;
        go.transform.localScale = new Vector3(scale, scale, 1);
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (renderer.sprite == null) Debug.LogError($"[Issue641] スプライトが見つかりません: {spritePath}");
        renderer.sortingLayerID = sortingLayerID;
        renderer.sortingOrder = sortingOrder;
        return go;
    }

    // カットイン: 全画面 Canvas に「斜めの暗帯 + 赤い線 + 怒り顔」を Content としてまとめ、CutIn.Play() で走らせる
    static GameObject CreateCutIn(UnityEngine.SceneManagement.Scene scene)
    {
        GameObject go = new GameObject(CutInName);
        MoveToScene(go, scene);
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5; // SayDialog(1) より手前
        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform content = CreateRect("Content", go.transform);
        content.anchorMin = Vector2.zero;
        content.anchorMax = Vector2.one;
        content.offsetMin = Vector2.zero;
        content.offsetMax = Vector2.zero;

        Image band = CreateImage("Band", content, null, new Color(0.06f, 0f, 0.08f, 0.9f), new Vector2(3400, 560), Vector2.zero);
        band.rectTransform.localRotation = Quaternion.Euler(0, 0, -7);
        Color red = new Color(0.9f, 0.08f, 0.08f, 1f);
        CreateImage("LineTop", band.rectTransform, null, red, new Vector2(3400, 22), new Vector2(0, 269));
        CreateImage("LineBottom", band.rectTransform, null, red, new Vector2(3400, 22), new Vector2(0, -269));
        Sprite face = AssetDatabase.LoadAssetAtPath<Sprite>(DarkFacePath);
        if (face == null) Debug.LogError($"[Issue641] 怒り顔が見つかりません: {DarkFacePath}");
        Image faceImage = CreateImage("Face", content, face, Color.white, new Vector2(720, 720), new Vector2(140, 0));
        faceImage.preserveAspect = true;

        CutIn cutIn = go.AddComponent<CutIn>();
        SerializedObject so = new SerializedObject(cutIn);
        so.FindProperty("content").objectReferenceValue = content;
        so.FindProperty("se").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(CutInSePath);
        so.ApplyModifiedPropertiesWithoutUndo();
        content.gameObject.SetActive(false);
        return go;
    }

    static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    static Image CreateImage(string name, Transform parent, Sprite sprite, Color color, Vector2 size, Vector2 anchoredPosition)
    {
        RectTransform rect = CreateRect(name, parent);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    static T EnsureComponent<T>(GameObject go) where T : Component
    {
        if (go == null) return null;
        T component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }

    // Emotion が無い NPC に追加する。SE が null だと Bikkuri() の PlayClipAtPoint で例外になり
    // Fungus の InvokeMethod がそこで止まるので、必ず SE を入れる(既存の Emotion も null なら補う)
    static void EnsureEmotion(GameObject go)
    {
        if (go == null) return;
        Emotion emotion = go.GetComponent<Emotion>();
        if (emotion == null) emotion = go.AddComponent<Emotion>();
        SerializedObject so = new SerializedObject(emotion);
        SerializedProperty se = so.FindProperty("se");
        if (se.objectReferenceValue == null)
        {
            se.objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(EmotionSePath);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    // 新規スプライトの取り込み設定(既存キャラと同じ PPU 100・Point・非圧縮)。腕だけ原点を拳の下端にする。
    // 闇堕ち顔は原型 leelee.png と同じ滑らかな線画なので Bilinear にする
    static void EnsureSpriteImports()
    {
        EnsureSpriteImport(GiantSpritePath, 100, SpriteAlignment.Center);
        EnsureSpriteImport(DarkFacePath, 100, SpriteAlignment.Center, FilterMode.Bilinear);
        EnsureSpriteImport(ArmSpritePath, 100, SpriteAlignment.BottomCenter);
        EnsureSpriteImport(CrackSpritePath, 100, SpriteAlignment.Center);
        EnsureSpriteImport(HoleSpritePath, 4, SpriteAlignment.Center);
    }

    static void EnsureSpriteImport(string path, int pixelsPerUnit, SpriteAlignment alignment, FilterMode filterMode = FilterMode.Point)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"[Issue641] 画像が見つかりません: {path}");
            return;
        }
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        bool changed = importer.textureType != TextureImporterType.Sprite
            || importer.spriteImportMode != SpriteImportMode.Single
            || !Mathf.Approximately(importer.spritePixelsPerUnit, pixelsPerUnit)
            || importer.filterMode != filterMode
            || importer.textureCompression != TextureImporterCompression.Uncompressed
            || importer.mipmapEnabled
            || settings.spriteAlignment != (int)alignment;
        if (!changed) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.filterMode = filterMode;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 2048;
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)alignment;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
        Debug.Log($"[Issue641] 取り込み設定を更新しました: {path}");
    }

    // AllFaces に闇堕ちリーリー(名札は「VLリーリー」のまま、顔グラは怒り顔)を追加する
    static void EnsureDarkLeeleeCharacter()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(AllFacesPrefabPath);
        try
        {
            if (FindCharacter(root, DarkLeeleeCharacterName) != null) return;
            Character leelee = FindCharacter(root, "LeeleeCharacter");
            Sprite face = AssetDatabase.LoadAssetAtPath<Sprite>(DarkFacePath);
            if (leelee == null || face == null)
            {
                Debug.LogError($"[Issue641] LeeleeCharacter または怒り顔が見つかりません。leelee={leelee} face={face}");
                return;
            }
            GameObject go = new GameObject(DarkLeeleeCharacterName);
            go.transform.SetParent(root.transform, false);
            Character dark = go.AddComponent<Character>();
            EditorUtility.CopySerialized(leelee, dark);
            SerializedObject so = new SerializedObject(dark);
            so.FindProperty("nameText").stringValue = "VLリーリー";
            SerializedProperty portraits = so.FindProperty("portraits");
            portraits.arraySize = 1;
            portraits.GetArrayElementAtIndex(0).objectReferenceValue = face;
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, AllFacesPrefabPath);
            Debug.Log($"[Issue641] AllFaces に {DarkLeeleeCharacterName} を追加しました。");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
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

    static T AddCommand<T>(Flowchart flowchart, Block block) where T : Command
    {
        T command = flowchart.gameObject.AddComponent<T>();
        command.ItemId = flowchart.NextItemId();
        command.ParentBlock = block;
        block.CommandList.Add(command);
        return command;
    }

    static void AddBikkuri(Flowchart flowchart, Block block, GameObject go)
    {
        AddInvokeMethod(flowchart, block, go, typeof(Emotion), "Bikkuri");
    }

    static void AddSetActive(Flowchart flowchart, Block block, GameObject target, bool active)
    {
        if (target == null)
        {
            Debug.LogError("[Issue641] SetActive の対象がありません。");
            return;
        }
        SetActive command = AddCommand<SetActive>(flowchart, block);
        SerializedObject so = new SerializedObject(command);
        so.FindProperty("_targetGameObject").FindPropertyRelative("gameObjectVal").objectReferenceValue = target;
        so.FindProperty("activeState").FindPropertyRelative("booleanVal").boolValue = active;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // iTween の MoveTo(ワールド座標)。waitUntilFinished=false なら並行して動かせる
    static void AddMoveTo(Flowchart flowchart, Block block, GameObject target, Vector3 position, float duration, iTween.EaseType easeType, bool waitUntilFinished)
    {
        if (target == null)
        {
            Debug.LogError("[Issue641] MoveTo の対象がありません。");
            return;
        }
        MoveTo command = AddCommand<MoveTo>(flowchart, block);
        SerializedObject so = new SerializedObject(command);
        so.FindProperty("_targetObject").FindPropertyRelative("gameObjectVal").objectReferenceValue = target;
        so.FindProperty("_toPosition").FindPropertyRelative("vector3Val").vector3Value = position;
        so.FindProperty("_duration").FindPropertyRelative("floatVal").floatValue = duration;
        so.FindProperty("easeType").intValue = (int)easeType;
        so.FindProperty("waitUntilFinished").boolValue = waitUntilFinished;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void AddFadeScreen(Flowchart flowchart, Block block, float duration, float targetAlpha, Color color, bool waitUntilFinished)
    {
        FadeScreen command = AddCommand<FadeScreen>(flowchart, block);
        SerializedObject so = new SerializedObject(command);
        so.FindProperty("duration").floatValue = duration;
        so.FindProperty("targetAlpha").floatValue = targetAlpha;
        so.FindProperty("fadeColor").colorValue = color;
        so.FindProperty("waitUntilFinished").boolValue = waitUntilFinished;
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
                case Vector2 v2: objValue.FindPropertyRelative("vector2Value").vector2Value = v2; break;
                case Vector3 v: objValue.FindPropertyRelative("vector3Value").vector3Value = v; break;
                case System.Enum e: objValue.FindPropertyRelative("intValue").intValue = System.Convert.ToInt32(e); break;
                case Object o: objValue.FindPropertyRelative("objectValue").objectReferenceValue = o; break;
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

    // プレイモード検証用: カットインだけを再生する
    [MenuItem("Tools/Issue641/Debug/Play CutIn")]
    public static void DebugPlayCutIn()
    {
        CutIn cutIn = Object.FindObjectOfType<CutIn>(true);
        if (cutIn == null)
        {
            Debug.LogWarning("[Issue641] CutIn がシーンにありません。");
            return;
        }
        cutIn.Play();
    }

    // プレイモード検証用: シーンを開く(エディットモード)
    [MenuItem("Tools/Issue641/Debug/Open Yama Revenge Scene")]
    public static void DebugOpenScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }
}
