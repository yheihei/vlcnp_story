---
name: generate-2d-sprite
description: Generate 2D pixel-art sprite PNGs for vlcnpStory2022 by delegating image generation to the Codex CLI. Use whenever a task needs a new or revised sprite/tile/prop image (enemy sprites, tilemap tiles, props, effects) because Claude cannot generate images itself. Covers the codex exec command template, prompt requirements (exact dimensions, transparent background, output path), and post-generation verification.
---

# 2Dスプライト画像生成(Codex委譲)

## 基本方針

Claude は画像を生成できない。スプライト・タイル・プロップなどの PNG が必要になったら、自分で描こうとせず **Codex CLI に生成を委譲する**。編集(切り出し・リサイズ・色調整・透過処理)は従来どおり Pillow で自分で行ってよい。新規の絵柄を作る場合だけ Codex を使う。

## コマンドテンプレート

```bash
codex exec --skip-git-repo-check -s workspace-write -c model_reasoning_effort=low \
  "<プロンプト>"
```

- reasoning effort は **low** を使う(コスト抑制。low でも 32x32 ピクセルアート生成は十分実用)。
- Codex は Claude Code のサンドボックス内では起動できず SIGKILL(exit 137)になる。**サンドボックス無しで実行する**こと。
- 生成には1枚あたり数分かかる。タイムアウトは 5〜10 分に設定する。
- 関連するスプライトが複数枚あるときは、1回の codex 呼び出しにまとめて依頼する(呼び出し回数を減らす)。

## プロンプトの書き方

プロンプトには必ず次を含める。

1. **正確な寸法**(例: 32x32、64x96)。最終ファイルが指定寸法ぴったりであることを要求する。
2. **画風**: ピクセルアート、このプロジェクトの既存スプライトの雰囲気。参考にすべき既存 PNG があれば絶対パスで伝える(codex exec は `-i <FILE>` で画像を添付できる)。
3. **背景透過**(RGBA、背景 alpha=0)。
4. **出力先の絶対パス**とファイル名。ファイル名は既存規約どおり寸法を含める(例: `slime_green_01_32x32.png`)。
5. 完了時に保存パスを報告するよう指示する。

例:

```bash
codex exec --skip-git-repo-check -s workspace-write -c model_reasoning_effort=low \
  "32x32のピクセルアートで緑のスライムの敵スプライトを作成し、背景透過のRGBA PNGとして /path/to/output/slime_green_01_32x32.png に保存して。最終画像は正確に32x32にすること。完了したら保存パスを報告して。"
```

## 生成後の検証(必須)

Codex の報告を鵜呑みにせず、呼び出し側で必ず確認する。

1. ファイルが指定パスに存在すること。
2. 寸法が指定どおりであること(`sips -g pixelWidth -g pixelHeight <file>` か Pillow)。
3. Read ツールで画像を実際に見て、絵柄・透過が意図どおりか確認する。
4. 問題があれば、修正内容を具体的に伝えて codex に再依頼するか、軽微なら Pillow で自分で修正する。

検証が通ってから、呼び出し元スキル(`building-tilemap-builder`、`create-enemy-character` など)の手順に戻り、`Assets/` への配置と `unicli exec AssetDatabase.Import` を行う。

## やってはいけないこと

- Claude 自身で Pillow 等を使ってゼロから絵柄を描き起こす(単色図形の仮素材を明示的に頼まれた場合を除く)。
- effort を high にする(明示的に頼まれた場合を除く)。
- 生成 PNG を検証せずに `Assets/` へ配置する。
