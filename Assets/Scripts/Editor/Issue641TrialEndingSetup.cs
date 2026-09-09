using System;
using System.Linq;
using System.Reflection;
using Cinemachine;
using Fungus;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VLCNP.UI;
using VLCNP.Control;
using VLCNP.Core;
using VLCNP.Movie;
using VLCNP.SceneManagement;
using Object = UnityEngine.Object;
using Scene = UnityEngine.SceneManagement.Scene;

namespace VLCNP.Editor
{
    /** #641: 体験版完了挨拶の舞台と、ヤーマ襲来からの遷移を作る。 */
    public static class Issue641TrialEndingSetup
    {
        public const string ScenePath = "Assets/Scenes/TrialEnding_3.unity";
        const string TransitionName = "ToTrialEnding3";
        const string NpcPath = "Assets/Game/Characters/Npc/NPCWithController.prefab";

        [MenuItem("Tools/Issue641/Setup Trial Ending 3")]
        public static void Setup()
        {
            if (Enumerable.Range(0, UnityEngine.SceneManagement.SceneManager.sceneCount)
                .Any(i => UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty))
                throw new InvalidOperationException("未保存シーンがあります。保存後に実行してください。");
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
                throw new InvalidOperationException("TrialEnding_3 は既にあります。手編集を守るため再作成しません。");
            if (!AssetDatabase.CopyAsset("Assets/Scenes/TrialEnding.unity", ScenePath))
                throw new InvalidOperationException("TrialEnding の複製に失敗しました。");
            var settings = EditorBuildSettings.scenes.ToList();
            settings.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = settings.ToArray();
            var scene = EditorSceneManager.OpenScene(ScenePath);
            string[] keep = { "Core", "Grid", "TransitionSpawnPoint", "_FungusState", "BackGround", "Haikei", "BackGroundObject" };
            foreach (var root in scene.GetRootGameObjects())
                if (!keep.Contains(root.name)) Object.DestroyImmediate(root);
            var core = Find(scene, "Core");
            var promo = core.transform.Find("CMCamera/Canvas");
            if (promo != null) Object.DestroyImmediate(promo.gameObject);

            PlaceCast(scene);
            SetupCamera(scene);
            Find(scene, "TransitionSpawnPoint").transform.position = new Vector3(0, -16, 0);
            HidePartyInScene(scene);
            SetupStart(scene);
            BuildGreeting(scene);
            RecordOverrides(scene);
            EditorSceneManager.SaveScene(scene);

            var previous = EditorSceneManager.OpenScene(Issue641YamaRevengeSetup.ScenePath);
            ConnectPrevious(previous);
            EditorSceneManager.SaveScene(previous);
            EditorSceneManager.OpenScene(ScenePath);
            Debug.Log("[Issue641] TrialEnding_3 と前シーンの遷移を保存しました。");
        }

        /**
         * 配置の正。TrialEnding と同じ構図: 植物VLCNP が後列、アキムと闇堕ちリーリーが中央、ヤーマとカルマは右端に離す。
         * 座標は 2026-09-06 にユーザーがエディタで手置きした値をそのまま持つ(足元はスプライトの透明余白込みで見た目合わせ)。
         * faceLeft は X スケールを反転して左を向かせる。
         */
        const string AnimDir = "Assets/Game/Characters/Animations/";
        static readonly (string name, string file, string sprite, string animator, float x, float y, float scale, bool faceLeft, int order)[] Cast =
        {
            ("Plant_VLCNP_Orochi", "plant_vlcnp_orochi.png", "plant_vlcnp_orochi_0", AnimDir + "plant_vlcnp_orochi_0.controller", -14.4f, -15.29576f, .327f, false, 1),
            ("Plant_VLCNP_Narukami", "plant_vlcnp_narukami.png", "plant_vlcnp_narukami_0", AnimDir + "plant_vlcnp_narukami_0.controller", -13.04f, -15.29576f, .327f, false, 1),
            ("Plant_VLCNP_Mitama", "plant_vlcnp_mitama.png", "plant_vlcnp_mitama_0", AnimDir + "plant_vlcnp_mitama_0.controller", -4.3f, -15.29576f, .327f, false, 1),
            ("Plant_VLCNP_Leelee", "plant_vlcnp_leelee.png", "plant_vlcnp_leelee_0", AnimDir + "plant_vlcnp_leelee_0.controller", -2.75f, -15.29576f, .327f, false, 1),
            ("VLMitama", "VLMitama.png", "VLMitama_1", AnimDir + "VLMitamaAnimator.overrideController", -11.1f, -16.238f, .6f, true, 5),
            ("Akim", "akim.png", "akim_0", "Assets/Game/Characters/PlayerLevel1.overrideController", -9.86f, -16.29f, .6f, true, 5),
            ("DarkLeeleeGiant", "dark_giant_leelee_512x640.png", "dark_giant_leelee_512x640", null, -7.91f, -14.41f, .8f, false, 5),
            ("VLOrochi", "VLOrochiNPC.png", "VLOrochiNPC_0", AnimDir + "OrochiAnimator.overrideController", -6.18f, -16.25f, .6f, false, 5),
            ("VLNarukami", "VLNarukami.png", "VLNarukami_0", "Assets/Game/Characters/Npc/NPCVLNarukamiFlyController.controller", -5.98f, -14.24f, .4f, false, 5),
            ("VLYama", "VLYama.png", "VLYama_0", AnimDir + "VLYamaAnimatorController.overrideController", -1.44f, -16.25f, .6f, false, 5),
            ("Karma", "Karma.png", "Karma_0", AnimDir + "KarmaAnimator.overrideController", -.69f, -16.3f, .39f, false, 5),
        };
        public static readonly string[] MainCast = { "Akim", "DarkLeeleeGiant", "VLMitama", "VLOrochi", "VLNarukami", "VLYama", "Karma" };

        [MenuItem("Tools/Issue641/Relayout Trial Ending 3")]
        public static void Relayout()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != ScenePath) scene = EditorSceneManager.OpenScene(ScenePath);
            PlaceCast(scene);
            SetupCamera(scene);
            RemoveStartInvoke(scene);
            RecordOverrides(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Issue641] TrialEnding_3 の配置とカメラを並べ直して保存しました。");
        }

        static void RemoveStartInvoke(Scene scene)
        {
            var flow = Find(scene, "TrialEnding3StartEvent").GetComponent<Flowchart>();
            var block = flow.FindBlock("Start");
            foreach (var command in block.CommandList.OfType<InvokeEvent>().ToArray())
            {
                block.CommandList.Remove(command);
                Object.DestroyImmediate(command);
            }
        }

        static void PlaceCast(Scene scene)
        {
            // 同名の root を名前検索1件ずつで消すと取りこぼすことがあったので、全件まとめて消してから作る。
            foreach (var root in scene.GetRootGameObjects().Where(g => Cast.Any(c => c.name == g.name)).ToArray())
                Object.DestroyImmediate(root);
            foreach (var entry in Cast)
                CreateNpc(scene, entry.name, entry.file, entry.sprite, entry.animator, entry.x, entry.y, entry.scale, entry.faceLeft, entry.order);
        }

        static void CreateNpc(Scene scene, string name, string file, string spriteName, string animatorPath, float x, float y, float scale, bool faceLeft, int order)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(NpcPath), scene);
            go.name = name;
            go.transform.localScale = new Vector3(faceLeft ? -scale : scale, scale, 1);
            var sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Game/Characters/Sprite/" + file).OfType<Sprite>().First(s => s.name == spriteName);
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            go.transform.position = new Vector3(x, y, 0);
            var animator = go.GetComponent<Animator>();
            if (animatorPath != null)
                animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(animatorPath);
            else animator.enabled = false;
            // 植物VLCNP は揺れるだけの背景。NPCController が isGround 等のパラメータを毎フレーム書いて警告を出すので止める。
            if (name.StartsWith("Plant_")) go.GetComponent<NPCController>().enabled = false;
            var body = go.GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0;
            foreach (var collider in go.GetComponents<BoxCollider2D>())
            {
                collider.offset = sprite.bounds.center;
                collider.size = sprite.bounds.size;
            }
        }

        public static void SetupCamera(Scene scene)
        {
            var existing = Find(scene, "CMCameraTrialEnding3");
            if (existing != null) Object.DestroyImmediate(existing);
            var go = new GameObject("CMCameraTrialEnding3");
            var camera = go.AddComponent<CinemachineVirtualCamera>();
            camera.Priority = 100;
            // TrialEnding と同じ寄り(ortho 5)と高さ(地面の下に土が約3ユニット映る)。x は 2026-09-06 にユーザーが手で寄せた値。
            camera.transform.position = new Vector3(-8.2f, -15.3f, -10);
            camera.m_Lens.OrthographicSize = 5f;
            Find(scene, "Main Camera").transform.position = camera.transform.position;
        }

        public static void RecordOverrides(Scene scene)
        {
            foreach (var component in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Component>(true)))
                if (component != null && PrefabUtility.IsPartOfPrefabInstance(component))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(component);
        }

        public static void HidePartyInScene(Scene scene)
        {
            // 初期化・セーブ復元で操作キャラが変わっても一瞬映り込まないよう全員を隠す。
            var party = Find(scene, "Party");
            foreach (var renderer in party.GetComponentsInChildren<SpriteRenderer>(true))
                renderer.enabled = false;
            foreach (var member in party.GetComponentsInChildren<Transform>(true).Where(t => t.CompareTag("Player")))
            {
                foreach (string name in new[] { "Hand", "Leg" })
                {
                    var child = member.Find(name);
                    if (child != null) child.gameObject.SetActive(false);
                }
            }
        }

        static void SetupStart(Scene scene)
        {
            var go = new GameObject("TrialEnding3StartEvent");
            var flow = go.AddComponent<Flowchart>();
            go.AddComponent<FlowchartStopAllGuard>();
            var gameEvent = go.AddComponent<GameEvent>();
            gameEvent.flowChart = flow;
            gameEvent.InformationText = "";
            var so = new SerializedObject(gameEvent);
            var entries = so.FindProperty("flagToBlockName");
            entries.arraySize = 1;
            var entry = entries.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("flag").intValue = (int)Flag.None;
            entry.FindPropertyRelative("blockName").stringValue = "Start";
            entry.FindPropertyRelative("isAutoStart").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
            var block = flow.CreateBlock(new Vector2(50, 50));
            block.BlockName = "Start";
            // Party はシーン側のスプライト無効で隠している。SetVisibility は呼ばない(InvokeEvent が戻らずブロックが止まった)。
            // Fungus の暗転はシーンをまたぐため、通常の遷移用 Fader とは別に解除する。
            var fade = Add<FadeScreen>(flow, block);
            so = new SerializedObject(fade);
            so.FindProperty("targetAlpha").floatValue = 0;
            so.FindProperty("duration").floatValue = 1;
            so.ApplyModifiedPropertiesWithoutUndo();
            var comment = Add<Comment>(flow, block);
            so = new SerializedObject(comment);
            so.FindProperty("commentText").stringValue = "体験版完了挨拶の開始。台詞・CTAはこの位置に追加する。";
            so.ApplyModifiedPropertiesWithoutUndo();
            var label = Add<Label>(flow, block);
            so = new SerializedObject(label);
            so.FindProperty("key").stringValue = "AwaitGreeting";
            so.ApplyModifiedPropertiesWithoutUndo();
            var wait = Add<Wait>(flow, block);
            so = new SerializedObject(wait);
            so.FindProperty("_duration").FindPropertyRelative("floatVal").floatValue = 1;
            so.ApplyModifiedPropertiesWithoutUndo();
            var jump = Add<Fungus.Jump>(flow, block);
            so = new SerializedObject(jump);
            so.FindProperty("_targetLabel").FindPropertyRelative("stringVal").stringValue = "AwaitGreeting";
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void ConnectPrevious(Scene scene)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null) return;
            var flow = Find(scene, "YamaRevengeEvent").GetComponent<Flowchart>();
            var block = flow.FindBlock("Revenge");
            var existing = Find(scene, TransitionName);
            if (existing != null) return;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Core/GameEventTransition.prefab"), scene);
            go.name = TransitionName;
            var transition = go.GetComponent<TransitionEvent>();
            var so = new SerializedObject(transition);
            int index = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToList().IndexOf(ScenePath);
            if (index < 0) throw new InvalidOperationException("TrialEnding_3 が Build Settings にありません。");
            so.FindProperty("sceneToLoad").intValue = index;
            so.FindProperty("isAutoSave").boolValue = false;
            so.FindProperty("fadeOutTime").floatValue = 0;
            so.FindProperty("fadeWaitTime").floatValue = .2f;
            so.ApplyModifiedPropertiesWithoutUndo();
            foreach (var comment in block.CommandList.OfType<Comment>().Where(c => c.GetSummary().Contains("TODO(#641): 次シーン")).ToArray())
            {
                block.CommandList.Remove(comment);
                Object.DestroyImmediate(comment);
            }
            var command = Add<InvokeEvent>(flow, block);
            UnityEventTools.AddVoidPersistentListener(Event(command), transition.ExecuteTransition);
            EditorSceneManager.MarkSceneDirty(scene);
        }

        /** #664: 既存の植物VLCNP 4本を作り直さずに、揺れアニメだけ付ける。 */
        [MenuItem("Tools/Issue641/Animate Trial Ending 3 Plants")]
        public static void AnimatePlants()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            foreach (var entry in Cast.Where(c => c.name.StartsWith("Plant_")))
            {
                var go = Find(scene, entry.name);
                var animator = go.GetComponent<Animator>();
                animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(entry.animator);
                animator.enabled = true;
                go.GetComponent<NPCController>().enabled = false;
                EditorUtility.SetDirty(go);
            }
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Issue641] 植物VLCNP にアニメーションを付けて保存しました。");
        }

        [MenuItem("Tools/Issue641/Verify Trial Ending 3 Runtime")]
        public static void VerifyRuntime()
        {
            try { VerifyRuntimeCore(); }
            catch (Exception e) { Debug.LogException(e); throw; }
        }

        static void VerifyRuntimeCore()
        {
            if (!Application.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != ScenePath)
                throw new InvalidOperationException("TrialEnding_3 の Play Mode で実行してください。");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var flow = Find(scene, "TrialEnding3StartEvent").GetComponent<Flowchart>();
            Require(flow.HasExecutingBlocks(), "開始イベントが実行されていません。");
            var active = flow.FindBlock("Start").ActiveCommand;
            Require(active is Wait || active is Say, "開始後の台詞または待機に到達していません。現在: " + (active == null ? "null" : active.GetType().Name));
            var party = Find(scene, "Party");
            Require(!party.GetComponentsInChildren<SpriteRenderer>().Any(r => r.enabled), "Player が表示されています。");
            var fader = Object.FindObjectOfType<Fader>();
            Require(fader != null && fader.GetComponent<CanvasGroup>().alpha < .01f, "遷移の暗転が残っています。");
            float alpha = (float)typeof(CameraManager).GetField("fadeAlpha", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(FungusManager.Instance.CameraManager);
            Require(alpha < .01f, "Fungus の暗転が残っています。");
            var npcs = Object.FindObjectsOfType<NPCController>().ToDictionary(n => n.name);
            foreach (var entry in Cast)
                Require(npcs.ContainsKey(entry.name), entry.name + " がいません。");
            var placed = new System.Collections.Generic.List<Bounds>();
            foreach (var npc in MainCast.Select(n => npcs[n]))
            {
                var r = npc.GetComponent<SpriteRenderer>();
                var b = r.bounds;
                Require(r.enabled && npc.gameObject.activeInHierarchy, npc.name + " が非表示です。");
                // 手置きの位置はスプライト矩形の透明余白ぶん地面(y=-17)より下に出る。見た目の接地はスクショで確認済み。
                Require(b.min.y >= -17.1f, npc.name + " が地面にめり込んでいます。");
                var min = Camera.main.WorldToViewportPoint(b.min);
                var max = Camera.main.WorldToViewportPoint(b.max);
                Require(min.x >= 0 && max.x <= 1 && min.y >= 0 && max.y <= 1, npc.name + " が画面外です。");
                // 透明余白を除いた中身どうしの重なりだけ見る(矩形を各辺 25% 縮める)。
                var core = new Bounds(b.center, b.size * .5f);
                Require(!placed.Any(p => p.Intersects(core)), npc.name + " が他の主要キャラに重なっています。");
                placed.Add(core);
            }
            foreach (var plant in Cast.Where(c => c.order == 1).Select(c => npcs[c.name]))
                Require(plant.GetComponent<SpriteRenderer>().sortingOrder < 5, plant.name + " が前列に出ています。");
            Debug.Log("[Issue641] PASS: 開始イベント待機、Player非表示、両Fader解除、主要7人+植物4本、画面内、地面との位置、非重複、植物は後列。");
        }

        [MenuItem("Tools/Issue641/Preview Ending Transition From Final Fade")]
        public static void PreviewTransition()
        {
            if (!Application.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != Issue641YamaRevengeSetup.ScenePath)
                throw new InvalidOperationException("ヤーマ襲来の Play Mode で実行してください。");
            var flow = GameObject.Find("YamaRevengeEvent").GetComponent<Flowchart>();
            flow.StopAllBlocks();
            if (SayDialog.ActiveSayDialog != null) SayDialog.ActiveSayDialog.gameObject.SetActive(false);
            var block = flow.FindBlock("Revenge");
            flow.ExecuteBlock(block, block.CommandList.FindLastIndex(c => c is FadeScreen));
        }

        [MenuItem("Tools/Issue641/Debug/Dump Start Block")]
        public static void DumpStartBlock()
        {
            var go = GameObject.Find("TrialEnding3StartEvent");
            var flow = go.GetComponent<Flowchart>();
            var block = flow.FindBlock("Start");
            var sb = new System.Text.StringBuilder("[Issue641] dump ");
            sb.Append("time=").Append(Time.time.ToString("F2")).Append(" scale=").Append(Time.timeScale)
              .Append(" active=").Append(go.activeInHierarchy).Append(" flowEnabled=").Append(flow.enabled)
              .Append(" state=").Append(block.State).Append(" execCount=").Append(block.GetExecutionCount())
              .Append(" activeCmd=").Append(block.ActiveCommand == null ? "null" : block.ActiveCommand.GetType().Name + "#" + block.ActiveCommand.CommandIndex)
              .Append(" fader=").Append(Object.FindObjectOfType<Fader>()?.GetComponent<CanvasGroup>().alpha)
              .Append(" party=").Append(GameObject.Find("Party")?.GetComponent<PartyCongroller>()?.enabled);
            foreach (var c in block.CommandList)
                sb.Append("\n  ").Append(c.GetType().Name).Append(" idx=").Append(c.CommandIndex)
                  .Append(" parent=").Append(c.ParentBlock == null ? "null" : c.ParentBlock.BlockName + (c.ParentBlock == block ? "(same)" : "(OTHER)"))
                  .Append(" executing=").Append(c.IsExecuting).Append(" enabled=").Append(c.enabled);
            Debug.Log(sb.ToString());
        }

        // ---- #641 体験版完了挨拶の台詞・BGM・発売案内 ----

        const string GuideName = "ReleaseGuide";
        const string EventName = "TrialEnding3StartEvent";
        const string FontPath = "Assets/Game/Fonts/NotoSansJP-Regular.ttf";
        const string KeyartPath = "Assets/Game/UI/Sprite/wishlist_keyart.jpg";
        const string XSpritePath = "Assets/Game/UI/Sprite/yhei.png";
        const string OpeningBgmPath = "Assets/Game/BGM/opening.mp3";
        const string AllFacesPath = "Assets/Game/Characters/Face/AllFaces.prefab";

        [MenuItem("Tools/Issue641/Build Trial Ending 3 Greeting")]
        public static void BuildGreetingMenu()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != ScenePath) scene = EditorSceneManager.OpenScene(ScenePath);
            BuildGreeting(scene);
            RecordOverrides(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Issue641] TrialEnding_3 の台詞と発売案内を組み立てて保存しました。");
        }

        /** 台詞・BGM・発売案内の正。再実行可(生成物は同名を消して作り直す)。 */
        public static void BuildGreeting(Scene scene)
        {
            EnsureKeyartImport();
            // 会話中の BGM はシーンの AreaBGM(Makami、音量 0.5)。遷移時に TransitionEvent が切り替える。
            var eventGo = Find(scene, EventName);
            if (eventGo == null) throw new InvalidOperationException(EventName + " がありません。Setup を先に実行してください。");
            var faces = EnsureFaces(scene, eventGo);
            var guide = BuildReleaseGuide(scene);
            BuildStartBlock(eventGo.GetComponent<Flowchart>(), faces, guide);
        }

        static void EnsureKeyartImport()
        {
            var importer = AssetImporter.GetAtPath(KeyartPath) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("キービジュアルがありません: " + KeyartPath);
            if (importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Single
                && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        static System.Collections.Generic.Dictionary<string, Character> EnsureFaces(Scene scene, GameObject eventGo)
        {
            var existing = Find(scene, "AllFaces");
            if (existing != null) Object.DestroyImmediate(existing);
            var allFaces = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(AllFacesPath), scene);
            allFaces.name = "AllFaces";
            allFaces.transform.SetParent(eventGo.transform, false);
            var sayDialog = scene.GetRootGameObjects().Select(r => r.GetComponentInChildren<SayDialog>(true)).FirstOrDefault(d => d != null);
            if (sayDialog == null) throw new InvalidOperationException("シーンに SayDialog がありません。");
            var faces = new System.Collections.Generic.Dictionary<string, Character>();
            foreach (var character in allFaces.GetComponentsInChildren<Character>(true))
            {
                var so = new SerializedObject(character);
                so.FindProperty("setSayDialog").objectReferenceValue = sayDialog;
                so.ApplyModifiedPropertiesWithoutUndo();
                faces[character.name] = character;
            }
            foreach (string name in new[] { "LeeleeDarkCharacter", "MitamaCharacter", "OrochiCharacter", "NarukamiCharacter" })
                if (!faces.ContainsKey(name)) throw new InvalidOperationException("AllFaces に " + name + " がありません。");
            return faces;
        }

        /** 発売案内(黒背景の三択)。左からXをフォロー / ウィッシュリスト登録 / タイトルへ戻る。 */
        static GameObject BuildReleaseGuide(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects().Where(g => g.name == GuideName).ToArray())
                Object.DestroyImmediate(root);
            var go = new GameObject(GuideName, typeof(RectTransform));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            var cta = go.AddComponent<TrialEndCtaActions>();
            var menu = go.AddComponent<ReleaseGuideMenu>();

            var background = CreateRect("Background", go.transform);
            Stretch(background);
            var backImage = background.gameObject.AddComponent<Image>();
            backImage.color = Color.black;
            backImage.raycastTarget = false;
            var backGroup = background.gameObject.AddComponent<CanvasGroup>();
            backGroup.alpha = 0;
            backGroup.blocksRaycasts = false;
            backGroup.interactable = false;

            var content = CreateRect("Content", go.transform);
            Stretch(content);
            var contentGroup = content.gameObject.AddComponent<CanvasGroup>();
            contentGroup.alpha = 0;
            contentGroup.blocksRaycasts = false;
            contentGroup.interactable = false;

            var font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            var xSprite = AssetDatabase.LoadAssetAtPath<Sprite>(XSpritePath);
            var keyart = AssetDatabase.LoadAssetAtPath<Sprite>(KeyartPath);
            if (font == null || xSprite == null || keyart == null)
                throw new InvalidOperationException("発売案内の素材が読めません。font=" + font + " x=" + xSprite + " keyart=" + keyart);

            var followX = CreateImageCard(content, "FollowX", new Vector2(-620, 40), new Vector2(480, 275), xSprite, "Xをフォロー", font);
            var wishlist = CreateImageCard(content, "Wishlist", new Vector2(0, 40), new Vector2(600, 344), keyart, "ウィッシュリスト登録", font);
            var backToTitle = CreateTextCard(content, "BackToTitle", new Vector2(620, 40), new Vector2(480, 275), "タイトルへ戻る", font);
            UnityEventTools.AddVoidPersistentListener(followX.OnSubmit, cta.OpenX);
            UnityEventTools.AddVoidPersistentListener(wishlist.OnSubmit, cta.OpenWishlist);
            // タイトルへ戻るは BGM を止めてから遷移するので menu 経由。
            UnityEventTools.AddVoidPersistentListener(backToTitle.OnSubmit, menu.ReturnToTitle);

            var hint = CreateText("Hint", content, "左右で選択 / 決定で開く", font, 32, new Color(.8f, .8f, .8f));
            hint.rectTransform.anchoredPosition = new Vector2(0, -440);
            hint.rectTransform.sizeDelta = new Vector2(1200, 60);

            var bgmWrapper = Find(scene, "BGMWrapper")?.GetComponent<BGMWrapper>();
            if (bgmWrapper == null) throw new InvalidOperationException("BGMWrapper がありません。");
            var so = new SerializedObject(menu);
            so.FindProperty("background").objectReferenceValue = backGroup;
            so.FindProperty("content").objectReferenceValue = contentGroup;
            var items = so.FindProperty("items");
            items.arraySize = 3;
            items.GetArrayElementAtIndex(0).objectReferenceValue = followX;
            items.GetArrayElementAtIndex(1).objectReferenceValue = wishlist;
            items.GetArrayElementAtIndex(2).objectReferenceValue = backToTitle;
            so.FindProperty("initialIndex").intValue = 1;
            so.FindProperty("bgmWrapper").objectReferenceValue = bgmWrapper;
            so.FindProperty("guideBgm").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(OpeningBgmPath);
            so.FindProperty("guideBgmVolume").floatValue = .4f;
            so.FindProperty("guideBgmPitch").floatValue = 1f;
            so.FindProperty("ctaActions").objectReferenceValue = cta;
            so.FindProperty("exitFadeDuration").floatValue = 1f;
            so.ApplyModifiedPropertiesWithoutUndo();
            return go;
        }

        static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static void Stretch(RectTransform rect, float inset = 0)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        static ReleaseGuideItem CreateCardRoot(RectTransform parent, string name, Vector2 position, Vector2 size)
        {
            var rect = CreateRect(name, parent);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var group = rect.gameObject.AddComponent<CanvasGroup>();
            var item = rect.gameObject.AddComponent<ReleaseGuideItem>();
            var so = new SerializedObject(item);
            so.FindProperty("group").objectReferenceValue = group;
            so.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        static ReleaseGuideItem CreateImageCard(RectTransform parent, string name, Vector2 position, Vector2 size, Sprite sprite, string label, Font font)
        {
            var item = CreateCardRoot(parent, name, position, size);
            var image = item.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            var text = CreateText("Label", item.transform, label, font, 40, Color.white);
            text.rectTransform.anchorMin = new Vector2(.5f, 0);
            text.rectTransform.anchorMax = new Vector2(.5f, 0);
            text.rectTransform.pivot = new Vector2(.5f, 1);
            text.rectTransform.anchoredPosition = new Vector2(0, -16);
            text.rectTransform.sizeDelta = new Vector2(size.x + 200, 56);
            return item;
        }

        static ReleaseGuideItem CreateTextCard(RectTransform parent, string name, Vector2 position, Vector2 size, string label, Font font)
        {
            var item = CreateCardRoot(parent, name, position, size);
            var frame = item.gameObject.AddComponent<Image>();
            frame.color = Color.white;
            frame.raycastTarget = false;
            var inner = CreateRect("Inner", item.transform);
            Stretch(inner, 6);
            var innerImage = inner.gameObject.AddComponent<Image>();
            innerImage.color = Color.black;
            innerImage.raycastTarget = false;
            var text = CreateText("Label", item.transform, label, font, 48, Color.white);
            Stretch(text.rectTransform);
            return item;
        }

        static Text CreateText(string name, Transform parent, string value, Font font, int size, Color color)
        {
            var rect = CreateRect(name, parent);
            var text = rect.gameObject.AddComponent<Text>();
            text.text = value;
            text.font = font;
            text.fontSize = size;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        /** issue コメント 5557271332 の台本(2026-09-06 にユーザーがエディタで調整した版)。Start ブロックを作り直す。 */
        static void BuildStartBlock(Flowchart flow, System.Collections.Generic.Dictionary<string, Character> faces, GameObject guide)
        {
            var block = flow.FindBlock("Start");
            foreach (var command in block.CommandList.ToArray())
            {
                block.CommandList.Remove(command);
                if (command != null) Object.DestroyImmediate(command);
            }
            foreach (var stray in flow.GetComponents<Command>().Where(c => c.ParentBlock == null || c.ParentBlock == block).ToArray())
                Object.DestroyImmediate(stray);

            Character leelee = faces["LeeleeDarkCharacter"], mitama = faces["MitamaCharacter"],
                orochi = faces["OrochiCharacter"], narukami = faces["NarukamiCharacter"];
            // 遷移用 Fader とは別に、Fungus の暗転を解除して始める(シーンをまたいで残る)。
            AddFadeScreen(flow, block, 1f, 0f, true);
            AddSay(flow, block, leelee, "体験版、しゅーーーーりょぉーーーーー！");
            AddWait(flow, block, 2f);
            AddSay(flow, block, leelee, "プレイありがとうな？ 面白かった？ このあとどうなってしまうんやろな〜。");
            AddSay(flow, block, mitama, "てか 闇堕ちしながら普通に喋ってますが。\n大丈夫なんですか？");
            AddSay(flow, block, orochi, "{size=32}めっちゃぶん殴られたけど...{/size}");
            AddSay(flow, block, narukami, "大穴が空いて奈落に落ちていったようだな。\n無事では済むまい。");
            AddSay(flow, block, leelee, "せやねん、無事じゃないねんw");
            AddSay(flow, block, leelee, "果たして5人のVLCNPは生きているのか？ いなくなったシモーヌはどうなったのか？ Millcoさんやウニはどうなってしまうのか？");
            AddSay(flow, block, mitama, "気になるところで終わりましたね...");
            AddSay(flow, block, leelee, "これは見逃せへんでえ〜w");
            AddWait(flow, block, 2f);
            AddSay(flow, block, leelee, "では最後にアナウンス！\nVLCNP物語は、近日発売予定や！");
            AddSay(flow, block, leelee, "Steamのサイトからウィッシュリスト登録しておくと、\n発売日当日に通知が来るからな。\n是非登録してみてや。");
            AddSay(flow, block, mitama, "制作者のXもフォローしておくと\n色々捗るかもしれないですね〜。");
            AddSay(flow, block, narukami, "おうえんのコメントもいただけると\n泣いて喜ぶらしいぞ。");
            AddWait(flow, block, 2f);
            AddSay(flow, block, leelee, "では発売まで首をベリーロングにして待っといてや！ また会えることを楽しみにしとるで〜！");
            // 暗転・BGM フェード・発売案内の表示は ReleaseGuideMenu が担う。
            AddInvokeMethod(flow, block, guide, typeof(ReleaseGuideMenu), "Begin");
            // ブロックを終わらせない(StopAll を維持して見えないプレイヤーが動かないようにする)。
            var label = Add<Label>(flow, block);
            var so = new SerializedObject(label);
            so.FindProperty("key").stringValue = "AwaitGuide";
            so.ApplyModifiedPropertiesWithoutUndo();
            AddWait(flow, block, 1f);
            var jump = Add<Fungus.Jump>(flow, block);
            so = new SerializedObject(jump);
            so.FindProperty("_targetLabel").FindPropertyRelative("stringVal").stringValue = "AwaitGuide";
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AddFadeScreen(Flowchart flow, Block block, float duration, float targetAlpha, bool waitUntilFinished)
        {
            var fade = Add<FadeScreen>(flow, block);
            var so = new SerializedObject(fade);
            so.FindProperty("duration").floatValue = duration;
            so.FindProperty("targetAlpha").floatValue = targetAlpha;
            so.FindProperty("fadeColor").colorValue = Color.black;
            so.FindProperty("waitUntilFinished").boolValue = waitUntilFinished;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AddSay(Flowchart flow, Block block, Character character, string storyText)
        {
            var say = Add<Say>(flow, block);
            var so = new SerializedObject(say);
            so.FindProperty("storyText").stringValue = storyText;
            so.FindProperty("character").objectReferenceValue = character;
            so.FindProperty("portrait").objectReferenceValue = character.Portraits != null && character.Portraits.Count > 0 ? character.Portraits[0] : null;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AddWait(Flowchart flow, Block block, float duration)
        {
            var wait = Add<Wait>(flow, block);
            var so = new SerializedObject(wait);
            so.FindProperty("_duration").FindPropertyRelative("floatVal").floatValue = duration;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AddInvokeMethod(Flowchart flow, Block block, GameObject target, Type componentType, string methodName)
        {
            if (target == null || target.GetComponent(componentType) == null)
                throw new InvalidOperationException("InvokeMethod の対象がありません: " + componentType.Name + "." + methodName);
            var invoke = Add<InvokeMethod>(flow, block);
            var so = new SerializedObject(invoke);
            so.FindProperty("targetObject").objectReferenceValue = target;
            so.FindProperty("targetComponentAssemblyName").stringValue = componentType.AssemblyQualifiedName;
            so.FindProperty("targetComponentFullname").stringValue = "UnityEngine.Component[]";
            so.FindProperty("targetComponentText").stringValue = componentType.Name;
            so.FindProperty("targetMethod").stringValue = methodName;
            so.FindProperty("targetMethodText").stringValue = methodName + " (): Void";
            so.FindProperty("returnValueType").stringValue = "System.Void";
            so.FindProperty("methodParameters").arraySize = 0;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [MenuItem("Tools/Issue641/Verify Release Guide Runtime")]
        public static void VerifyGuideRuntime()
        {
            try { VerifyGuideRuntimeCore(); }
            catch (Exception e) { Debug.LogException(e); throw; }
        }

        static void VerifyGuideRuntimeCore()
        {
            if (!Application.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != ScenePath)
                throw new InvalidOperationException("TrialEnding_3 の Play Mode で実行してください。");
            var guide = GameObject.Find(GuideName);
            Require(guide != null, "ReleaseGuide がありません。");
            var menu = guide.GetComponent<ReleaseGuideMenu>();
            Require(menu.IsReady, "発売案内がまだ操作可能になっていません。");
            var groups = guide.GetComponentsInChildren<CanvasGroup>().ToDictionary(g => g.name);
            Require(groups["Background"].alpha > .99f, "黒背景が不透明になっていません。");
            Require(groups["Content"].alpha > .99f, "カードが表示し切っていません。");
            Require(menu.SelectedIndex == 1, "初期選択がウィッシュリストではありません: " + menu.SelectedIndex);
            var bgm = GameObject.FindWithTag("BGM").GetComponent<AudioSource>();
            Require(bgm.clip != null && bgm.clip.name == "opening" && bgm.isPlaying, "発売案内の BGM が opening ではありません: " + (bgm.clip == null ? "null" : bgm.clip.name));
            Require(Mathf.Abs(bgm.volume - .4f) < .01f, "発売案内の BGM 音量が 0.4 ではありません: " + bgm.volume);
            Require(SayDialog.ActiveSayDialog == null || !SayDialog.ActiveSayDialog.gameObject.activeInHierarchy, "会話枠が残っています。");
            var flow = GameObject.Find(EventName).GetComponent<Flowchart>();
            Require(flow.FindBlock("Start").ActiveCommand is Wait, "Start ブロックが待機ループにいません。");
            Debug.Log("[Issue641] PASS: 発売案内が表示され操作可能、黒背景、初期選択ウィッシュリスト、BGM opening 0.4、会話枠なし、ブロック待機中。");
        }

        [MenuItem("Tools/Issue641/Debug/Skip To Release Guide")]
        public static void SkipToGuide()
        {
            if (!Application.isPlaying) throw new InvalidOperationException("Play Mode で実行してください。");
            var flow = GameObject.Find(EventName).GetComponent<Flowchart>();
            flow.StopAllBlocks();
            if (SayDialog.ActiveSayDialog != null) SayDialog.ActiveSayDialog.gameObject.SetActive(false);
            var block = flow.FindBlock("Start");
            flow.ExecuteBlock(block, block.CommandList.FindIndex(c => c is InvokeMethod));
        }

        [MenuItem("Tools/Issue641/Debug/Release Guide Left")]
        public static void GuideLeft() => GameObject.Find(GuideName).GetComponent<ReleaseGuideMenu>().MoveSelection(-1);

        [MenuItem("Tools/Issue641/Debug/Release Guide Right")]
        public static void GuideRight() => GameObject.Find(GuideName).GetComponent<ReleaseGuideMenu>().MoveSelection(1);

        [MenuItem("Tools/Issue641/Debug/Release Guide Submit")]
        public static void GuideSubmit() => GameObject.Find(GuideName).GetComponent<ReleaseGuideMenu>().SubmitCurrent();

        static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        static T Add<T>(Flowchart flow, Block block) where T : Command
        {
            var command = flow.gameObject.AddComponent<T>();
            command.ItemId = flow.NextItemId();
            command.ParentBlock = block;
            block.CommandList.Add(command);
            return command;
        }

        static UnityEvent Event(InvokeEvent command) => (UnityEvent)typeof(InvokeEvent)
            .GetField("staticEvent", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(command);

        static GameObject Find(Scene scene, string name) => scene.GetRootGameObjects()
            .SelectMany(g => g.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t => t.name == name)?.gameObject;
    }
}
