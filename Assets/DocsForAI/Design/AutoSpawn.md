# AutoSpawn Design

## Component role
`AutoSpawn` periodically spawns a prefab around the player within a distance range, and now limits the number of active spawned instances.

## Changes
- Added a serialized `maxSpawnCount` (default 3).
- Tracks spawned instances in `spawnedObjects`.
- Prevents spawning when the tracked count reaches the max.

## Unity Editor steps
- Select the GameObject with `AutoSpawn`.
- Set `Max Spawn Count` if you want a value other than 3.
- Ensure `Spawn Object` is assigned as before.
