using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VLCNP.Core;

namespace VLCNP.Editor
{
    /** 演算子をフラグの前に並べ、AND優先の解釈を表示する。 */
    [CustomPropertyDrawer(typeof(VisibilityFlagManagerV2.AdditionalFlag))]
    public class VisibilityFlagV2AdditionalDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            int indent = EditorGUI.indentLevel;
            position = EditorGUI.IndentedRect(position);
            EditorGUI.indentLevel = 0;
            Rect operationRect = new Rect(position.x, position.y, 65, position.height);
            Rect flagRect = new Rect(position.x + 70, position.y, position.width - 70, position.height);
            EditorGUI.PropertyField(operationRect, property.FindPropertyRelative("operation"), GUIContent.none);
            EditorGUI.PropertyField(flagRect, property.FindPropertyRelative("flag"), GUIContent.none);
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }

    /** 条件要素ごとに、編集欄と評価順序のプレビューを表示する。 */
    [CustomPropertyDrawer(typeof(VisibilityFlagManagerV2.VisibilityFlag))]
    public class VisibilityFlagV2Drawer : PropertyDrawer
    {
        private static float Line => EditorGUIUtility.singleLineHeight;
        private static float Gap => EditorGUIUtility.standardVerticalSpacing;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return Line;
            return (Line + Gap) * 3
                + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("additionalFlags"), true)
                + Gap + Line * 3;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            Rect row = new Rect(position.x, position.y, position.width, Line);
            property.isExpanded = EditorGUI.Foldout(row, property.isExpanded, label, true);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                row.y += Line + Gap;
                EditorGUI.PropertyField(row, property.FindPropertyRelative("flag"));
                row.y += Line + Gap;
                SerializedProperty additional = property.FindPropertyRelative("additionalFlags");
                row.height = EditorGUI.GetPropertyHeight(additional, true);
                EditorGUI.PropertyField(row, additional, true);
                row.y += row.height + Gap;
                row.height = Line;
                EditorGUI.PropertyField(row, property.FindPropertyRelative("isVisible"));
                row.y += Line + Gap;
                row.height = Line * 3;
                EditorGUI.SelectableLabel(EditorGUI.IndentedRect(row), "条件式（AND優先）\n" + Expression(property), EditorStyles.helpBox);
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndProperty();
        }

        private static string Expression(SerializedProperty property)
        {
            var groups = new List<string>();
            string group = FlagName(property.FindPropertyRelative("flag"));
            bool hasAnd = false;
            SerializedProperty additional = property.FindPropertyRelative("additionalFlags");
            for (int i = 0; i < additional.arraySize; i++)
            {
                SerializedProperty item = additional.GetArrayElementAtIndex(i);
                string name = FlagName(item.FindPropertyRelative("flag"));
                if (item.FindPropertyRelative("operation").intValue == (int)VisibilityFlagManagerV2.LogicalOperator.OR)
                {
                    groups.Add(hasAnd ? "(" + group + ")" : group);
                    group = name;
                    hasAnd = false;
                }
                else
                {
                    group += " AND " + name;
                    hasAnd = true;
                }
            }
            groups.Add(hasAnd ? "(" + group + ")" : group);
            return string.Join(" OR ", groups);
        }

        private static string FlagName(SerializedProperty flag)
        {
            return flag.intValue == (int)Flag.None ? "None（常にtrue）" : ((Flag)flag.intValue).ToString();
        }
    }
}
