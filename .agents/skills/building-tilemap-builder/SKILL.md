---
name: building-tilemap-builder
description: vlcnpStory2022 の建物・室内タイルセットを作成する、または壁の継ぎ目・建具素材を修正するときに使う。
---

# 建物タイルマップ素材

依頼された素材や設定に範囲を絞る。建物一式の依頼では壁・足場・建具を揃え、単一の継ぎ目修正で新規セットやシーンを追加しない。

- 新しい絵が必要なときは [スプライト生成](../generate-2d-sprite/SKILL.md) を使う。
- 壁の分割、端、色合わせ、ひび割れの処理は [建物素材の調整](references/building-materials.md) を読む。
- Tile・Palette・Prefab の作成と取り込みは [Unity アセット化](references/unity-assets.md) を読む。ステージ素材にも共通する取り込み手順をここにまとめている。

このプロジェクトは 32×32 px タイル、PPU 32。背景壁はコライダーなし、床・屋根・足場は Sprite コライダー。扉・窓・柱は SpriteRenderer Prefab を基本とし、動作や当たり判定は依頼で必要なものだけ付ける。

変更した画像は寸法と接続・重ね合わせを目視確認する。Unity へ組み込む場合は Import と対象参照・コライダー・保存を確認する。C# を変えていなければ Compile は不要。
