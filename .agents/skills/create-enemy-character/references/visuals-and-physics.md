# 向き・アニメーション・物理

## 左向き素材

`VLCNP.Combat.EnemyAction.LookAtPlayer` と Bee は左向きの元絵を前提とする。

- Player が左なら `localScale.x = +Abs(localScale.x)`。
- Player が右なら `localScale.x = -Abs(localScale.x)`。
- `SpriteRenderer.flipX` を混用しない。右向き素材は全 Frame を左向きへ揃え、共有コードで敵ごとの例外補正をしない。
- 同名の別 namespace の `LookAtPlayer` と取り違えない。

進行方向へ傾ける場合は X scale と回転の基準を合わせる。

```csharp
bool isFacingLeft = direction.x < 0f;
Vector3 localScale = transform.localScale;
localScale.x = isFacingLeft
    ? Mathf.Abs(localScale.x)
    : -Mathf.Abs(localScale.x);
transform.localScale = localScale;

Vector2 orientationDirection = isFacingLeft ? -direction : direction;
float angle = Mathf.Atan2(orientationDirection.y, orientationDirection.x)
    * Mathf.Rad2Deg;
transform.rotation = Quaternion.Euler(0f, 0f, angle);
```

突き刺さりの演出なら衝突時の角度を保って停止し、復帰 Action で identity へ戻す。左右の斜め移動で腹や背が逆転しないか確認する。

## Animation

待機・Hover はループ、単発の攻撃は非ループを基本にする。Action から直接再生する場合は設定した state を `animator.Play(stateName, 0, 0f)` で先頭から再生する。RangeDetect の未発見演出には同機構の bool と Animation Event を使う。

Sprite key の sub-asset 参照、順序、最後の Frame、Clip 長と Action の完了条件を確認する。見た目だけで終了時刻を推測しない。

## Rigidbody2D と Collider2D

空中敵は Gravity Scale 0、高速突撃では Continuous を検討する。コードで姿勢を制御する場合は意図しない物理回転を防ぐ。Interpolate は見た目に必要なら使う。

継承元の余分な Collider や攻撃用の子が判定を二重にしていないか確認する。当たり判定を変えた場合は、絵の寸法だけでなく左右の姿勢、Ground 接触、Player や通路との関係で検証する。
