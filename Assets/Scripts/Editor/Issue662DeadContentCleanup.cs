using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Fungus;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VLCNP.Core;
using VLCNP.SceneManagement;
using Object = UnityEngine.Object;
using Scene = UnityEngine.SceneManagement.Scene;

namespace VLCNP.Editor
{
    /** #662: 死にコンテンツ(旧ワープ先・TrialEnd2 の Debug プレハブ・旧終了シーン)の到達経路を断つ。 */
    public static class Issue662DeadContentCleanup
    {
        const string FarmScene = "Assets/Scenes/VeryLongFarm_1.unity";
        const string ReportDir = "Temp"; // プロジェクト直下の Temp/ に出力

        [MenuItem("Tools/Issue662/Report VeryLongFarm_1")]
        public static void ReportFarm()
        {
            var scene = OpenIfNeeded(FarmScene);
            var sb = new StringBuilder();
            sb.AppendLine("== Flowcharts ==");
            foreach (var fc in Object.FindObjectsOfType<Flowchart>(true))
            {
                sb.AppendLine($"[Flowchart] {Path(fc.transform)}");
                foreach (var block in fc.GetComponents<Block>())
                {
                    sb.AppendLine($"  [Block] '{block.BlockName}' handler={(block._EventHandler ? block._EventHandler.GetType().Name : "none")} desc={block.Description}");
                    foreach (var cmd in block.CommandList)
                    {
                        sb.AppendLine($"    - {Describe(cmd)}");
                    }
                }
            }
            sb.AppendLine("== GameEvent / Chat ==");
            foreach (var ge in Object.FindObjectsOfType<GameEvent>(true))
            {
                var so = new SerializedObject(ge);
                var arr = so.FindProperty("flagToBlockName");
                sb.AppendLine($"[GameEvent] {Path(ge.transform)} flowchart={(ge.flowChart ? Path(ge.flowChart.transform) : "null")}");
                for (int i = 0; i < arr.arraySize; i++)
                {
                    var e = arr.GetArrayElementAtIndex(i);
                    sb.AppendLine($"    flag={(Flag)e.FindPropertyRelative("flag").enumValueIndex} block='{e.FindPropertyRelative("blockName").stringValue}' auto={e.FindPropertyRelative("isAutoStart").boolValue} collision={e.FindPropertyRelative("isCollisionStart").boolValue}");
                }
            }
            foreach (var ch in Object.FindObjectsOfType<VLCNP.Actions.Chat>(true))
            {
                sb.AppendLine($"[Chat] {Path(ch.transform)} flowchart={(ch.flowChart ? Path(ch.flowChart.transform) : "null")} block='{ch.BlockName}'");
                foreach (var kv in ch.flagToBlockName ?? Array.Empty<VLCNP.Actions.Chat.SerializableKeyPair<Flag, string>>())
                    sb.AppendLine($"    flag={kv.Key} block='{kv.Value}' after={kv.afterChatSetFlag}");
            }
            sb.AppendLine("== TransitionEvent ==");
            foreach (var te in Object.FindObjectsOfType<TransitionEvent>(true))
            {
                var so = new SerializedObject(te);
                sb.AppendLine($"[TransitionEvent] {Path(te.transform)} sceneToLoad={so.FindProperty("sceneToLoad").intValue} spawn={so.FindProperty("destinationSpawnPointName").stringValue}");
            }
            File.WriteAllText(System.IO.Path.Combine(ReportDir, "farm_report.txt"), sb.ToString());
            Debug.Log("[Issue662] report written");
        }

        [MenuItem("Tools/Issue662/Report Scene Roots")]
        public static void ReportRoots()
        {
            var sb = new StringBuilder();
            foreach (var path in new[] { "Assets/Scenes/TrialEnding.unity", "Assets/Scenes/TrialEnd2.unity" })
            {
                var scene = OpenIfNeeded(path);
                sb.AppendLine($"== {path} ==");
                foreach (var root in scene.GetRootGameObjects())
                    Dump(root.transform, 0, sb, 2);
            }
            File.WriteAllText(System.IO.Path.Combine(ReportDir, "roots_report.txt"), sb.ToString());
            Debug.Log("[Issue662] roots report written");
        }

        [MenuItem("Tools/Issue662/Apply VeryLongFarm_1")]
        public static void ApplyFarm()
        {
            var scene = OpenIfNeeded(FarmScene);
            var deguchi = scene.GetRootGameObjects().First(g => g.name == "DeguchiGameEvent");
            var flowchart = deguchi.GetComponentInChildren<Flowchart>(true);
            flowchart.ClearSelectedBlocks();
            flowchart.ClearSelectedCommands();

            // Message2 の「ベリーロングパレス」(TrialEnd ブロック行き) を外す
            var message2 = flowchart.FindBlock("Message2");
            var palace = message2.CommandList.OfType<Fungus.Menu>().Where(m => TargetBlockOf(m) != null && TargetBlockOf(m).BlockName == "TrialEnd").ToList();
            foreach (var m in palace)
            {
                message2.CommandList.Remove(m);
                Object.DestroyImmediate(m);
                Debug.Log("[Issue662] Menu 'ベリーロングパレス' を Message2 から削除");
            }

            foreach (var name in new[] { "AfterOrochi", "TrialEnd", "TrialEnd2" })
            {
                var block = flowchart.FindBlock(name);
                if (block == null) { Debug.LogWarning($"[Issue662] block {name} なし"); continue; }
                foreach (var cmd in block.CommandList.ToList()) if (cmd != null) Object.DestroyImmediate(cmd);
                Object.DestroyImmediate(block);
                Debug.Log($"[Issue662] block {name} を削除");
            }

            // 配列サイズ 2 に対する data[2] の宙ぶらりん上書き(flag=VLOrochiJoined→AfterOrochi)を掃除
            var mods = PrefabUtility.GetPropertyModifications(deguchi);
            var kept = mods.Where(m => !m.propertyPath.StartsWith("flagToBlockName.Array.data[2]")).ToArray();
            Debug.Log($"[Issue662] DeguchiGameEvent の上書き {mods.Length} → {kept.Length}");
            PrefabUtility.SetPropertyModifications(deguchi, kept);

            foreach (var name in new[] { "GameEventTransition", "GameEventTransitionTrialEnd2" })
            {
                var go = scene.GetRootGameObjects().FirstOrDefault(g => g.name == name);
                if (go == null) { Debug.LogWarning($"[Issue662] {name} なし"); continue; }
                var so = new SerializedObject(go.GetComponent<TransitionEvent>());
                Debug.Log($"[Issue662] {name} (sceneToLoad={so.FindProperty("sceneToLoad").intValue}) を削除");
                Object.DestroyImmediate(go);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Issue662] VeryLongFarm_1 を保存");
        }

        [MenuItem("Tools/Issue662/Strip TrialEnding and TrialEnd2")]
        public static void StripEndingScenes()
        {
            foreach (var path in new[] { "Assets/Scenes/TrialEnding.unity", "Assets/Scenes/TrialEnd2.unity" })
            {
                var scene = OpenIfNeeded(path);
                foreach (var root in scene.GetRootGameObjects()) Object.DestroyImmediate(root);
                var cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                cam.tag = "MainCamera";
                var camera = cam.GetComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 5.4f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                cam.transform.position = new Vector3(0, 0, -10);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Issue662] {path} をカメラだけにして保存");
            }
        }

        // ---- プレイモード検証用(Menu.Execute から叩く) ----
        [MenuItem("Tools/Issue662/Debug/Set All Join Flags")]
        public static void DebugSetFlags()
        {
            var fm = FlagManager.FindInScene();
            fm.SetFlag(Flag.ReturnedToVLFarm, true); // FarmReturnEvent の自動発火を避ける
            foreach (var f in new[] { Flag.IkehayaBlockChainChated, Flag.VLOrochiJoined, Flag.VLMitamaJoined, Flag.VLNarukamiJoined })
                fm.SetFlag(f, true);
            Debug.Log("[Issue662] 全加入フラグを立てました");
        }

        [MenuItem("Tools/Issue662/Debug/Start Deguchi")]
        public static void DebugStartDeguchi()
        {
            var ge = GameObject.Find("DeguchiGameEvent").GetComponent<GameEvent>();
            var entry = ge.GetCurrentBlockNameFromFlag();
            Debug.Log($"[Issue662] Deguchi 実行 block={(entry != null ? entry.BlockName : "(default)")}");
            ge.Execute();
        }

        [MenuItem("Tools/Issue662/Debug/Advance Dialog")]
        public static void DebugAdvance()
        {
            var input = Object.FindObjectOfType<DialogInput>();
            if (input == null) { Debug.Log("[Issue662] DialogInput なし"); return; }
            input.SetNextLineFlag();
            Debug.Log("[Issue662] SetNextLineFlag");
        }

        [MenuItem("Tools/Issue662/Debug/Log Menu Options")]
        public static void DebugLogOptions()
        {
            var buttons = ActiveButtons();
            Debug.Log($"[Issue662] 選択肢 {buttons.Count} 件: {string.Join(" / ", buttons.Select(b => LabelOf(b)))}");
        }

        [MenuItem("Tools/Issue662/Debug/Select Option 1")] public static void DebugSelect1() => DebugSelect(0);
        [MenuItem("Tools/Issue662/Debug/Select Option 2")] public static void DebugSelect2() => DebugSelect(1);
        [MenuItem("Tools/Issue662/Debug/Select Option 3")] public static void DebugSelect3() => DebugSelect(2);
        [MenuItem("Tools/Issue662/Debug/Select Option 4")] public static void DebugSelect4() => DebugSelect(3);

        static void DebugSelect(int index)
        {
            var buttons = ActiveButtons();
            if (index >= buttons.Count) { Debug.LogError($"[Issue662] 選択肢 {index + 1} なし(表示 {buttons.Count})"); return; }
            Debug.Log($"[Issue662] 選択: {LabelOf(buttons[index])}");
            buttons[index].onClick.Invoke();
        }

        [MenuItem("Tools/Issue662/Debug/Log Scene")]
        public static void DebugLogScene()
        {
            var s = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            Debug.Log($"[Issue662] active scene buildIndex={s.buildIndex} name={s.name}");
        }

        [MenuItem("Tools/Issue662/Debug/Continue Game")]
        public static void DebugContinue()
        {
            var cg = Object.FindObjectOfType<ContinueGame>(true);
            if (cg == null) { Debug.LogError("[Issue662] ContinueGame なし"); return; }
            cg.gameObject.SetActive(true);
            cg.Execute();
            Debug.Log("[Issue662] ContinueGame.Execute");
        }

        static List<UnityEngine.UI.Button> ActiveButtons()
        {
            var md = MenuDialog.ActiveMenuDialog;
            if (md == null || !md.gameObject.activeSelf) return new List<UnityEngine.UI.Button>();
            return md.CachedButtons.Where(b => b != null && b.gameObject.activeSelf).ToList();
        }

        static string LabelOf(UnityEngine.UI.Button b)
        {
            var t = b.GetComponentInChildren<UnityEngine.UI.Text>();
            if (t != null) return t.text;
            var tmp = b.GetComponentInChildren<TMPro.TMP_Text>();
            return tmp != null ? tmp.text : "?";
        }

        static Block TargetBlockOf(Command c) => new SerializedObject(c).FindProperty("targetBlock").objectReferenceValue as Block;

        static void Dump(Transform t, int depth, StringBuilder sb, int maxDepth)
        {
            var comps = t.GetComponents<Component>().Where(c => c != null && !(c is Transform)).Select(c => c.GetType().Name);
            sb.AppendLine($"{new string(' ', depth * 2)}{t.name} [{string.Join(",", comps)}] active={t.gameObject.activeSelf} prefab={PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.gameObject)}");
            if (depth >= maxDepth) return;
            foreach (Transform c in t) Dump(c, depth + 1, sb, maxDepth);
        }

        static string Describe(Command cmd)
        {
            if (cmd == null) return "(null command)";
            var so = new SerializedObject(cmd);
            var parts = new List<string>();
            var it = so.GetIterator();
            bool enter = true;
            while (it.NextVisible(enter))
            {
                enter = false;
                if (it.name == "m_Script" || it.name == "itemId" || it.name == "indentLevel") continue;
                parts.Add($"{it.name}={Value(it)}");
            }
            return $"{cmd.GetType().Name}: {string.Join(" ", parts)}";
        }

        static string Value(SerializedProperty p)
        {
            switch (p.propertyType)
            {
                case SerializedPropertyType.String: return $"'{p.stringValue}'";
                case SerializedPropertyType.Integer: return p.intValue.ToString();
                case SerializedPropertyType.Boolean: return p.boolValue.ToString();
                case SerializedPropertyType.Float: return p.floatValue.ToString();
                case SerializedPropertyType.Enum: return p.enumNames.Length > p.enumValueIndex && p.enumValueIndex >= 0 ? p.enumNames[p.enumValueIndex] : p.enumValueIndex.ToString();
                case SerializedPropertyType.ObjectReference:
                    var o = p.objectReferenceValue;
                    if (o == null) return "null";
                    if (o is Block b) return $"Block'{b.BlockName}'@{Path(b.transform)}";
                    if (o is Component c) return $"{c.GetType().Name}@{Path(c.transform)}";
                    return o.name;
                case SerializedPropertyType.Generic:
                    if (p.isArray) return $"[{p.arraySize}]";
                    var sub = p.Copy(); var end = sub.GetEndProperty(); var inner = new List<string>();
                    if (sub.NextVisible(true))
                        do { if (SerializedProperty.EqualContents(sub, end)) break; inner.Add($"{sub.name}={Value(sub)}"); } while (sub.NextVisible(false));
                    return "{" + string.Join(" ", inner) + "}";
                default: return p.propertyType.ToString();
            }
        }

        static string Path(Transform t)
        {
            var s = t.name;
            while (t.parent != null) { t = t.parent; s = t.name + "/" + s; }
            return s;
        }

        static Scene OpenIfNeeded(string path)
        {
            var active = EditorSceneManager.GetActiveScene();
            if (active.path == path) return active;
            if (active.isDirty) throw new InvalidOperationException("未保存シーンがあります。");
            return EditorSceneManager.OpenScene(path);
        }
    }
}
