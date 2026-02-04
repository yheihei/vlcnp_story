# AppearEnemyAction Plan

## Goal
Add an EnemyAction that hides the enemy at start by disabling a specified hurtbox collider and renderer, then shows/enables them on Execute and completes on an animation event.

## Files
- Assets/Scripts/Combat/EnemyAction/AppearEnemyAction.cs
- Assets/DocsForAI/Design/AppearEnemyAction.md

## Implementation
- Create `AppearEnemyAction` inheriting `EnemyAction`.
- Fields (SerializeField):
  - Animator animator (self; auto-fetch if null)
  - Collider2D hurtboxCollider (explicit; fallback to GetComponent if null)
  - Renderer visualRenderer (self; auto-fetch if null)
  - string appearBoolParam = "IsAppear"
- Awake:
  - Cache refs; warn if any missing.
- Start:
  - SetVisible(false)
  - SetHurtboxEnabled(false)
- Execute:
  - Guard IsExecuting/IsDone.
  - Enable renderer + hurtbox.
  - If animator null -> complete immediately.
  - animator.SetBool(appearBoolParam, true)
- Animation Event:
  - public void OnAppearAnimationFinished()
  - animator.SetBool(appearBoolParam, false)
  - IsDone = true; IsExecuting = false

## Acceptance
- Enemy starts hidden and cannot be damaged.
- On Execute: becomes visible, hurtbox enabled, IsAppear set true.
- Animation event marks action done.
