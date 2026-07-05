---
name: unity-project-conventions
description: Code, asset, and scene conventions for vlcnpStory2022. Use when creating or editing C# scripts, prefabs, ScriptableObject configs, scenes, or tilemaps — namespaces, file placement, tags, sorting layers, and which directories are off-limits.
---

# プロジェクト規約

## ファイル配置

| 種類 | 場所 |
| ---- | ---- |
| C# ゲームロジック | `Assets/Scripts/<カテゴリ>/`(Actions, Attributes, Combat, Control, Core, Effects, Movement, Pickups, Projectiles, Saving, SceneManagement, Stats, Steam, UI など) |
| エディタ拡張 | `Assets/Scripts/Editor/` |
| プレハブ・マップ素材 | `Assets/Game/` |
| 武器などの Config(ScriptableObject) | `Assets/Game/Resources/`(例: `ArrowConfig.asset`, `VeryLongGunConfig.asset`) |
| シーン | `Assets/Scenes/` |

`Assets/ExtraPackage/`(Thirdweb 等)や `Assets/Fungus/` などのサードパーティ資産は**変更しない**。上記以外のディレクトリは原則実装対象外(AGENTS.md / CLAUDE.md 準拠)。

## C# コーディング規約

- namespace は `VLCNP.<カテゴリ>`。フォルダと対応させる(例: `Assets/Scripts/Combat/` → `VLCNP.Combat`)。
- フィールドは private + `[SerializeField]` で Inspector に出す。
- クラスの説明コメントは日本語の `/** ... */`。
- 新しい仕組みを書く前に、既存の類似コンポーネントを検索して流用・踏襲する(例: `Health`, `Flag`, `FallMissZone`, `CameraConfineArea`)。

## タグ・レイヤー

- 主要タグ: `Enemy`, `Weapon`, `Item`, `Ground`, `Water`, `Projectile`, `CMCamera`, `FlagManager` など(全量は `ProjectSettings/TagManager.asset`)。
- 新規タグ・レイヤーの追加は最終手段。まず既存を使う。
- レイヤー: `Default`, `Player`, `Water` など。

## シーン・タイルマップ構成

- `Grid` 配下に 2 枚:
  - `Tilemap` — tag=`Ground`、`TilemapCollider2D` あり、sorting order 100。足場・屋根・ブロックなど衝突するもの。
  - `BGTilemap` — コライダーなし、sorting order -1。背景の壁など非衝突のもの。
- カメラ追従範囲の制限は `CameraConfineArea`、落下ミスは `FallMissZone` + `Health.Kill`(実装例: `Assets/Scenes/Kaze1.unity`)。

## その他

- 会話・イベント演出は Fungus を使う。
- Unity 2022.3 / ビルドターゲットは WebGL。WebGL で動かない API(スレッド、`System.IO` の一部など)に注意する。
