using System.Collections.Generic;
using TNRD;
using UnityEditor;
using UnityEngine;
using VLCNP.Combat.EnemyAction;

/**
 * VLNarukamiBoss へ FlapAndReturnToHeight(高さリセット)を追加し、
 * 急降下攻撃(CrescentSwoopCharge)直後の Waiting をこれに差し替えるセットアップ。
 * 再実行しても安全(差し替え済みなら何もしない)。
 */
public static class NarukamiReturnHeightBuilder
{
    private const string BossPrefabPath = "Assets/Game/Characters/Enemy/VLNarukamiBoss.prefab";
    private const float TargetY = 4.24f;

    [MenuItem("Tools/VLCNP/Narukami Return Height/Setup All")]
    public static void SetupAll()
    {
        bool replaced = AddActionAndReplaceWaitAfterSwoop();
        Debug.Log($"NarukamiReturnHeightBuilder: 完了。急降下後Waitingの差し替え={replaced}");
    }

    private static bool AddActionAndReplaceWaitAfterSwoop()
    {
        GameObject bossRoot = PrefabUtility.LoadPrefabContents(BossPrefabPath);
        try
        {
            FlapAndReturnToHeight action = bossRoot.GetComponent<FlapAndReturnToHeight>();
            if (action == null)
                action = bossRoot.AddComponent<FlapAndReturnToHeight>();

            SerializedObject serializedAction = new SerializedObject(action);
            serializedAction.FindProperty("targetY").floatValue = TargetY;
            serializedAction.ApplyModifiedPropertiesWithoutUndo();

            bool replaced = false;
            EnemyV2Controller controller = bossRoot.GetComponent<EnemyV2Controller>();
            if (controller != null)
            {
                List<SerializableInterface<IEnemyAction>> actions = controller.enemyActions;
                for (int i = 0; i < actions.Count - 1; i++)
                {
                    if (!(actions[i]?.Value is CrescentSwoopCharge))
                        continue;

                    if (actions[i + 1]?.Value is FlapAndReturnToHeight)
                    {
                        // 差し替え済み
                        replaced = true;
                    }
                    else if (actions[i + 1]?.Value is Waiting)
                    {
                        actions[i + 1].Value = action;
                        replaced = true;
                    }
                    break;
                }
            }

            PrefabUtility.SaveAsPrefabAsset(bossRoot, BossPrefabPath);
            return replaced;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(bossRoot);
        }
    }
}
