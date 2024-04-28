using UnityEngine;
using UnityEditor;
using VLCNP.Combat.EnemyAction;

[CustomEditor(typeof(MonoBehaviour), true)]
public class CustomInspectorColorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MonoBehaviour myTarget = (MonoBehaviour) target;

        if (myTarget is IEnemyAction)
        {
            // 特定のインターフェースを実装している場合の色変更
            GUI.backgroundColor = Color.blue;
        }

        // 通常のインスペクターGUIを描画
        base.OnInspectorGUI();
    }
}
