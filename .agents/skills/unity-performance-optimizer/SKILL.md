---
name: unity-performance-optimizer
description: vlcnpStory2022 の FPS低下・GC・ロード遅延を診断する、または性能レビューを依頼されたときに使う。
---

# Unity 性能診断

対象は Windows / macOS Standalone。症状と再現経路を依頼や既存の計測から絞り、CPU・GPU・GC・メモリ・ロードのどこが支配的かを判断する。

計測できる場合は同じ場面のフレーム時間、割り当て量、ロード時間などを変更前後で比較する。Profiler がなくても静的レビューや原因の明らかな修正は進め、実測と推定を区別する。既知のボトルネックがあるなら全面スキャンは不要。

## 調査の入口

- コード全体の性能レビューや調査対象が未特定の場合は [静的スキャナー](scripts/scan_unity_perf.py) を使える。
- 特定の問題への対策を選ぶときだけ [対策の判断材料](references/unity-performance-checklist.md) の該当項目を読む。

```bash
python3 .agents/skills/unity-performance-optimizer/scripts/scan_unity_perf.py .
```

スキャン結果は候補であり、性能劣化の証明ではない。実行頻度と実測の寄与を優先し、未使用の Update や低頻度の LINQ を一律に修正しない。

挙動、シリアライズ、Prefab 互換性を保つ。Input System・Addressables・TMP への移行や物理設定変更は、性能修正の定型作業にしない。C# を編集する場合は [Editor 操作](../unity-editor-automation/SKILL.md) の Import / Compile を行う。

結果は変更点とその根拠、変更前後の測定値があればその比較、未計測の範囲を伝える。
