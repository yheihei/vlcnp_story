# PRD

## 目的

- @Assets/Scripts/Core/AutoSpawn.cs に最大スポーン数をもうけたい
- 最大スポーン数を超えたらスポーンしないようにしたい
- 最大スポーン数のデフォルトは3
- 現状使っている箇所に影響が出ないようにしたい

## 詳細

@Assets/Scripts/Combat/EnemyAction/AutoSpawnAction.cs の maxSpawnCount と spawnedObjectsが参考になる
