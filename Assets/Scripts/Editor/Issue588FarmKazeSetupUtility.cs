using System;
using System.Linq;
using Fungus;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VLCNP.Core;
using VLCNP.SceneManagement;
using FungusMenu = Fungus.Menu;

namespace VLCNP.Editor
{
    /**
     * issue #588 コメント対応の一括セットアップ
     * 1. OhirunebeyaYami1 のエリア名を「おひるねべや(闇)」に修正
     * 2. VeryLongFarm_1 の体験版終了メッセージ条件に VLNarukamiJoined を追加
     * 3. VeryLongFarm_1 の移動選択肢「おひるねべや(風)」を有効化し Kaze1 へ遷移できるようにする
     */
    public static class Issue588FarmKazeSetupUtility
    {
        private const string YamiScenePath = "Assets/Scenes/OhirunebeyaYami1.unity";
        private const string FarmScenePath = "Assets/Scenes/VeryLongFarm_1.unity";
        private const int Kaze1BuildIndex = 37;

        [MenuItem("Tools/VLCNP/Setup/Issue588 Farm Kaze Setup", false, 2300)]
        public static void Configure()
        {
            ConfigureYamiAreaName();
            ConfigureFarm();
            AssetDatabase.SaveAssets();
            Debug.Log("[Issue588] Setup finished.");
        }

        private static void ConfigureYamiAreaName()
        {
            Scene scene = EditorSceneManager.OpenScene(YamiScenePath, OpenSceneMode.Single);
            GameObject areaNameObject = GameObject.FindWithTag("AreaName");
            if (areaNameObject == null)
            {
                throw new InvalidOperationException("AreaName object not found in OhirunebeyaYami1");
            }
            Text text = areaNameObject.GetComponentInChildren<Text>(true);
            if (text == null)
            {
                throw new InvalidOperationException("AreaName Text not found in OhirunebeyaYami1");
            }
            Undo.RecordObject(text, "Fix Yami area name");
            text.text = "おひるねべや(闇)";
            EditorUtility.SetDirty(text);
            PrefabUtility.RecordPrefabInstancePropertyModifications(text);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException($"Failed to save scene: {YamiScenePath}");
            }
            Debug.Log("[Issue588] OhirunebeyaYami1 AreaName updated: " + text.text);
        }

        private static void ConfigureFarm()
        {
            Scene scene = EditorSceneManager.OpenScene(FarmScenePath, OpenSceneMode.Single);
            AddNarukamiConditionToTrialEnd(scene);
            ConfigureKazeWarp(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException($"Failed to save scene: {FarmScenePath}");
            }
        }

        private static void AddNarukamiConditionToTrialEnd(Scene scene)
        {
            GameObject eventObject = FindGameObject(scene, "GameEventTrial2End");
            if (eventObject == null)
            {
                throw new InvalidOperationException("GameEventTrial2End not found");
            }
            MultiFlagGameEvent multiFlagGameEvent = eventObject.GetComponent<MultiFlagGameEvent>();
            if (multiFlagGameEvent == null)
            {
                throw new InvalidOperationException("MultiFlagGameEvent not found on GameEventTrial2End");
            }
            SerializedObject serialized = new SerializedObject(multiFlagGameEvent);
            SerializedProperty entries = serialized.FindProperty("flagConditionToBlockName");
            SerializedProperty targetConditions = null;
            for (int i = 0; i < entries.arraySize; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                if (entry.FindPropertyRelative("blockName").stringValue != "Message") continue;
                targetConditions = entry.FindPropertyRelative("conditions");
                break;
            }
            if (targetConditions == null)
            {
                throw new InvalidOperationException("Message entry not found in MultiFlagGameEvent");
            }
            for (int i = 0; i < targetConditions.arraySize; i++)
            {
                SerializedProperty condition = targetConditions.GetArrayElementAtIndex(i);
                if (condition.FindPropertyRelative("flag").intValue == (int)Flag.VLNarukamiJoined)
                {
                    Debug.Log("[Issue588] VLNarukamiJoined condition already exists. Skipped.");
                    return;
                }
            }
            targetConditions.arraySize += 1;
            SerializedProperty added = targetConditions.GetArrayElementAtIndex(targetConditions.arraySize - 1);
            added.FindPropertyRelative("flag").intValue = (int)Flag.VLNarukamiJoined;
            added.FindPropertyRelative("expectedValue").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(multiFlagGameEvent);
            Debug.Log("[Issue588] Added VLNarukamiJoined condition to trial end message.");
        }

        private static void ConfigureKazeWarp(Scene scene)
        {
            GameObject deguchi = FindGameObject(scene, "DeguchiGameEvent");
            if (deguchi == null)
            {
                throw new InvalidOperationException("DeguchiGameEvent not found");
            }
            Flowchart flowchart = deguchi.GetComponentInChildren<Flowchart>(true);
            if (flowchart == null)
            {
                throw new InvalidOperationException("Flowchart not found under DeguchiGameEvent");
            }

            TransitionEvent kazeTransition = FindOrCreateKazeTransition(scene);

            // GoToKaze ブロックを作成し、GoToYami と同じ形式の InvokeMethod で遷移させる
            Block goToYami = flowchart.FindBlock("GoToYami");
            if (goToYami == null)
            {
                throw new InvalidOperationException("GoToYami block not found");
            }
            InvokeMethod yamiInvoke = goToYami.CommandList.OfType<InvokeMethod>().FirstOrDefault();
            if (yamiInvoke == null)
            {
                throw new InvalidOperationException("InvokeMethod not found in GoToYami block");
            }

            Block goToKaze = flowchart.FindBlock("GoToKaze");
            if (goToKaze == null)
            {
                goToKaze = flowchart.CreateBlock(new Vector2(-82f, 340f));
                goToKaze.BlockName = "GoToKaze";
                SerializedObject serializedBlock = new SerializedObject(goToKaze);
                serializedBlock.FindProperty("description").stringValue = "おひるねべや(風)へ";
                serializedBlock.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(goToKaze);
            }
            InvokeMethod kazeInvoke = goToKaze.CommandList.OfType<InvokeMethod>().FirstOrDefault();
            if (kazeInvoke == null)
            {
                kazeInvoke = Undo.AddComponent<InvokeMethod>(goToKaze.gameObject);
                EditorUtility.CopySerialized(yamiInvoke, kazeInvoke);
                SerializedObject serializedInvoke = new SerializedObject(kazeInvoke);
                serializedInvoke.FindProperty("itemId").intValue = flowchart.NextItemId();
                serializedInvoke.FindProperty("targetObject").objectReferenceValue = kazeTransition.gameObject;
                serializedInvoke.ApplyModifiedPropertiesWithoutUndo();
                kazeInvoke.ParentBlock = goToKaze;
                goToKaze.CommandList.Clear();
                goToKaze.CommandList.Add(kazeInvoke);
                EditorUtility.SetDirty(kazeInvoke);
                EditorUtility.SetDirty(goToKaze);
            }

            // Message2 の「おひるねべや(風)」選択肢を表示し GoToKaze につなぐ
            Block message2 = flowchart.FindBlock("Message2");
            if (message2 == null)
            {
                throw new InvalidOperationException("Message2 block not found");
            }
            FungusMenu kazeMenu = message2.CommandList
                .OfType<FungusMenu>()
                .FirstOrDefault(m =>
                {
                    SerializedObject serializedMenu = new SerializedObject(m);
                    return serializedMenu.FindProperty("text").stringValue == "おひるねべや(風)";
                });
            if (kazeMenu == null)
            {
                throw new InvalidOperationException("おひるねべや(風) menu not found in Message2");
            }
            SerializedObject serializedKazeMenu = new SerializedObject(kazeMenu);
            serializedKazeMenu.FindProperty("targetBlock").objectReferenceValue = goToKaze;
            serializedKazeMenu.FindProperty("hideThisOption.booleanRef").objectReferenceValue = null;
            serializedKazeMenu.FindProperty("hideThisOption.booleanVal").boolValue = false;
            serializedKazeMenu.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(kazeMenu);
            EditorUtility.SetDirty(flowchart);
            PrefabUtility.RecordPrefabInstancePropertyModifications(kazeMenu);
            Debug.Log("[Issue588] おひるねべや(風) menu wired to GoToKaze block.");
        }

        private static TransitionEvent FindOrCreateKazeTransition(Scene scene)
        {
            GameObject existing = FindGameObject(scene, "GameEventTransition_1_kaze");
            if (existing != null)
            {
                return existing.GetComponent<TransitionEvent>();
            }
            GameObject yamiTransition = FindGameObject(scene, "GameEventTransition_1_yami");
            if (yamiTransition == null)
            {
                throw new InvalidOperationException("GameEventTransition_1_yami not found");
            }
            // プレハブ接続を保ったまま複製する
            Selection.activeGameObject = yamiTransition;
            Unsupported.DuplicateGameObjectsUsingPasteboard();
            GameObject kazeObject = Selection.activeGameObject;
            if (kazeObject == null || kazeObject == yamiTransition)
            {
                throw new InvalidOperationException("Failed to duplicate GameEventTransition_1_yami");
            }
            kazeObject.name = "GameEventTransition_1_kaze";
            TransitionEvent transitionEvent = kazeObject.GetComponent<TransitionEvent>();
            if (transitionEvent == null)
            {
                throw new InvalidOperationException("TransitionEvent not found on duplicated object");
            }
            SerializedObject serialized = new SerializedObject(transitionEvent);
            serialized.FindProperty("sceneToLoad").intValue = Kaze1BuildIndex;
            serialized.FindProperty("destinationSpawnPointName").stringValue = "A";
            serialized.FindProperty("isShowAreaName").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(transitionEvent);
            PrefabUtility.RecordPrefabInstancePropertyModifications(transitionEvent);
            Debug.Log("[Issue588] GameEventTransition_1_kaze created. sceneToLoad=" + Kaze1BuildIndex);
            return transitionEvent;
        }

        private static GameObject FindGameObject(Scene scene, string objectName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform match = root
                    .GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(candidate => candidate.name == objectName);
                if (match != null)
                {
                    return match.gameObject;
                }
            }
            return null;
        }
    }
}
