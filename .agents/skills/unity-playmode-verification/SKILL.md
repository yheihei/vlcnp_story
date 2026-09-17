---
name: unity-playmode-verification
description: vlcnpStory6 の変更した挙動を、実行中のシーンで直接観察する必要があるときに使う。
---

# Play Mode 検証

有効な自動テストで結果を確認できる場合は、その検証でよい。画像だけの修正や文書変更に Play Mode を追加しない。

- 変更した挙動を再現できるシーンと対象を選ぶ。シーンを切り替える前に未保存状態を確認する。
- 公式Unity CLIの `editor_play` / `editor_stop`、`find_gameobjects`、`get_component_properties`、`console`、`capture_game_view` を使う。引数は現在の `unity command` の定義で確認する。
- Play Mode中に再コンパイルを伴う検証コードを追加しない。復旧が必要なら [Editor 操作](../unity-editor-automation/SKILL.md) を参照する。
- 時間やフレームの観察には必要に応じて `editor_pause` やEditorのStepボタン を使う。目視が必要な変更では画面を確認し、スクリーンショットの保存は検証や報告に役立つ場合に絞る。
- 検証のために開始した Play Mode は観察後に終了する。採用する調整値は終了前に記録し、終了後に Edit Mode の対象へ反映して保存する。Play Mode 中の変更は自動保存されない。
- Console の新規 Error / Exception と、対象シーン・Prefab の保存状態を確認する。他の作業の変更まで解消しない。

観測した結果と重要な未検証事項を報告する。共有コンポーネントなど影響が広い変更でなければ、代表シーンを巡回する回帰確認は追加しない。
