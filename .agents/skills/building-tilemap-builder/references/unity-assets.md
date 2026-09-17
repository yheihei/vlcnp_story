# Unity アセット化

建物・ステージの素材を Unity へ組み込むときに使う。PNG だけの納品ではこの手順は不要。

## 配置とインポート

既存セットの修正では現在の配置と GUID を保つ。新規セットの標準配置は次のとおり。

| 内容 | パス |
| --- | --- |
| 再切り出し用の原画 | `Assets/Game/MapObject/<Name>Generated/` |
| 32×32 PNG と Tile asset | `Assets/Game/MapObject/<Name>Tilemap/Tiles/` |
| Palette prefab | `Assets/Game/MapObject/<Name>Tilemap/Palette/` |
| Prop PNG と Prefab | `Assets/Game/MapObject/<Name>Tilemap/Prefabs/` |

作業用プレビューを製品アセットに混ぜない。原画は必要な解像度だけを置き、容量削減のために唯一の原画を上書きしない。

TextureImporter は Sprite / Single、PPU 32、Point、mipmap off、非圧縮、alphaIsTransparency、pivot 中央が標準。既存素材に意図した別設定があれば用途を確認する。`.meta` は Unity に生成させる。

[Editor 操作](../../unity-editor-automation/SKILL.md) に従って対象を Import する。フォルダ指定だけでは内部の新規ファイルが Import されないことがある。各ファイルの importer・sprite が存在するか確認し、欠けている場合は個別 Import または Edit Mode で再帰 Import を行う。

```csharp
UnityEditor.AssetDatabase.ImportAsset(
    "Assets/Game/MapObject/<Name>Tilemap",
    UnityEditor.ImportAssetOptions.ImportRecursive);
UnityEditor.AssetDatabase.Refresh();
```

## Tile・Palette・Prop

- Tile PNG ごとに `UnityEngine.Tilemaps.Tile` を作り、対応 Sprite を参照する。名前は `<png_stem>_tile.asset`。`bg_` は `ColliderType.None`、`ground_` は `ColliderType.Sprite`。
- Palette は root に Grid、子 `Layer1` に Tilemap と TilemapRenderer。レイヤーは 0。例は `Assets/Game/MapObject/PagodaTilemap/Palette/PagodaPalette.prefab` と `Assets/Game/MapObject/KazeBossTilemap/Palette/KazeBossPalette.prefab`。第三者の layer 31 をコピーしない。列数は素材数に合わせる。
- 扉・窓・柱・ひび割れなどの非タイル素材は SpriteRenderer Prefab とし、null でない Sprite を設定する。コライダーは動作・接触の要件がある場合だけ付ける。

既存の `Assets/Scripts/Editor/KazeBossTilesetBuilder.cs` / `KazeBossPropsBuilder.cs` は実装例として必要部分だけ参照する。コピーと定数置換を必須にしない。自動化は差分更新にし、手動調整や既存参照を作り直さない。一時ビルダーの後片付けは Editor 操作の手順に従う。

メニュー経由なら公式CLIの `menu --path "<メニューパス>"` を使う。実際の定義は `unity command` で確認する。成功応答だけでなく Console の新規例外と生成物も確認する。

## Palette が認識されない場合

Tile Palette の一覧に出るかを確認する。出ない場合は `GridPalette` の `Palette Settings` sub-asset と既存 palette の保存構造を調べ、自動付与を前提にしない。

sub-asset が存在してキャッシュだけが古い場合は `UnityEditor.Tilemaps.GridPalettes` の `CleanCache()` を検討する。型は `Unity.2D.Tilemap.Editor` にあり、`typeof(UnityEditor.Editor).Assembly` だけでは見つからない。内部 API が必要なら実際の型・メンバーを確認してから使う。

## 完了確認

変更した対象で、PNG 寸法、Tile と Sprite の対応、背景・地面の collider、Prefab の Renderer と Sprite、Palette の登録・配置を確認する。ファイル数だけを成功条件にしない。C# を追加・変更・削除した場合は最終 Compile を行い、対象 Prefab・Scene・asset の保存を確認する。
