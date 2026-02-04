# AppearEnemyAction Design

## Component role
`AppearEnemyAction` hides the enemy on start by disabling its hurtbox collider and renderer, then re-enables them on Execute and completes when the appear animation event fires.

## Changes
- Added `AppearEnemyAction` under `Assets/Scripts/Combat/EnemyAction/`.
- Uses `IsAppear` Animator bool to trigger appear animation.
- Toggles `Collider2D` (hurtbox) and `Renderer` (visuals).

## Unity Editor steps
- Add `AppearEnemyAction` to the enemy GameObject.
- Assign the hurtbox `Collider2D` to `Hurtbox Collider`.
- Ensure the same GameObject has an `Animator` with a bool parameter named `IsAppear`.
- Add an Animation Event at the end of the appear animation that calls `OnAppearAnimationFinished`.
