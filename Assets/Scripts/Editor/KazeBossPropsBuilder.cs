using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/**
 * KazeBoss(風エリア ボス部屋: 荒廃した武家屋敷)のプロップ Prefab を構築するエディタ拡張。
 * Prefabs フォルダ内の PNG それぞれについて、
 *   1. テクスチャインポート設定の統一(PPU32 / Point / mipmap off / 非圧縮 / 透過あり)
 *   2. SpriteRenderer だけを持つ Prefab の生成
 * を行う。当たり判定は付けない(足場が必要な箇所は ground タイルや InvisibleWall で組む)。
 * PNG を差し替えた後に再実行しても同じ結果になる。
 */
public static class KazeBossPropsBuilder
{
    private const string PrefabsDir = "Assets/Game/MapObject/KazeBossTilemap/Prefabs";
    private const string SortingLayer = "Default";
    private const int PixelsPerUnit = 32;

    [MenuItem("Tools/VLCNP/MapObject/Build KazeBoss Props", false, 3211)]
    public static void Build()
    {
        var pngPaths = Directory.GetFiles(PrefabsDir, "*.png")
            .Select(p => p.Replace('\\', '/'))
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();

        if (pngPaths.Count == 0)
        {
            throw new InvalidOperationException($"プロップ PNG が見つかりません: {PrefabsDir}");
        }

        var importerFixed = 0;
        var created = 0;
        var updated = 0;

        foreach (var path in pngPaths)
        {
            if (ConfigureTexture(path))
            {
                importerFixed++;
            }

            var stem = Path.GetFileNameWithoutExtension(path);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new InvalidOperationException($"Sprite not loaded: {path}");
            }

            var prefabPath = $"{PrefabsDir}/{stem}.prefab";
            var existed = File.Exists(prefabPath);

            var go = new GameObject(stem);
            try
            {
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingLayerName = SortingLayer;
                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }

            if (existed)
            {
                updated++;
            }
            else
            {
                created++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[KazeBossPropsBuilder] png={pngPaths.Count} importerFixed={importerFixed} " +
                  $"prefabCreated={created} prefabUpdated={updated}");
    }

    private static bool ConfigureTexture(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            throw new InvalidOperationException($"TextureImporter not found: {path}");
        }

        var dirty = false;
        dirty |= Set(importer.textureType != TextureImporterType.Sprite, () => importer.textureType = TextureImporterType.Sprite);
        dirty |= Set(importer.spriteImportMode != SpriteImportMode.Single, () => importer.spriteImportMode = SpriteImportMode.Single);
        dirty |= Set(!Mathf.Approximately(importer.spritePixelsPerUnit, PixelsPerUnit), () => importer.spritePixelsPerUnit = PixelsPerUnit);
        dirty |= Set(importer.filterMode != FilterMode.Point, () => importer.filterMode = FilterMode.Point);
        dirty |= Set(importer.mipmapEnabled, () => importer.mipmapEnabled = false);
        dirty |= Set(!importer.alphaIsTransparency, () => importer.alphaIsTransparency = true);
        dirty |= Set(importer.textureCompression != TextureImporterCompression.Uncompressed, () => importer.textureCompression = TextureImporterCompression.Uncompressed);
        dirty |= Set(importer.wrapMode != TextureWrapMode.Clamp, () => importer.wrapMode = TextureWrapMode.Clamp);
        dirty |= Set(importer.spritePivot != new Vector2(0.5f, 0.5f), () => importer.spritePivot = new Vector2(0.5f, 0.5f));

        if (dirty)
        {
            importer.SaveAndReimport();
        }

        return dirty;
    }

    private static bool Set(bool needsChange, Action apply)
    {
        if (!needsChange)
        {
            return false;
        }

        apply();
        return true;
    }
}
