# AutoSpawn Plan

## Goal
Add a max spawn count to `AutoSpawn` so it stops spawning when the limit is reached, with a default of 3 and no impact to existing scenes.

## Files
- Assets/Scripts/Core/AutoSpawn.cs
- Assets/DocsForAI/Design/AutoSpawn.md

## Implementation
- Add `maxSpawnCount` as a serialized field with default 3.
- Track spawned instances in `spawnedObjects`.
- Add `CanSpawnCount()` to prune nulls and enforce the limit.
- Check `CanSpawnCount()` before spawning and only reset the timer when a spawn occurs.

## Acceptance
- AutoSpawn never exceeds the configured max count.
- Destroyed spawns free up slots.
- Default behavior remains unchanged except for the new max limit.
