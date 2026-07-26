---
name: generate-2d-sprite
description: Generate 2D pixel-art sprite PNGs for vlcnpStory2022. Use whenever a task needs a new or revised sprite/tile/prop image (enemy sprites, tilemap tiles, props, effects). Agents that cannot generate images themselves (such as Claude) delegate to the Codex CLI; Codex generates directly. Covers the codex exec delegation template, prompt requirements (exact dimensions, transparent background, output path), and mandatory post-generation verification.
---

# 2Dスプライト画像生成

## 基本方針

スプライト・タイル・プロップなどの新規 PNG が必要になったら、実行中のエージェントによって手段が変わる。

- **Codex 自身が実行している場合**: 委譲せず自分で画像を生成する。「プロンプトの書き方」の各要件(正確な寸法・背景透過・出力先パス)は、そのまま自分が満たすべき要件として扱うこと。
- **Claude など画像を生成できないエージェントの場合**: 自分で絵柄を描き起こそうとせず、**Codex CLI に生成を委譲する**(下記テンプレート)。

編集(切り出し・リサイズ・色調整・透過処理)はどちらの場合も Pillow で自分で行ってよい。生成が必要なのは新規の絵柄を作る場合だけ。

## Codex への委譲(Claude 等の場合)

```bash
codex exec --skip-git-repo-check -s workspace-write -c model_reasoning_effort=low \
  "<プロンプト>"
```

- reasoning effort は **low** を使う(コスト抑制。low でも 32x32 ピクセルアート生成は十分実用)。
- Codex は Claude Code のサンドボックス内では起動できず SIGKILL(exit 137)になる。Claude Code から呼ぶときは Bash ツールに **`dangerouslyDisableSandbox: true`** を指定してサンドボックス無しで実行すること。
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

自分で生成した場合も委譲した場合も、生成できた気になって次へ進まず必ず確認する(委譲したときは Codex の報告を鵜呑みにしない)。

1. ファイルが指定パスに存在すること。
2. 寸法が指定どおりであること(`sips -g pixelWidth -g pixelHeight <file>` か Pillow)。
3. Read ツールで画像を実際に見て、絵柄・透過が意図どおりか確認する。
4. 問題があれば、修正内容を具体的にして生成をやり直す(委譲している場合は codex に再依頼)。軽微なら Pillow で自分で修正する。

検証が通ってから、呼び出し元スキル(`building-tilemap-builder`、`create-enemy-character` など)の手順に戻り、`Assets/` への配置と `unicli exec AssetDatabase.Import` を行う。

## やってはいけないこと

- 画像生成できないエージェントが、Pillow 等を使ってゼロから絵柄を描き起こす(単色図形の仮素材を明示的に頼まれた場合を除く)。
- 委譲時に effort を high にする(明示的に頼まれた場合を除く)。
- 生成 PNG を検証せずに `Assets/` へ配置する。
