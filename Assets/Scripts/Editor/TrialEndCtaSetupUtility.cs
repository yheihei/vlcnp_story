using System;
using System.Linq;
using Fungus;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VLCNP.UI;
using FungusMenu = Fungus.Menu;

namespace VLCNP.Editor
{
    /**
     * VeryLongFarm_1 の体験版終了イベントへ CTA フローを再現可能な形で設定する。
     */
    public static class TrialEndCtaSetupUtility
    {
        private const string ScenePath = "Assets/Scenes/VeryLongFarm_1.unity";
        private const string EventObjectName = "GameEventTrial2End";
        private const string MessageBlockName = "Message";
        private const string WishlistBlockName = "WishlistCTA";
        private const string TitleBlockName = "TitleCTA";
        private const string WishlistText = "ウィッシュリストに登録する";
        private const string TitleText = "タイトルへ戻る";
        private const string CtaMessage =
            "体験版を最後までプレイしていただき、ありがとうございました！\n" +
            "ここから先は製品版でお楽しみください。";

        [MenuItem("Tools/VLCNP/Setup/Configure Trial End CTA", false, 2200)]
        public static void Configure()
        {
            Scene scene = GetOrOpenScene();
            GameObject eventObject = FindGameObject(scene, EventObjectName);
            if (eventObject == null)
            {
                throw new InvalidOperationException($"GameObject not found: {EventObjectName}");
            }

            TrialEndCtaActions actions = eventObject.GetComponent<TrialEndCtaActions>();
            if (actions == null)
            {
                actions = Undo.AddComponent<TrialEndCtaActions>(eventObject);
            }

            Flowchart flowchart = eventObject.GetComponentInChildren<Flowchart>(true);
            if (flowchart == null)
            {
                throw new InvalidOperationException($"Flowchart not found under: {EventObjectName}");
            }

            Block messageBlock = RequireBlock(flowchart, MessageBlockName);
            Block wishlistBlock = FindOrCreateBlock(flowchart, WishlistBlockName, new Vector2(250f, 70f));
            Block titleBlock = FindOrCreateBlock(flowchart, TitleBlockName, new Vector2(250f, 170f));

            InvokeMethod stopCommand = FindInvokeMethod(messageBlock, "StopAll");
            InvokeMethod setFlagCommand = FindInvokeMethod(messageBlock, "SetFlag");
            Say messageSay = FindOrCreateCommand<Say>(flowchart, messageBlock);
            ConfigureSay(messageSay);

            FungusMenu messageWishlist = FindOrCreateMenu(flowchart, messageBlock, wishlistBlock);
            ConfigureMenu(messageWishlist, WishlistText, wishlistBlock);
            FungusMenu messageTitle = FindOrCreateMenu(flowchart, messageBlock, titleBlock);
            ConfigureMenu(messageTitle, TitleText, titleBlock);

            CallMethod openWishlist = FindOrCreateCallMethod(flowchart, wishlistBlock, "OpenWishlist");
            ConfigureCallMethod(openWishlist, eventObject, "OpenWishlist");
            Say wishlistSay = FindOrCreateCommand<Say>(flowchart, wishlistBlock);
            ConfigureSay(wishlistSay);
            FungusMenu wishlistAgain = FindOrCreateMenu(flowchart, wishlistBlock, wishlistBlock);
            ConfigureMenu(wishlistAgain, WishlistText, wishlistBlock);
            FungusMenu wishlistTitle = FindOrCreateMenu(flowchart, wishlistBlock, titleBlock);
            ConfigureMenu(wishlistTitle, TitleText, titleBlock);

            CallMethod backToTitle = FindOrCreateCallMethod(flowchart, titleBlock, "BackToTitle");
            ConfigureCallMethod(backToTitle, eventObject, "BackToTitle");

            SetCommandList(
                messageBlock,
                stopCommand,
                messageSay,
                setFlagCommand,
                messageWishlist,
                messageTitle
            );
            SetCommandList(
                wishlistBlock,
                openWishlist,
                wishlistSay,
                wishlistAgain,
                wishlistTitle
            );
            SetCommandList(titleBlock, backToTitle);

            EditorUtility.SetDirty(actions);
            EditorUtility.SetDirty(flowchart);
            PrefabUtility.RecordPrefabInstancePropertyModifications(messageBlock);
            EditorSceneManager.MarkSceneDirty(scene);

            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException($"Failed to save scene: {ScenePath}");
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[TrialEndCTA] Configured scene: {ScenePath}");
        }

        private static Scene GetOrOpenScene()
        {
            Scene loadedScene = SceneManager.GetSceneByPath(ScenePath);
            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                return loadedScene;
            }

            return EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
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

        private static Block RequireBlock(Flowchart flowchart, string blockName)
        {
            Block block = flowchart.FindBlock(blockName);
            if (block == null)
            {
                throw new InvalidOperationException($"Fungus block not found: {blockName}");
            }

            return block;
        }

        private static Block FindOrCreateBlock(Flowchart flowchart, string blockName, Vector2 position)
        {
            Block block = flowchart.FindBlock(blockName);
            if (block != null)
            {
                return block;
            }

            block = flowchart.CreateBlock(position);
            block.BlockName = blockName;
            EditorUtility.SetDirty(block);
            return block;
        }

        private static InvokeMethod FindInvokeMethod(Block block, string targetMethod)
        {
            foreach (InvokeMethod command in block.CommandList.OfType<InvokeMethod>())
            {
                SerializedObject serializedCommand = new SerializedObject(command);
                if (serializedCommand.FindProperty("targetMethod").stringValue == targetMethod)
                {
                    return command;
                }
            }

            throw new InvalidOperationException(
                $"InvokeMethod command not found. block={block.BlockName} method={targetMethod}"
            );
        }

        private static T FindOrCreateCommand<T>(Flowchart flowchart, Block block)
            where T : Command
        {
            T command = block.CommandList.OfType<T>().FirstOrDefault();
            return command != null ? command : AddCommand<T>(flowchart, block);
        }

        private static FungusMenu FindOrCreateMenu(Flowchart flowchart, Block block, Block targetBlock)
        {
            foreach (FungusMenu menu in block.CommandList.OfType<FungusMenu>())
            {
                SerializedObject serializedMenu = new SerializedObject(menu);
                if (serializedMenu.FindProperty("targetBlock").objectReferenceValue == targetBlock)
                {
                    return menu;
                }
            }

            return AddCommand<FungusMenu>(flowchart, block);
        }

        private static CallMethod FindOrCreateCallMethod(
            Flowchart flowchart,
            Block block,
            string methodName
        )
        {
            foreach (CallMethod command in block.CommandList.OfType<CallMethod>())
            {
                SerializedObject serializedCommand = new SerializedObject(command);
                if (serializedCommand.FindProperty("methodName").stringValue == methodName)
                {
                    return command;
                }
            }

            return AddCommand<CallMethod>(flowchart, block);
        }

        private static T AddCommand<T>(Flowchart flowchart, Block block)
            where T : Command
        {
            T command = Undo.AddComponent<T>(block.gameObject);
            command.ParentBlock = block;
            command.ItemId = flowchart.NextItemId();
            command.OnCommandAdded(block);
            EditorUtility.SetDirty(command);
            return command;
        }

        private static void ConfigureSay(Say say)
        {
            SerializedObject serializedSay = new SerializedObject(say);
            serializedSay.FindProperty("storyText").stringValue = CtaMessage;
            serializedSay.FindProperty("showAlways").boolValue = true;
            serializedSay.FindProperty("fadeWhenDone").boolValue = false;
            serializedSay.FindProperty("waitForClick").boolValue = false;
            serializedSay.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(say);
        }

        private static void ConfigureMenu(FungusMenu menu, string text, Block targetBlock)
        {
            SerializedObject serializedMenu = new SerializedObject(menu);
            serializedMenu.FindProperty("text").stringValue = text;
            serializedMenu.FindProperty("targetBlock").objectReferenceValue = targetBlock;
            serializedMenu.FindProperty("hideIfVisited").boolValue = false;
            serializedMenu.FindProperty("interactable.booleanRef").objectReferenceValue = null;
            serializedMenu.FindProperty("interactable.booleanVal").boolValue = true;
            serializedMenu.FindProperty("hideThisOption.booleanRef").objectReferenceValue = null;
            serializedMenu.FindProperty("hideThisOption.booleanVal").boolValue = false;
            serializedMenu.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(menu);
        }

        private static void ConfigureCallMethod(
            CallMethod command,
            GameObject targetObject,
            string methodName
        )
        {
            SerializedObject serializedCommand = new SerializedObject(command);
            serializedCommand.FindProperty("targetObject").objectReferenceValue = targetObject;
            serializedCommand.FindProperty("methodName").stringValue = methodName;
            serializedCommand.FindProperty("delay").floatValue = 0f;
            serializedCommand.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(command);
        }

        private static void SetCommandList(Block block, params Command[] commands)
        {
            Undo.RecordObject(block, "Configure trial end CTA commands");
            block.CommandList.Clear();
            block.CommandList.AddRange(commands);

            for (int index = 0; index < commands.Length; index++)
            {
                commands[index].ParentBlock = block;
                commands[index].CommandIndex = index;
                commands[index].IndentLevel = 0;
                EditorUtility.SetDirty(commands[index]);
            }

            EditorUtility.SetDirty(block);
        }
    }
}
