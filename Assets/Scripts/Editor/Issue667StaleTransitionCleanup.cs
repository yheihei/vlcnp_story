using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Fungus;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VLCNP.Attributes;
using VLCNP.Control;
using VLCNP.Core;
using VLCNP.SceneManagement;
using VLCNP.UI;
using Object = UnityEngine.Object;
using Scene = UnityEngine.SceneManagement.Scene;

namespace VLCNP.Editor
{
    /** #667: tuti_5 複製時に残った boss_1 / boss_3 の GameEventTransition(見えないドア)を調査・削除する。 */
    public static class Issue667StaleTransitionCleanup
    {
        static readonly string[] Scenes =
        {
            "Assets/Scenes/Ohirunebeya_tuti_5.unity",
            "Assets/Scenes/Ohirunebeya_tuti_6_boss_1.unity",
            "Assets/Scenes/Ohirunebeya_tuti_6_boss_2.unity",
            "Assets/Scenes/Ohirunebeya_tuti_6_boss_3.unity",
        };

        // 削除対象: シーン → ルート名
        static readonly Dictionary<string, string[]> StaleDoors = new Dictionary<string, string[]>
        {
            { "Assets/Scenes/Ohirunebeya_tuti_6_boss_1.unity", new[] { "GameEventTransition_1", "GameEventTransition_2" } },
            { "Assets/Scenes/Ohirunebeya_tuti_6_boss_3.unity", new[] { "GameEventTransition_1" } },
        };

        [MenuItem("Tools/Issue667/Report Tuti Transitions")]
        public static void Report()
        {
            var sb = new StringBuilder();
            foreach (var path in Scenes)
            {
                var scene = OpenIfNeeded(path);
                sb.AppendLine($"==== {path} (buildIndex={UnityEngine.SceneManagement.SceneUtility.GetBuildIndexByScenePath(path)}) ====");
                var spawns = Object.FindObjectsOfType<TransitionSpawnPoint>(true);
                foreach (var sp in spawns)
                    sb.AppendLine($"[SpawnPoint] {Path(sp.transform)} name='{sp.spawnPointName}' pos={sp.transform.position}");
                foreach (var te in Object.FindObjectsOfType<TransitionEvent>(true))
                {
                    var so = new SerializedObject(te);
                    int idx = so.FindProperty("sceneToLoad").intValue;
                    var box = te.GetComponent<BoxCollider2D>();
                    string boxInfo = box ? $"trigger={box.isTrigger} bounds={box.bounds.min}..{box.bounds.max}" : "no BoxCollider2D";
                    var inside = box ? spawns.Where(s => box.bounds.Contains(new Vector3(s.transform.position.x, s.transform.position.y, box.bounds.center.z))).Select(s => s.spawnPointName).ToArray() : Array.Empty<string>();
                    sb.AppendLine($"[TransitionEvent] {Path(te.transform)} active={te.gameObject.activeInHierarchy} sceneToLoad={idx} ({SceneName(idx)}) spawn='{so.FindProperty("destinationSpawnPointName").stringValue}' pos={te.transform.position} {boxInfo} spawnPointsInside=[{string.Join(",", inside)}]");
                    var ge = te.GetComponent<GameEvent>();
                    if (ge != null)
                    {
                        var arr = new SerializedObject(ge).FindProperty("flagToBlockName");
                        for (int i = 0; i < arr.arraySize; i++)
                        {
                            var e = arr.GetArrayElementAtIndex(i);
                            sb.AppendLine($"    GameEvent flag={(Flag)e.FindPropertyRelative("flag").enumValueIndex} block='{e.FindPropertyRelative("blockName").stringValue}' auto={e.FindPropertyRelative("isAutoStart").boolValue} collision={e.FindPropertyRelative("isCollisionStart").boolValue} info='{ge.InformationText}'");
                        }
                    }
                }
                sb.AppendLine("-- ExecuteTransition を呼ぶ InvokeMethod --");
                foreach (var fc in Object.FindObjectsOfType<Flowchart>(true))
                {
                    foreach (var block in fc.GetComponents<Block>())
                    {
                        foreach (var cmd in block.CommandList)
                        {
                            if (!(cmd is InvokeMethod)) continue;
                            var so = new SerializedObject(cmd);
                            if (so.FindProperty("targetMethod").stringValue != "ExecuteTransition") continue;
                            var target = so.FindProperty("targetObject").objectReferenceValue as GameObject;
                            sb.AppendLine($"  {Path(fc.transform)} block='{block.BlockName}' handler={(block._EventHandler ? block._EventHandler.GetType().Name : "none")} -> {(target ? Path(target.transform) : "null")}");
                        }
                    }
                }
                sb.AppendLine("-- Health.dieEvent の永続リスナー --");
                foreach (var h in Object.FindObjectsOfType<Health>(true))
                {
                    int n = h.dieEvent.GetPersistentEventCount();
                    if (n == 0) continue;
                    var parts = new List<string>();
                    for (int i = 0; i < n; i++)
                    {
                        var t = h.dieEvent.GetPersistentTarget(i);
                        parts.Add($"{(t is Component c ? Path(c.transform) : t?.name)}.{h.dieEvent.GetPersistentMethodName(i)}");
                    }
                    sb.AppendLine($"  {Path(h.transform)} active={h.gameObject.activeInHierarchy} -> {string.Join(" / ", parts)}");
                }
                sb.AppendLine("-- 自動開始する GameEvent --");
                foreach (var ge in Object.FindObjectsOfType<GameEvent>(true))
                {
                    var arr = new SerializedObject(ge).FindProperty("flagToBlockName");
                    for (int i = 0; i < arr.arraySize; i++)
                    {
                        var e = arr.GetArrayElementAtIndex(i);
                        if (!e.FindPropertyRelative("isAutoStart").boolValue) continue;
                        sb.AppendLine($"  {Path(ge.transform)} active={ge.gameObject.activeInHierarchy} flag={(Flag)e.FindPropertyRelative("flag").enumValueIndex} block='{e.FindPropertyRelative("blockName").stringValue}'");
                    }
                }
            }
            Directory.CreateDirectory("Temp");
            File.WriteAllText("Temp/issue667_report.txt", sb.ToString());
            Debug.Log("[Issue667] report written: Temp/issue667_report.txt");
        }

        [MenuItem("Tools/Issue667/Apply (Remove Stale Doors)")]
        public static void Apply()
        {
            foreach (var kv in StaleDoors)
            {
                var scene = OpenIfNeeded(kv.Key);
                foreach (var name in kv.Value)
                {
                    var go = scene.GetRootGameObjects().FirstOrDefault(g => g.name == name);
                    if (go == null) { Debug.LogWarning($"[Issue667] {kv.Key}: {name} なし"); continue; }
                    var te = go.GetComponent<TransitionEvent>();
                    int idx = te ? new SerializedObject(te).FindProperty("sceneToLoad").intValue : -1;
                    Debug.Log($"[Issue667] {System.IO.Path.GetFileNameWithoutExtension(kv.Key)}: {name} (sceneToLoad={idx} {SceneName(idx)}) を削除");
                    Object.DestroyImmediate(go);
                }
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Issue667] {kv.Key} を保存");
            }
        }

        // ---- プレイモード検証用(Menu.Execute から叩く) ----
        [MenuItem("Tools/Issue667/Debug/Log Scene")]
        public static void DebugLogScene()
        {
            var s = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            Debug.Log($"[Issue667] active scene buildIndex={s.buildIndex} name={s.name}");
        }

        [MenuItem("Tools/Issue667/Debug/Log Doors")]
        public static void DebugLogDoors()
        {
            var player = Object.FindObjectsOfType<PlayerController>().FirstOrDefault(p => p.gameObject.activeInHierarchy);
            Vector3 ppos = player ? player.transform.position : Vector3.positiveInfinity;
            Debug.Log($"[Issue667] player={(player ? Path(player.transform) : "none")} pos={ppos}");
            foreach (var te in Object.FindObjectsOfType<TransitionEvent>(true))
            {
                var so = new SerializedObject(te);
                int idx = so.FindProperty("sceneToLoad").intValue;
                var box = te.GetComponent<BoxCollider2D>();
                bool inside = player && box && box.OverlapPoint(ppos);
                Debug.Log($"[Issue667] door {Path(te.transform)} active={te.gameObject.activeInHierarchy} sceneToLoad={idx} ({SceneName(idx)}) spawn='{so.FindProperty("destinationSpawnPointName").stringValue}' pos={te.transform.position} playerInside={inside}");
            }
            foreach (var it in Object.FindObjectsOfType<InformationText>())
                Debug.Log($"[Issue667] InformationText shown: '{it.GetComponentInChildren<TMPro.TMP_Text>()?.text ?? it.GetComponentInChildren<UnityEngine.UI.Text>()?.text}' at {it.transform.position}");
        }

        [MenuItem("Tools/Issue667/Debug/Execute Door 1")] public static void DebugDoor1() => DebugExecuteDoor("GameEventTransition_1");
        [MenuItem("Tools/Issue667/Debug/Execute Door 2")] public static void DebugDoor2() => DebugExecuteDoor("GameEventTransition_2");

        static void DebugExecuteDoor(string name)
        {
            var go = GameObject.Find(name);
            if (go == null) { Debug.LogError($"[Issue667] {name} なし"); return; }
            var ge = go.GetComponent<GameEvent>();
            var entry = ge.GetCurrentBlockNameFromFlag();
            Debug.Log($"[Issue667] {name}.Execute block={(entry != null ? entry.BlockName : "(default)")}");
            ge.Execute();
        }

        [MenuItem("Tools/Issue667/Debug/Advance Dialog")]
        public static void DebugAdvance()
        {
            var input = Object.FindObjectOfType<DialogInput>();
            if (input == null) { Debug.Log("[Issue667] DialogInput なし"); return; }
            input.SetNextLineFlag();
            Debug.Log("[Issue667] SetNextLineFlag");
        }

        [MenuItem("Tools/Issue667/Debug/Log Bosses")]
        public static void DebugLogBosses()
        {
            foreach (var h in Object.FindObjectsOfType<Health>(true))
            {
                if (h.dieEvent.GetPersistentEventCount() == 0) continue;
                Debug.Log($"[Issue667] boss candidate {Path(h.transform)} active={h.gameObject.activeInHierarchy} dead={h.IsDead}");
            }
        }

        [MenuItem("Tools/Issue667/Debug/Kill Boss")]
        public static void DebugKillBoss()
        {
            var boss = Object.FindObjectsOfType<Health>().FirstOrDefault(h => h.gameObject.activeInHierarchy && !h.IsDead && h.dieEvent.GetPersistentEventCount() > 0 && !h.GetComponent<PlayerController>());
            if (boss == null) { Debug.LogError("[Issue667] boss なし"); return; }
            Debug.Log($"[Issue667] Kill {Path(boss.transform)}");
            boss.Kill();
        }

        static string SceneName(int idx)
        {
            var scenes = EditorBuildSettings.scenes;
            if (idx < 0 || idx >= scenes.Length) return "?";
            return System.IO.Path.GetFileNameWithoutExtension(scenes[idx].path);
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
