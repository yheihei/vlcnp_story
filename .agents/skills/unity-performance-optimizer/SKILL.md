---
name: unity-performance-optimizer
description: "Unity projects performance diagnosis and optimization guidance for gameplay code, UI, assets, physics, memory/GC, WebGL builds, and profiling. Use when asked to make a Unity game lighter or faster, reduce frame drops, improve FPS, remove GC allocations, optimize Update/FixedUpdate hot paths, improve uGUI/TMP/Animator/Input/physics usage, replace synchronous loading, investigate WebGL performance, or review Unity C# code for performance risks."
---

# Unity Performance Optimizer

## Workflow

Optimize only after building a small evidence loop:

1. Identify the target: platform, scene, symptom, target FPS, reproduction path, and whether the issue is CPU, GPU, memory, GC, loading, or input latency.
2. Measure first when possible: Unity Profiler, Memory Profiler, Frame Debugger, Physics Profiler, build logs, browser devtools for WebGL, or a focused static scan.
3. Make the smallest high-confidence change that targets the measured bottleneck.
4. Re-run the same measurement and report before/after evidence. If measurement is unavailable, state the assumption and residual risk.

For Unity C# edits, also follow the `unity-development` skill: import changed assets and compile after script changes.

## Static Scan

Use `scripts/scan_unity_perf.py` for a first pass before manual edits:

```bash
python3 .agents/skills/unity-performance-optimizer/scripts/scan_unity_perf.py .
```

Treat findings as triage hints, not proof. Prioritize hits inside `Update`, `FixedUpdate`, `LateUpdate`, `OnGUI`, `OnTriggerStay`, frequently fired callbacks, and empty Unity event methods that can be deleted.

## Priority Rules

Prefer changes in this order:

1. Proven frame-time or allocation regressions from Profiler/Memory Profiler.
2. Hot-path C# allocations and scene-wide searches.
3. Synchronous loading or repeated instantiate/destroy bursts.
4. UI rebuild/raycast waste and text update churn.
5. Physics timestep, collision, and Rigidbody/Rigidbody2D work proven expensive in Profiler.
6. Render batching/material/culling issues found with Frame Debugger.
7. Architectural cleanup only when it directly removes repeated work or enables measurement/testing.

Do not replace clear code with obscure micro-optimizations. Keep gameplay behavior identical unless the user explicitly asks for design changes.

## Common Fixes

Load `references/unity-performance-checklist.md` when you need specific patterns. It covers:

- `GetComponent`, `transform`, `Camera.main`, `GameObject.Find`, `FindObjectOfType`, and Rigidbody/Rigidbody2D reference caching.
- Reducing work in `Update` with events, dirty flags, timers, and pure C# logic classes.
- Removing empty Unity event methods such as unused `Start`, `Update`, `FixedUpdate`, and `LateUpdate`.
- Avoiding GC from LINQ, per-frame `new`, string churn, logging, and object creation/destruction.
- Data structure choices for repeated lookup.
- Distance comparisons, Animator hashes, `CompareTag`, Input System migration.
- uGUI Canvas splitting, GraphicRaycaster removal, Raycast Target cleanup, TMP migration.
- `MaterialPropertyBlock`, culling/LOD checks, Addressables, async IO, parallel independent loads.
- Profiling loop and WebGL-specific verification notes.

## Reporting

When finishing an optimization task, report:

- what was measured or scanned;
- files changed;
- the bottleneck class addressed;
- how to verify in Unity;
- any remaining risks or measurements still needed.
