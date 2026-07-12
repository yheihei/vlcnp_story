---
name: building-tilemap-builder
description: Create and revise Unity 2D building/interior tilemap assets from generated PNGs. Use when generating or postprocessing building-stage sprites, splitting 32 px wall/floor/roof tiles, creating Unity Tile assets with correct collider settings, building Tile Palette prefabs, creating SpriteRenderer prefabs for non-tile props such as doors/windows/pillars/cracks/holes/entrances, or fixing tile seam/color issues in Assets/Game/MapObject building tilemap folders.
---

# Building Tilemap Builder

## Core Rules

Use this skill with `unity-development` when working inside the Unity project. After creating or modifying anything under `Assets/`, run:

```bash
unicli exec AssetDatabase.Import --path "<asset-path>" --json
```

Keep production outputs under a map-specific folder. For example, the pagoda second-floor set used:

- Source/generated sprites: `Assets/Game/MapObject/PagodaSecondFloorGenerated`
- Tile sprites and `.asset` tiles: `Assets/Game/MapObject/PagodaTilemap/Tiles`
- Tile palettes: `Assets/Game/MapObject/PagodaTilemap/Palette`
- Non-tile prefabs: `Assets/Game/MapObject/PagodaTilemap/Prefabs`

For a new building set, choose equivalent names such as:

- `Assets/Game/MapObject/<BuildingName>Generated`
- `Assets/Game/MapObject/<BuildingName>Tilemap/Tiles`
- `Assets/Game/MapObject/<BuildingName>Tilemap/Palette`
- `Assets/Game/MapObject/<BuildingName>Tilemap/Prefabs`

Use 32 pixels per unit. Import sprite textures as Sprite, Single, PPU 32, Point filtering, mipmaps off, uncompressed, alpha transparency on.

Do not hand-create `.meta` files. Let Unity generate them through import.

## Asset Plan

For a building tilemap set, separate assets by intended use:

- `clean_wall_*`, `wall_*`, or equivalent: background tiles, no collider.
- `roof_platform_*`, `floor_*`, `ground_*`, or equivalent: ground/platform tiles, collider enabled.
- `crack_overlay_*`: damage/wall variant sprites or prefabs. Prefer matching the wall color over transparency if the user wants natural cracks.
- `hole_overlay_*`: damage/hole wall variant sprites or prefabs. Prefer matching the wall color over transparency if the user wants surrounding cracks preserved.
- Doors, pillars, windows, entrances: SpriteRenderer prefabs, not Tile assets unless the user explicitly asks.
- Preview/source/contact sheets: keep as working artifacts; do not make Tile assets or Prefabs from them.

Default building dimensions:

- Tile size: 32x32 px.
- Wall height: 3 tiles, usually 32x96 px.
- Wide overlays/doors/entrances: 64 px wide when spanning 2 tiles.
- Floors, roofs, or ledges: usually 1 tile high and used as the floor/collider layer.

## Image Generation And Extraction

When generating new building assets, ask for a contact sheet only if it helps review; extract final individual PNGs with deterministic pixel crops. Name files with dimensions:

```text
clean_wall_center_01_32x96.png
clean_wall_left_edge_32x96.png
roof_platform_01_32x32.png
floor_01_32x32.png
crack_overlay_01_32x32.png
hole_overlay_wide_01_64x32.png
double_door_64x96.png
```

For wall sets:

- Left edge wall: remove top, bottom, and right wood frame; keep the left outer frame.
- Right edge wall: remove top, bottom, and left wood frame; keep the right outer frame.
- Center wall: remove top, bottom, left, and right wood frame.
- Ensure center wall variations tile horizontally without black side lines or hue jumps.

When postprocessing generated sprites, prefer structured image processing via Pillow over manual pixel edits. Preserve pixel-art sharpness and keep final files at exact intended dimensions.

## Tile Creation

Create 32x32 tile PNGs in the map's `Tiles` folder:

- Copy every 32x32 background wall sprite to `bg_<source>.png`.
- Split every 32x96 background wall sprite into:
  - `bg_<source>_top.png`
  - `bg_<source>_middle.png`
  - `bg_<source>_bottom.png`
- Copy every floor/roof/platform sprite to `ground_<source>.png` or another local ground prefix.

Create one `UnityEngine.Tilemaps.Tile` asset per tile PNG:

- Tile asset name: `<tile_png_stem>_tile.asset`
- Sprite: the matching PNG sprite.
- Collider:
  - ground/floor/roof/platform tiles: `Tile.ColliderType.Sprite`
  - background wall tiles: `Tile.ColliderType.None`

Validate:

- Every tile PNG is exactly 32x32.
- Tile asset count matches tile PNG count.
- Only ground roof tiles have colliders.

## Palette Creation

Create palette prefabs under the map's `Palette` folder.

Follow the existing Unity palette structure used by project palettes such as `TP Dungeon Ground`:

- Root GameObject with `Grid`, layer 31 if that is the local palette convention.
- Child `Layer1` with `Tilemap` and `TilemapRenderer`.
- Place background wall tiles first, then ground/floor/roof tiles.
- Use compact rows such as 16 columns.

Save with `PrefabUtility.SaveAsPrefabAsset`. Import the `Palette` folder after saving.

## Prefab Creation

For generated PNGs not used as Tile assets, create SpriteRenderer prefabs under the map's `Prefabs` folder.

Include:

- `crack_overlay_*`
- `hole_overlay_*`
- `hole_overlay_wide_*`
- `single_door_*`
- `double_door_*`
- `wooden_pillar_*`
- `barred_window_*`
- `open_entrance_interior_*`
- `boarded_opening_*`

Exclude:

- `clean_wall_*`
- floor/roof/platform sprites already converted into Tile assets
- `preview_*`
- `source_*`

Prefab GameObject name should match the PNG stem. Add a `SpriteRenderer` with the source sprite. Do not add colliders unless the user asks for interaction.

## Seam And Tone Fixes

Use screenshots and quick composite previews to diagnose seam problems.

Common fixes:

- Black line on center wall left edge: replace the first 1-2 columns with adjacent wall plaster color.
- Black line on left edge wall right side: replace the rightmost 1-2 columns with wall plaster color.
- Black line on right edge wall left side: replace the leftmost 1-2 columns with wall plaster color.
- Bottom shadow on wall bottom tiles: remove or lighten the lower edge so adjacent tiles connect.
- Hue mismatch between wall variants: pull plaster pixels toward the average current center-wall color.

For damage overlays, decide based on the user's intent:

- If the user wants an overlay that can sit on any wall: transparentize only the clean wall backing, but avoid deleting crack fragments.
- If transparency makes cracks unnatural or removes detail: restore the asset and color-match the wall backing to the current wall tile instead.
- For generated building damage that includes surrounding plaster, wood, stone, or wallpaper cracks, wall-color/material matching is usually better than transparency.

Never remove crack/hole detail just to eliminate a rectangular patch. If a patch border remains, recolor the border to the current wall color unless the user explicitly wants transparent overlays.

## Verification

Before finishing:

- Import modified `Assets/` paths with UniCli.
- Confirm PNG dimensions for all new tiles/prefabs.
- Confirm prefab count and that each prefab has a `SpriteRenderer` with a non-null sprite.
- For visual edits, create a temporary composite preview over a representative wall tile and inspect it.
- Do not run Unity compile unless C# changed.
