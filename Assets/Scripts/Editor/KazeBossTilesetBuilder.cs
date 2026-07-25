using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/**
 * KazeBoss(風エリア ボス部屋: 荒廃した武家屋敷)のタイルセットを構築するエディタ拡張。
 * Tiles フォルダ内の 32x32 PNG に対して、
 *   1. テクスチャインポート設定の統一(PPU32 / Point / mipmap off / 非圧縮)
 *   2. Tile アセット生成(bg_* はコライダー無し、ground_* は Sprite コライダー)
 *   3. Tile Palette プレハブ生成
 * を行う。PNG を差し替えた後に再実行しても同じ結果になる。
 */
public static class KazeBossTilesetBuilder
{
    private const string TilesDir = "Assets/Game/MapObject/KazeBossTilemap/Tiles";
    private const string PaletteDir = "Assets/Game/MapObject/KazeBossTilemap/Palette";
    private const string PalettePath = PaletteDir + "/KazeBossPalette.prefab";
    private const string GroundPrefix = "ground_";
    private const string BgPrefix = "bg_";
    private const int TileSize = 32;
    private const int PaletteColumns = 8;

    [MenuItem("Tools/VLCNP/MapObject/Build KazeBoss Tileset", false, 3210)]
    public static void Build()
    {
        var pngPaths = Directory.GetFiles(TilesDir, "*.png")
            .Select(p => p.Replace('\\', '/'))
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();

        if (pngPaths.Count == 0)
        {
            throw new InvalidOperationException($"タイル PNG が見つかりません: {TilesDir}");
        }

        var importerFixed = ConfigureTextures(pngPaths);
        var tiles = BuildTileAssets(pngPaths);
        BuildPalette(tiles);

        var bgCount = tiles.Count(t => t.name.StartsWith(BgPrefix, StringComparison.Ordinal));
        var groundCount = tiles.Count(t => t.name.StartsWith(GroundPrefix, StringComparison.Ordinal));
        Debug.Log($"[KazeBossTilesetBuilder] png={pngPaths.Count} importerFixed={importerFixed} " +
                  $"tiles={tiles.Count} (bg={bgCount}, ground={groundCount}) palette={PalettePath}");
    }

    private static int ConfigureTextures(IEnumerable<string> pngPaths)
    {
        var fixedCount = 0;
        foreach (var path in pngPaths)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"TextureImporter not found: {path}");
            }

            var dirty = false;
            dirty |= Set(importer.textureType != TextureImporterType.Sprite, () => importer.textureType = TextureImporterType.Sprite);
            dirty |= Set(importer.spriteImportMode != SpriteImportMode.Single, () => importer.spriteImportMode = SpriteImportMode.Single);
            dirty |= Set(!Mathf.Approximately(importer.spritePixelsPerUnit, TileSize), () => importer.spritePixelsPerUnit = TileSize);
            dirty |= Set(importer.filterMode != FilterMode.Point, () => importer.filterMode = FilterMode.Point);
            dirty |= Set(importer.mipmapEnabled, () => importer.mipmapEnabled = false);
            dirty |= Set(!importer.alphaIsTransparency, () => importer.alphaIsTransparency = true);
            dirty |= Set(importer.textureCompression != TextureImporterCompression.Uncompressed, () => importer.textureCompression = TextureImporterCompression.Uncompressed);
            dirty |= Set(importer.wrapMode != TextureWrapMode.Clamp, () => importer.wrapMode = TextureWrapMode.Clamp);
            dirty |= Set(importer.spritePivot != new Vector2(0.5f, 0.5f), () => importer.spritePivot = new Vector2(0.5f, 0.5f));

            if (dirty)
            {
                importer.SaveAndReimport();
                fixedCount++;
            }
        }

        return fixedCount;
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

    private static List<TileBase> BuildTileAssets(IEnumerable<string> pngPaths)
    {
        var result = new List<TileBase>();
        foreach (var path in pngPaths)
        {
            var stem = Path.GetFileNameWithoutExtension(path);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new InvalidOperationException($"Sprite not loaded: {path}");
            }

            if (sprite.texture.width != TileSize || sprite.texture.height != TileSize)
            {
                throw new InvalidOperationException(
                    $"タイルは {TileSize}x{TileSize} である必要があります: {path} は {sprite.texture.width}x{sprite.texture.height}");
            }

            var tilePath = $"{TilesDir}/{stem}_tile.asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
            var isNew = tile == null;
            if (isNew)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
            }

            tile.sprite = sprite;
            tile.color = Color.white;
            tile.colliderType = stem.StartsWith(GroundPrefix, StringComparison.Ordinal)
                ? Tile.ColliderType.Sprite
                : Tile.ColliderType.None;

            if (isNew)
            {
                AssetDatabase.CreateAsset(tile, tilePath);
            }
            else
            {
                EditorUtility.SetDirty(tile);
            }

            result.Add(tile);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return result
            .OrderBy(t => t.name.StartsWith(GroundPrefix, StringComparison.Ordinal) ? 1 : 0)
            .ThenBy(t => t.name, StringComparer.Ordinal)
            .ToList();
    }

    private static void BuildPalette(IReadOnlyList<TileBase> tiles)
    {
        Directory.CreateDirectory(PaletteDir);

        // レイヤーは既存の PagodaPalette などに合わせて Default(0) にする
        var root = new GameObject("KazeBossPalette");
        try
        {
            var grid = root.AddComponent<Grid>();
            grid.cellSize = new Vector3(1f, 1f, 0f);

            var layer = new GameObject("Layer1");
            layer.transform.SetParent(root.transform, false);
            var tilemap = layer.AddComponent<Tilemap>();
            layer.AddComponent<TilemapRenderer>();

            for (var i = 0; i < tiles.Count; i++)
            {
                var position = new Vector3Int(i % PaletteColumns, -(i / PaletteColumns), 0);
                tilemap.SetTile(position, tiles[i]);
            }

            PrefabUtility.SaveAsPrefabAsset(root, PalettePath);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }

        AssetDatabase.Refresh();
    }
}
