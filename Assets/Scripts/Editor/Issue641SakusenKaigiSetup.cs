using System.Linq;
using System.Reflection;
using Cinemachine;
using Fungus;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using VLCNP.Control;
using VLCNP.Core;
using VLCNP.Movie;
using VLCNP.SceneManagement;
using VLCNP.UI;

/**
 * Issue #641: イベント「ウニキ家にて作戦会議」のセットアップ。
 * Tools/Issue641/Setup Sakusen Kaigi Event で実行する(シーンは自動で開く)。
 * - UnikiRoom の位置マーカー UnikiSakusenKaigi を、話しかけ可能なウニキNPCとして構築する
 * - 3人(オロチ・ミタマ・ナルカミ)加入済みのときだけ表示され、話しかけると作戦会議の会話が発動する
 * - 末尾で VeryLongFarm_Yama_Revenge(ヤーマ襲来)へ遷移する(専用の GameEventTransition を置く)
 * 再実行可能(生成物は毎回作り直す。Wait の秒数などの手直しはこのスクリプトに反映すること)。
 */
public static class Issue641SakusenKaigiSetup
{
    const string ScenePath = "Assets/Scenes/UnikiRoom.unity";
    const string TargetName = "UnikiSakusenKaigi";
    const string TemplateName = "Uni";
    const string BlockName = "SakusenKaigi";
    const string AllFacesPrefabPath = "Assets/Game/Characters/Face/AllFaces.prefab";
    const string EmotionSeGuid = "d186235e0361445b8affabe74e632c81";
    const string TransitionPrefabPath = "Assets/Game/Core/GameEventTransition.prefab";
    const string TransitionName = "ToYamaRevengeGameEventTransition";

    [MenuItem("Tools/Issue641/Setup Sakusen Kaigi Event")]
    public static void Setup()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject target = FindInScene(scene, TargetName);
        GameObject template = FindInScene(scene, TemplateName);
        if (target == null || template == null)
        {
            Debug.LogError($"[Issue641] {TargetName} または {TemplateName} が見つかりません。");
            return;
        }

        Clean(target);
        CopyTemplateComponents(target, template);
        GameObject emotionGo = CreateEmotionChild(target);
        GameObject transitionGo = SetupTransition(scene);
        if (transitionGo == null) return;
        Flowchart flowchart = CreateChat(scene, target, emotionGo, transitionGo);
        SetupGameEvent(target, flowchart);
        SetupVisibility(target);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Issue641] 作戦会議イベントのセットアップが完了しました。");
    }

    // 再実行できるよう、Transform 以外のコンポーネントと子を全て削除する
    static void Clean(GameObject target)
    {
        // MultiFlagGameEvent は InformationTextSpawner に依存(RequireComponent)するので先に消す
        foreach (Component c in target.GetComponents<Component>().OrderByDescending(c => c is MultiFlagGameEvent))
        {
            if (c is Transform) continue;
            Object.DestroyImmediate(c);
        }
        for (int i = target.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(target.transform.GetChild(i).gameObject);
        }
    }

    // 既存 Uni から見た目・当たり判定・情報テキスト・カメラ揺れの設定をコピーする
    static void CopyTemplateComponents(GameObject target, GameObject template)
    {
        target.transform.localScale = template.transform.localScale;
        target.layer = template.layer;
        target.tag = template.tag;
        CopyComponent<SpriteRenderer>(template, target);
        CopyComponent<BoxCollider2D>(template, target);
        CopyComponent<InformationTextSpawner>(template, target);
        CopyComponent<CinemachineImpulseSource>(template, target);
    }

    static T CopyComponent<T>(GameObject from, GameObject to) where T : Component
    {
        T source = from.GetComponent<T>();
        if (source == null)
        {
            Debug.LogError($"[Issue641] {from.name} に {typeof(T).Name} がありません。");
            return null;
        }
        T dest = to.AddComponent<T>();
        EditorUtility.CopySerialized(source, dest);
        return dest;
    }

    static GameObject CreateEmotionChild(GameObject target)
    {
        GameObject go = new GameObject("Emotion");
        go.transform.SetParent(target.transform, false);
        // NPCWithController プレハブと同じ配置(親スケール0.6の打ち消し込み)
        go.transform.localPosition = new Vector3(0, 1.83f, 0);
        go.transform.localScale = new Vector3(1.6666665f, 1.6666665f, 2.6666667f);
        Emotion emotion = go.AddComponent<Emotion>();
        string sePath = AssetDatabase.GUIDToAssetPath(EmotionSeGuid);
        AudioClip se = AssetDatabase.LoadAssetAtPath<AudioClip>(sePath);
        if (se == null) Debug.LogError("[Issue641] Emotion 用SEが見つかりません。");
        SerializedObject so = new SerializedObject(emotion);
        so.FindProperty("se").objectReferenceValue = se;
        so.ApplyModifiedPropertiesWithoutUndo();
        return go;
    }

    static Flowchart CreateChat(UnityEngine.SceneManagement.Scene scene, GameObject target, GameObject emotionGo, GameObject transitionGo)
    {
        GameObject chatGo = new GameObject("Chat");
        chatGo.transform.SetParent(target.transform, false);
        Flowchart flowchart = chatGo.AddComponent<Flowchart>();
        chatGo.AddComponent<FlowchartStopAllGuard>();

        // 会話キャラ(顔グラ)。AllFaces に全員入っている
        GameObject allFacesGo = InstantiatePrefab(scene, target, AllFacesPrefabPath);
        Character leelee = FindCharacter(allFacesGo, "LeeleeCharacter");
        Character uniki = FindCharacter(allFacesGo, "UniCharacterWithFace");
        Character orochi = FindCharacter(allFacesGo, "OrochiCharacter");
        Character mitama = FindCharacter(allFacesGo, "MitamaCharacter");
        Character narukami = FindCharacter(allFacesGo, "NarukamiCharacter");
        if (leelee == null || uniki == null || orochi == null || mitama == null || narukami == null)
        {
            Debug.LogError("[Issue641] AllFaces 内の会話キャラが見つかりません。");
            return flowchart;
        }
        SayDialog sayDialog = FindSceneSayDialog(scene);
        foreach (Character c in new[] { leelee, uniki, orochi, mitama, narukami })
        {
            SetSayDialog(c, sayDialog);
        }

        Block block = flowchart.CreateBlock(new Vector2(50, 50));
        block.BlockName = BlockName;
        AddSay(flowchart, block, leelee, "よーっす、ウニ！\n全員連れてきたで！");
        AddSay(flowchart, block, uniki, "おお、すごい！\nきみたちが噂の...！");
        AddSay(flowchart, block, leelee, "そや、こいつらがベリーロングCNPや。\nほら挨拶せえ");
        AddSay(flowchart, block, orochi, "{size=28}...どうも{/size}");
        AddSay(flowchart, block, mitama, "なんかこの家 磯臭いですねえ...\n海の生き物飼ってます？");
        AddSay(flowchart, block, narukami, "ふむ。これはウニの匂い。それがしは\nタカなのでウニはさすがに食えんな");
        AddWait(flowchart, block, 1f);
        AddSay(flowchart, block, uniki, "なんか全体的に頼りなくない？\nえ？ 大丈夫？");
        AddSay(flowchart, block, leelee, "ま、まあ目覚めたばっかやからな。\nそれよりVLヤーマの居場所を教えてくれ。");
        AddSay(flowchart, block, leelee, "昔の同僚全員でバッキバキや！\nお礼参りしたる");
        AddSay(flowchart, block, uniki, "そうだったね。ええと、VLヤーマは\n「ベリーロングパレス」という\nところにいるんだ。");
        AddSay(flowchart, block, leelee, "ベリーロングパレスな、おおきに。\nほな いこか");
        AddInvokeMethod(flowchart, block, emotionGo, typeof(Emotion), "Bikkuri");
        AddWait(flowchart, block, 0.7f);
        AddSay(flowchart, block, uniki, "ちょっと待ってよ！\nそんな準備もなしに");
        AddSay(flowchart, block, leelee, "ん？ 大丈夫やろ。\nわしら腐っても天下のCNPやで。\nそれが4人もおるんやから");
        AddSay(flowchart, block, uniki, "だとしても作戦を立てていこうよ！\nあっちはVLヤーマの他にシモーヌや\nその手下たちもいるんだ");
        AddSay(flowchart, block, uniki, "うかつに突っ込んだら\n死にに行くようなもんだよ");
        AddSay(flowchart, block, leelee, "しかしなあ。そうこうしてるうちに、やつら、\nもっかいここに来るかもしれんで");
        AddWait(flowchart, block, 1f);
        AddScreamEffects(flowchart, block, target);
        AddSay(flowchart, block, null, "きゃーーーーー！！");
        AddWait(flowchart, block, 2f);
        AddSay(flowchart, block, uniki, "デジャブ...！！ Millcoさん！！！");
        AddInvokeMethod(flowchart, block, transitionGo, typeof(TransitionEvent), "ExecuteTransition");

        return flowchart;
    }

    // ヤーマ襲来シーンへの遷移。既存 ToBossGameEventTransition と同じプレハブで、遷移先だけ新シーンにする
    static GameObject SetupTransition(UnityEngine.SceneManagement.Scene scene)
    {
        int buildIndex = Issue641YamaRevengeSetup.GetBuildIndex();
        if (buildIndex < 0)
        {
            Debug.LogError("[Issue641] VeryLongFarm_Yama_Revenge が Build Settings にありません。先に Tools/Issue641/Setup Yama Revenge Scene を実行してください。");
            return null;
        }
        GameObject existing = FindInScene(scene, TransitionName);
        if (existing != null) Object.DestroyImmediate(existing);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TransitionPrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[Issue641] プレハブが見つかりません: {TransitionPrefabPath}");
            return null;
        }
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        go.name = TransitionName;
        TransitionEvent transition = go.GetComponent<TransitionEvent>();
        SerializedObject so = new SerializedObject(transition);
        so.FindProperty("sceneToLoad").intValue = buildIndex;
        so.FindProperty("isAutoSave").boolValue = false;
        so.FindProperty("fadeWaitTime").floatValue = 1f;
        so.ApplyModifiedPropertiesWithoutUndo();
        return go;
    }

    // 既存 Uni の悲鳴演出と同じ: カメラ揺れ + BGMフェードアウト
    static void AddScreamEffects(Flowchart flowchart, Block block, GameObject target)
    {
        InvokeEvent invokeEvent = flowchart.gameObject.AddComponent<InvokeEvent>();
        invokeEvent.ItemId = flowchart.NextItemId();
        invokeEvent.ParentBlock = block;
        FieldInfo field = typeof(InvokeEvent).GetField("staticEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        UnityEvent staticEvent = (UnityEvent)field.GetValue(invokeEvent);
        CinemachineImpulseSource impulse = target.GetComponent<CinemachineImpulseSource>();
        if (impulse != null)
        {
            UnityEventTools.AddVoidPersistentListener(staticEvent, impulse.GenerateImpulse);
        }
        BGMWrapper bgm = Object.FindObjectOfType<BGMWrapper>(true);
        if (bgm != null)
        {
            UnityEventTools.AddFloatPersistentListener(staticEvent, bgm.FadeOut, 1f);
        }
        else
        {
            Debug.LogWarning("[Issue641] BGMWrapper が見つからないため、BGMフェードアウトは未設定です。");
        }
        block.CommandList.Add(invokeEvent);
    }

    // 3人加入済みで話しかけたときに会話ブロックを実行する
    static void SetupGameEvent(GameObject target, Flowchart flowchart)
    {
        MultiFlagGameEvent gameEvent = target.AddComponent<MultiFlagGameEvent>();
        gameEvent.flowChart = flowchart;
        gameEvent.InformationText = "(↑) はなす";
        SerializedObject so = new SerializedObject(gameEvent);
        SerializedProperty list = so.FindProperty("flagConditionToBlockName");
        list.arraySize = 1;
        SerializedProperty element = list.GetArrayElementAtIndex(0);
        SerializedProperty conditions = element.FindPropertyRelative("conditions");
        conditions.arraySize = 3;
        SetCondition(conditions.GetArrayElementAtIndex(0), Flag.VLOrochiJoined, true);
        SetCondition(conditions.GetArrayElementAtIndex(1), Flag.VLMitamaJoined, true);
        SetCondition(conditions.GetArrayElementAtIndex(2), Flag.VLNarukamiJoined, true);
        element.FindPropertyRelative("blockName").stringValue = BlockName;
        element.FindPropertyRelative("isAutoStart").boolValue = false;
        element.FindPropertyRelative("isCollisionStart").boolValue = false;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // デフォルト非表示、3人加入済みのときだけ表示
    static void SetupVisibility(GameObject target)
    {
        VisibilityFlagManager manager = target.AddComponent<VisibilityFlagManager>();
        SerializedObject so = new SerializedObject(manager);
        SerializedProperty list = so.FindProperty("visibilityFlags");
        list.arraySize = 2;
        SerializedProperty hidden = list.GetArrayElementAtIndex(0);
        hidden.FindPropertyRelative("flag").intValue = (int)Flag.None;
        hidden.FindPropertyRelative("additionalFlags").arraySize = 0;
        hidden.FindPropertyRelative("isVisible").boolValue = false;
        SerializedProperty visible = list.GetArrayElementAtIndex(1);
        visible.FindPropertyRelative("flag").intValue = (int)Flag.VLOrochiJoined;
        SerializedProperty additional = visible.FindPropertyRelative("additionalFlags");
        additional.arraySize = 2;
        additional.GetArrayElementAtIndex(0).intValue = (int)Flag.VLMitamaJoined;
        additional.GetArrayElementAtIndex(1).intValue = (int)Flag.VLNarukamiJoined;
        visible.FindPropertyRelative("isVisible").boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static GameObject InstantiatePrefab(UnityEngine.SceneManagement.Scene scene, GameObject parent, string prefabPath)
    {
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

    static void AddInvokeMethod(Flowchart flowchart, Block block, GameObject targetObject, System.Type componentType, string methodName)
    {
        InvokeMethod invoke = flowchart.gameObject.AddComponent<InvokeMethod>();
        invoke.ItemId = flowchart.NextItemId();
        invoke.ParentBlock = block;
        SerializedObject so = new SerializedObject(invoke);
        so.FindProperty("targetObject").objectReferenceValue = targetObject;
        so.FindProperty("targetComponentAssemblyName").stringValue = componentType.AssemblyQualifiedName;
        so.FindProperty("targetComponentFullname").stringValue = "UnityEngine.Component[]";
        so.FindProperty("targetComponentText").stringValue = componentType.Name;
        so.FindProperty("targetMethod").stringValue = methodName;
        so.FindProperty("targetMethodText").stringValue = $"{methodName} (): Void";
        so.FindProperty("methodParameters").arraySize = 0;
        so.ApplyModifiedPropertiesWithoutUndo();
        block.CommandList.Add(invoke);
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

    // プレイモード検証用: 3人加入 + Millco/既存Uni非表示化フラグを立てる
    [MenuItem("Tools/Issue641/Debug/Set Sakusen Kaigi Flags")]
    public static void DebugSetFlags()
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
        flagManager.SetFlag(Flag.IkehayaBlockChainChated, true);
        Debug.Log("[Issue641] 3人加入 + IkehayaBlockChainChated をONにしました。");
    }

    // プレイモード検証用: 話しかけたときと同じ経路で会話を発火する
    [MenuItem("Tools/Issue641/Debug/Execute Sakusen Kaigi Event")]
    public static void DebugExecuteEvent()
    {
        GameObject target = GameObject.Find(TargetName);
        if (target == null)
        {
            Debug.LogError($"[Issue641] {TargetName} が見つかりません(非表示の可能性)。");
            return;
        }
        target.GetComponent<MultiFlagGameEvent>().Execute();
        Debug.Log("[Issue641] 作戦会議イベントを発火しました。");
    }
}
