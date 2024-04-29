using UnityEngine;
using UnityEditor;
using VLCNP.Combat.EnemyAction;
using System.Collections.Generic;
using System;


[CustomEditor(typeof(MonoBehaviour), true)]
public class CustomInspectorColorEditor : Editor
{
    private static Dictionary<Type, Color> interfaceColorMapping;

    static CustomInspectorColorEditor()
    {
        interfaceColorMapping = new Dictionary<Type, Color>
        {
            { typeof(IEnemyAction), Color.blue }
            // 他のインターフェースと色のマッピングをここに追加
        };
    }

    public override void OnInspectorGUI()
    {
        MonoBehaviour myTarget = (MonoBehaviour) target;

        // 特定のインターフェースの場合 色を変更
        foreach (var pair in interfaceColorMapping)
        {
            if (myTarget.GetType().GetInterface(pair.Key.Name) != null)
            {
                GUI.backgroundColor = pair.Value;
                break; // 最初に見つかったマッチで色を設定
            }
        }

        // 通常のインスペクターGUIを描画
        base.OnInspectorGUI();
    }
}
