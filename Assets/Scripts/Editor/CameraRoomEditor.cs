using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using VLCNP.Movie;

/**
 * カメラ区画(CameraRoom)を選んだとき、範囲の矩形と映す下端・上端をシーンビューで動かせるようにする。
 * 値は 0.5 刻みにそろえる。
 */
[CustomEditor(typeof(CameraRoom))]
public class CameraRoomEditor : Editor
{
    const float Snap = 0.5f;

    readonly BoxBoundsHandle boundsHandle = new BoxBoundsHandle
    {
        axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y,
    };

    void OnSceneGUI()
    {
        CameraRoom room = (CameraRoom)target;
        Vector3 origin = room.transform.position;
        Color color = room.Mode == CameraRoom.CameraMode.Fixed ? new Color(0.23f, 0.63f, 1f) : new Color(1f, 0.6f, 0.18f);
        serializedObject.Update();

        SerializedProperty areas = serializedObject.FindProperty("areas");
        float xMin = float.MaxValue;
        float xMax = float.MinValue;
        float yMax = float.MinValue;
        for (int i = 0; i < areas.arraySize; i++)
        {
            SerializedProperty area = areas.GetArrayElementAtIndex(i);
            Rect rect = area.rectValue;
            xMin = Mathf.Min(xMin, origin.x + rect.xMin);
            xMax = Mathf.Max(xMax, origin.x + rect.xMax);
            yMax = Mathf.Max(yMax, origin.y + rect.yMax);

            boundsHandle.center = origin + (Vector3)rect.center;
            boundsHandle.size = new Vector3(rect.width, rect.height, 0f);
            boundsHandle.SetColor(color);
            EditorGUI.BeginChangeCheck();
            boundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Vector3 min = boundsHandle.center - boundsHandle.size / 2f - origin;
                Vector3 max = boundsHandle.center + boundsHandle.size / 2f - origin;
                area.rectValue = Rect.MinMaxRect(
                    Round(Mathf.Min(min.x, max.x)),
                    Round(Mathf.Min(min.y, max.y)),
                    Round(Mathf.Max(min.x, max.x)),
                    Round(Mathf.Max(min.y, max.y))
                );
            }
        }

        if (areas.arraySize > 0)
        {
            float x = (xMin + xMax) / 2f;
            DrawHeightHandle(serializedObject.FindProperty("viewBottom"), origin, x, color, "映す下端");
            DrawHeightHandle(serializedObject.FindProperty("viewTop"), origin, x, color, "映す上端");
            string mode = room.Mode == CameraRoom.CameraMode.Fixed ? "固定" : "追従";
            string priority = room.Priority != 0 ? $"・優先度{room.Priority}" : "";
            Handles.Label(new Vector3(xMin, yMax, origin.z), $"{room.name}({mode}{priority})");
        }
        serializedObject.ApplyModifiedProperties();
    }

    static void DrawHeightHandle(SerializedProperty height, Vector3 origin, float x, Color color, string label)
    {
        Vector3 position = new Vector3(x, origin.y + height.floatValue, origin.z);
        float size = HandleUtility.GetHandleSize(position) * 0.08f;
        Handles.color = color;
        EditorGUI.BeginChangeCheck();
        Vector3 moved = Handles.Slider(position, Vector3.up, size, Handles.DotHandleCap, 0f);
        if (EditorGUI.EndChangeCheck())
            height.floatValue = Round(moved.y - origin.y);
        Handles.Label(position + Vector3.right * size * 2f, label);
    }

    static float Round(float value)
    {
        return Mathf.Round(value / Snap) * Snap;
    }
}
