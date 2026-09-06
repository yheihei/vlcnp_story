# VisibilityFlagManagerV2

既存の VisibilityFlagManager の設定を維持するため、複数フラグのAND／OR指定は別コンポーネントとして提供する。既存コンポーネントの自動置換は行わない。

Visibility Flags の各要素に先頭の Flag を選び、Additional Flags に「AND／ORと次のFlag」を追加する。ANDをORより先に評価するため、`A OR B AND C` は `A OR (B AND C)` になる。Inspectorの条件式で評価順を確認できる。

条件がtrueの要素のうち、リストの最後にある要素の Is Visible を適用する。falseの要素は飛ばし、全てfalseなら現在の状態を維持する。対象と直下の子のアクティブ状態を変更する。

Noneは常にtrue。通常時の表示状態を指定したい場合は、先頭要素を Flag = None にして、後ろに個別条件を追加する。追加フラグが空の場合は先頭のFlagだけで判定し、Visibility Flags 自体が空なら何もしない。

例として、先頭要素を None / Is Visible = true、次の要素を `A OR B OR C` / Is Visible = false にすると、どれかがONなら非表示、全てOFFなら表示になる。

初回評価はStart時に行うため、対象は最初にアクティブな状態で開始する。自分自身を非表示にした後もフラグ通知を受け取り、再表示できる。旧版とV2を同じ対象に併用すると互いに表示設定を上書きするため、対象ごとにどちらか一方を使用する。
