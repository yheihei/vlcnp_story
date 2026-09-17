---
name: unity-project-conventions
description: vlcnpStory6 の C#・Prefab・シーンを変更するときに、配置・名前空間・既存のマップ構成を確認する。
---

# プロジェクト規約

## 配置とコード

| 種類 | 場所 |
| --- | --- |
| ゲームロジック | `Assets/Scripts/<カテゴリ>/` |
| エディタ拡張 | `Assets/Scripts/Editor/` |
| Prefab・マップ素材 | `Assets/Game/` |
| Config の ScriptableObject | `Assets/Game/Resources/` |
| シーン | `Assets/Scenes/` |

`Assets/ExtraPackage/`、`Assets/Fungus/` などのサードパーティ資産を実装対象にしない。作業範囲は AGENTS.md と依頼に従う。

- namespace はフォルダに対応する `VLCNP.<カテゴリ>`。既存クラスはその周辺の規約に合わせる。
- Inspector に出すフィールドは private と `[SerializeField]` を使う。既存の公開 API や保存済みフィールドを規約合わせだけで変更しない。
- クラス説明は日本語の `/** ... */`。
- タグ・レイヤーの実名は `ProjectSettings/TagManager.asset` で確認する。まず既存値を使う。

## マップと演出

既存の標準構成は `Grid` 配下の次の2枚。個別シーンの変更で全シーンをこの形へ揃え直さない。

| Tilemap | 用途 | 設定 |
| --- | --- | --- |
| `Tilemap` | 足場・屋根・ブロック | tag=`Ground`、`TilemapCollider2D`、sorting order 100 |
| `BGTilemap` | 背景の壁 | コライダーなし、sorting order -1 |

カメラ範囲は `CameraConfineArea`、落下ミスは `FallMissZone` と `Health.Kill` を利用する。配置例は `Assets/Scenes/Kaze1.unity`。会話・イベント演出は既存の Fungus 構成を使う。
