using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TilePaletteZoomResetMenu
{
    private const string MainPalettePath = "Assets/ExtraPackage/SunnyLand Artwork/Environment/Tileset/Main Palette.prefab";
    private const float TargetPixelsPerCell = 100f;

    [MenuItem("Tools/Tile Palette/Reset Zoom %#t", false, 2000)]
    public static void ResetZoom()
    {
        var palette = AssetDatabase.LoadAssetAtPath<GameObject>(MainPalettePath);
        if (palette == null)
        {
            Debug.LogError($"Tile Palette reset failed. Palette not found: {MainPalettePath}");
            return;
        }

        EnsurePaletteGridSize(palette);

        var windowType = FindType("UnityEditor.Tilemaps.GridPaintPaletteWindow");
        if (windowType == null)
        {
            Debug.LogError("Tile Palette reset failed. GridPaintPaletteWindow type was not found.");
            return;
        }

        var window = GetTilePaletteWindow(windowType);
        if (window == null)
        {
            Debug.LogError("Tile Palette reset failed. Tile Palette window could not be opened.");
            return;
        }

        window.titleContent = new GUIContent("Tile Palette");
        window.Show();
        window.Focus();
        SetProperty(windowType, window, "palette", palette);

        var clipboard = GetClipboard(windowType, window);
        if (clipboard == null)
        {
            Debug.LogError("Tile Palette reset failed. Clipboard view was not initialized.");
            return;
        }

        ResetClipboardCamera(clipboard);
        window.Repaint();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        Debug.Log("Tile Palette zoom reset: 1 cell is approximately 100px. Shortcut: Cmd/Ctrl+Shift+T.");
    }

    private static void EnsurePaletteGridSize(GameObject palette)
    {
        var grid = palette.GetComponent<Grid>();
        if (grid == null)
        {
            return;
        }

        var expectedCellSize = new Vector3(1f, 1f, 0f);
        if (grid.cellSize == expectedCellSize && grid.cellGap == Vector3.zero)
        {
            return;
        }

        grid.cellSize = expectedCellSize;
        grid.cellGap = Vector3.zero;
        EditorUtility.SetDirty(palette);
        AssetDatabase.SaveAssets();
    }

    private static EditorWindow GetTilePaletteWindow(Type windowType)
    {
        var getWindow = typeof(EditorWindow)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(method => method.Name == "GetWindow" && method.IsGenericMethodDefinition && method.GetParameters().Length == 0)
            .MakeGenericMethod(windowType);

        return getWindow.Invoke(null, null) as EditorWindow;
    }

    private static object GetClipboard(Type windowType, EditorWindow window)
    {
        var splitView = GetField(windowType, window, "m_ClipboardSplitView");
        if (splitView == null)
        {
            return null;
        }

        var paletteElement = GetProperty(splitView.GetType(), splitView, "paletteElement");
        if (paletteElement == null)
        {
            return null;
        }

        return GetProperty(paletteElement.GetType(), paletteElement, "clipboardView");
    }

    private static void ResetClipboardCamera(object clipboard)
    {
        var clipboardType = clipboard.GetType();
        InvokeMethod(clipboardType, clipboard, "ResetPreviewInstance");
        SetField(clipboardType, clipboard, "m_CameraInitializedToBounds", true);
        SetField(clipboardType, clipboard, "m_CameraSwizzleView", GridLayout.CellSwizzle.XYZ);

        var paletteInstance = GetProperty(clipboardType, clipboard, "paletteInstance") as GameObject;
        if (paletteInstance == null)
        {
            Debug.LogError("Tile Palette reset failed. Preview instance was not created.");
            return;
        }

        var grid = paletteInstance.GetComponent<Grid>();
        var tilemap = paletteInstance.GetComponentInChildren<Tilemap>();
        if (grid == null || tilemap == null)
        {
            Debug.LogError("Tile Palette reset failed. Preview Grid or Tilemap was not found.");
            return;
        }

        grid.cellSize = new Vector3(1f, 1f, 0f);
        grid.cellGap = Vector3.zero;
        tilemap.CompressBounds();

        var bounds = tilemap.cellBounds;
        var center = grid.CellToLocalInterpolated(new Vector3(bounds.center.x, bounds.center.y, 0f));
        var guiRect = (Rect)GetField(clipboardType, clipboard, "m_GUIRect");
        var viewHeight = guiRect.height > 10f ? guiRect.height : 530f;
        var targetOrthographicSize = viewHeight / (TargetPixelsPerCell * 2f);
        var cameraPosition = new Vector3(center.x, center.y, -10f);

        SetProperty(clipboardType, clipboard, "cameraPosition", cameraPosition);
        SetProperty(clipboardType, clipboard, "cameraSize", targetOrthographicSize);

        var livePosition = (Vector3)GetProperty(clipboardType, clipboard, "cameraPosition");
        var liveSize = (float)GetProperty(clipboardType, clipboard, "cameraSize");
        SetField(clipboardType, clipboard, "m_CameraPosition", livePosition);
        SetField(clipboardType, clipboard, "m_CameraOrthographicSize", liveSize);
        SetField(clipboardType, clipboard, "m_CameraPositionSaved", true);
    }

    private static Type FindType(string fullName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var type = assembly.GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }
            catch
            {
                // Some Unity assemblies can fail type inspection during reload.
            }
        }

        return null;
    }

    private static object GetField(Type type, object instance, string name)
    {
        return type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(instance);
    }

    private static void SetField(Type type, object instance, string name, object value)
    {
        type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(instance, value);
    }

    private static object GetProperty(Type type, object instance, string name)
    {
        return type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(instance);
    }

    private static void SetProperty(Type type, object instance, string name, object value)
    {
        type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(instance, value);
    }

    private static void InvokeMethod(Type type, object instance, string name)
    {
        type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(instance, null);
    }
}
