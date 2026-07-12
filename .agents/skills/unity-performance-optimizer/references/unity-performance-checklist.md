# Unity Performance Checklist

This guide is adapted from practical Unity lightweighting patterns, including the Zenn article "軽量化手法24選〖Unity〗" by neriame, published 2026-05-22: https://zenn.dev/neriame_code/articles/2875f05bef3983 and the Zenn article "Unityにおける最適化、高速化まとめ" by miko, published 2026-05-17: https://zenn.dev/miko_gamedev/articles/30d3794d701c21

Use it as a decision checklist. Confirm important changes with Profiler, Memory Profiler, or Frame Debugger.

## Hot Path API Usage

- Cache `GetComponent<T>()`, `GetComponentInChildren<T>()`, `Camera.main`, frequently used `Transform` references, and Rigidbody/Rigidbody2D references in `Awake`, `Start`, or serialized fields.
- Avoid `GameObject.Find`, `FindObjectOfType`, and broad scene searches during gameplay. Prefer serialized references, dependency injection, setup-time lookup, or registries.
- Keep `Update`, `FixedUpdate`, and `LateUpdate` thin. Move state-independent calculations into pure C# classes and trigger recalculation through events, dirty flags, or timers.
- Delete empty Unity event methods such as unused `Start`, `Update`, `FixedUpdate`, and `LateUpdate`. They are small individually but become noise and dispatch overhead across many MonoBehaviours.
- Use `CompareTag` instead of `gameObject.tag == "..."`, especially in repeated collision or update paths.

## Memory and GC

- Pool bullets, hit effects, pickups, damage numbers, and short-lived UI objects. Unity 2022.3 LTS includes `UnityEngine.Pool`.
- Avoid LINQ on gameplay hot paths. Replace with simple loops when the path runs every frame or for many objects.
- Avoid per-frame allocation of collections, classes, closures, delegates, arrays, and formatted strings. Reuse fields and clear collections.
- For frequently updated text, update only when the value changes. Use TMP and consider cached `StringBuilder` for complex strings.
- Remove `Debug.Log` from production paths. Prefer a `void` wrapper with `[System.Diagnostics.Conditional("UNITY_EDITOR")]` and `[System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]` so argument evaluation is compiled out.

## Data and Math

- Use `Dictionary` or `HashSet` for repeated lookup, ownership checks, collected item IDs, active entity membership, and visited state.
- Use early returns to skip work before expensive calls or nested logic.
- Use squared distance comparisons for range checks: compare `(a - b).sqrMagnitude` against `range * range` unless the actual distance value is needed.

## Rendering and UI

- Use `MaterialPropertyBlock` for per-object renderer property changes instead of touching `renderer.material` during gameplay.
- Confirm camera/layer culling for off-screen or irrelevant objects. Use `Culling Mask`, renderer enable/disable, chunk activation, or simple visibility systems when the Profiler/Frame Debugger shows avoidable rendering work.
- Consider LOD only when object scale, camera distance, or large scene counts make it worthwhile. For 2D/WebGL projects, prefer simpler culling or activation boundaries before adding LOD complexity.
- Split uGUI Canvas objects by update frequency. Static HUD, dynamic gauges, and rapidly changing text should not force one large Canvas rebuild.
- Remove or disable `GraphicRaycaster` on Canvas objects that do not need pointer interaction.
- Disable `Raycast Target` on decorative or non-interactive `Image`, `Text`, and TMP graphics.
- Prefer TextMeshPro for text. Migrate frequently updated score, timer, and counter text first.
- Use Frame Debugger to confirm draw calls, batching, material instances, and unexpected Canvas rebuild behavior.

## Physics

- In Unity 2022.3 LTS, inspect physics cost with Profiler/Physics Profiler before changing global settings.
- Adjust `Project Settings > Time > Fixed Timestep` only with gameplay verification. Larger values reduce physics update frequency but can change input feel, collision reliability, and 2D action responsiveness.
- Keep `FixedUpdate` limited to physics-facing work. Do non-physics state updates in `Update` or event-driven code when possible.
- Cache Rigidbody/Rigidbody2D and Collider/Collider2D references rather than resolving them repeatedly in physics callbacks.

## Assets and Loading

- Replace heavy `Resources.Load` runtime paths with Addressables or explicit serialized references. `Resources` also increases build size when unused assets remain under Resources folders.
- Run save/load, network, and other IO asynchronously. For Unity async work, use project-standard async patterns such as `Task`, coroutines, or UniTask if already installed.
- Load independent assets in parallel with `Task.WhenAll` or `UniTask.WhenAll`, but keep dependency order explicit when assets depend on one another.
- For WebGL, account for single-threaded constraints, browser memory ceilings, compressed build settings, asset bundle size, and main-thread stalls from synchronous work.

## Animator and Input

- Cache Animator parameter IDs with `Animator.StringToHash` and call `SetBool`, `SetFloat`, `SetTrigger`, etc. with IDs in hot paths.
- Prefer the Input System for new work or large input refactors. For existing Unity 2022.3 LTS projects, avoid casual migration unless input performance or maintainability is part of the requested work.
- If staying on the old Input Manager, centralize string-based input names and keep polling minimal.

## Measurement Loop

1. Reproduce the slow scene or action.
2. Capture baseline frame time, GC alloc per frame, spikes, draw calls, memory snapshot, or loading duration.
3. Isolate the bottleneck category: scripts, rendering, UI, physics, animation, loading, memory, or browser/WebGL.
4. Apply one focused fix.
5. Re-measure the same path and compare.
6. Keep a note of any tradeoff, behavior risk, or editor-only limitation.

## Review Heuristics

- Flag code in hot paths more aggressively than code in initialization.
- Do not optimize every LINQ or string use automatically. Low-frequency editor/setup paths can stay readable.
- Avoid broad architecture rewrites when a cached reference, dirty flag, pool, or Canvas split solves the measured issue.
- Preserve serialized field names and prefab compatibility where possible.
- In this repository, keep implementation work under `Assets/Scripts/`, `Assets/Game/`, and `Assets/Scenes/` unless the user asks otherwise.
