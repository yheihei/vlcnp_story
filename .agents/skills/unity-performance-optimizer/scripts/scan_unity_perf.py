#!/usr/bin/env python3
"""Scan Unity C# scripts for common performance triage findings."""

from __future__ import annotations

import argparse
import json
import re
from dataclasses import asdict, dataclass
from pathlib import Path
from typing import Iterable


HOT_METHODS = {
    "Update",
    "FixedUpdate",
    "LateUpdate",
    "OnGUI",
    "OnTriggerStay",
    "OnTriggerStay2D",
    "OnCollisionStay",
    "OnCollisionStay2D",
}

EMPTY_EVENT_METHODS = HOT_METHODS | {
    "Awake",
    "Start",
    "OnEnable",
    "OnDisable",
}

SKIP_DIRS = {
    ".git",
    "Library",
    "Temp",
    "Obj",
    "Build",
    "Builds",
    "Logs",
    "UserSettings",
    "Packages",
    "ProjectSettings",
}


@dataclass
class Finding:
    severity: str
    file: str
    line: int
    rule: str
    message: str
    snippet: str


METHOD_RE = re.compile(
    r"\b(?:public|private|protected|internal|static|virtual|override|async|sealed|partial|\s)+\s*"
    r"(?:void|IEnumerator|UniTask|Task)\s+"
    r"(?P<name>[A-Za-z_][A-Za-z0-9_]*)\s*\("
)

EVENT_METHOD_RE = re.compile(
    r"\b(?:public|private|protected|internal|static|virtual|override|async|sealed|partial|\s)*"
    r"void\s+"
    r"(?P<name>[A-Za-z_][A-Za-z0-9_]*)\s*\(\s*\)"
)

GLOBAL_RULES = [
    (
        "warn",
        "tag-string-compare",
        re.compile(r"\.tag\s*==|==\s*[^;\n]*\.tag\b"),
        "Use CompareTag for tag checks, especially in repeated collision or update paths.",
    ),
    (
        "warn",
        "renderer-material",
        re.compile(r"\.material\b"),
        "renderer.material can instantiate a material. Prefer sharedMaterial for read-only access or MaterialPropertyBlock for per-object changes.",
    ),
    (
        "info",
        "debug-log",
        re.compile(r"\bDebug\.(Log|LogWarning|LogError)\s*\("),
        "Guard production logging with a Conditional wrapper or compile symbols.",
    ),
]

HOT_RULES = [
    (
        "error",
        "scene-search-in-hot-path",
        re.compile(r"\b(GameObject\.Find|FindObjectOfType|FindObjectsOfType|FindAnyObjectByType|FindFirstObjectByType|FindGameObjectWithTag|FindGameObjectsWithTag)\b"),
        "Avoid scene-wide searches in hot paths. Use serialized references, setup-time lookup, or registries.",
    ),
    (
        "error",
        "getcomponent-in-hot-path",
        re.compile(r"\bGetComponent(?:InChildren|InParent)?\s*<|\bGetComponent(?:InChildren|InParent)?\s*\("),
        "Cache components in Awake/Start or assign them through serialized fields.",
    ),
    (
        "warn",
        "camera-main-in-hot-path",
        re.compile(r"\bCamera\.main\b"),
        "Cache Camera.main outside hot paths because it performs a tag lookup.",
    ),
    (
        "warn",
        "resources-load-in-hot-path",
        re.compile(r"\bResources\.Load"),
        "Avoid synchronous Resources.Load during gameplay. Prefer Addressables, async loading, or preloaded references.",
    ),
    (
        "warn",
        "instantiate-destroy-in-hot-path",
        re.compile(r"\b(Instantiate|Destroy)\s*\("),
        "Pool short-lived gameplay objects instead of repeated Instantiate/Destroy bursts.",
    ),
    (
        "warn",
        "linq-in-hot-path",
        re.compile(r"\.(Where|Select|SelectMany|OrderBy|OrderByDescending|ToList|ToArray|FirstOrDefault|Any|All|Count)\s*\("),
        "Avoid LINQ allocations in hot gameplay paths. Use loops or cached buffers.",
    ),
    (
        "warn",
        "allocation-in-hot-path",
        re.compile(r"\bnew\s+(List|Dictionary|HashSet|Queue|Stack|StringBuilder)\b"),
        "Check for per-frame collection/string-buffer allocation. Reuse fields or object pools when this path runs often.",
    ),
    (
        "warn",
        "input-manager-string-in-hot-path",
        re.compile(r"\bInput\.Get(?:Axis|Button|Key)\s*\(\s*\""),
        "Old Input Manager string polling can add lookup cost. Consider generated Input System actions for larger input work.",
    ),
    (
        "warn",
        "animator-string-in-hot-path",
        re.compile(
            r"\b[A-Za-z_][A-Za-z0-9_]*\."
            r"(?:SetBool|SetFloat|SetInteger|SetTrigger|ResetTrigger|GetBool|GetFloat|GetInteger)\s*\(\s*\""
        ),
        "Animator parameter strings in hot paths should be cached with Animator.StringToHash.",
    ),
    (
        "warn",
        "distance-sqrt-in-hot-path",
        re.compile(r"\b(Vector2|Vector3)\.Distance\b|\.magnitude\b"),
        "Use sqrMagnitude for range comparisons when the exact distance is not needed.",
    ),
    (
        "info",
        "transform-property-in-hot-path",
        re.compile(r"\btransform\."),
        "Repeated transform property access may be worth caching when used heavily across many objects.",
    ),
]


def strip_inline_comment(line: str) -> str:
    return line.split("//", 1)[0].strip()


def is_empty_event_method(lines: list[str], signature_index: int) -> bool:
    depth = 0
    saw_open = False

    for offset, line in enumerate(lines[signature_index:]):
        code = strip_inline_comment(line)
        if not code:
            continue

        if offset == 0:
            code = EVENT_METHOD_RE.sub("", code, count=1).strip()

        depth += code.count("{")
        if "{" in code:
            saw_open = True

        content = code.replace("{", "").replace("}", "").strip()
        if saw_open and content:
            return False

        depth -= code.count("}")
        if saw_open and depth <= 0:
            return True

    return False


def iter_cs_files(root: Path) -> Iterable[Path]:
    for path in root.rglob("*.cs"):
        if any(part in SKIP_DIRS for part in path.parts):
            continue
        yield path


def update_method_context(line: str, current: str | None, depth: int) -> tuple[str | None, int]:
    match = METHOD_RE.search(line)
    if match:
        current = match.group("name")
        depth = 0

    if current:
        depth += line.count("{") - line.count("}")
        if depth <= 0 and "}" in line:
            current = None
            depth = 0

    return current, depth


def scan_file(path: Path, root: Path) -> list[Finding]:
    findings: list[Finding] = []
    try:
        lines = path.read_text(encoding="utf-8-sig").splitlines()
    except UnicodeDecodeError:
        lines = path.read_text(errors="replace").splitlines()

    current_method: str | None = None
    depth = 0
    relative = str(path.relative_to(root))

    for index, line in enumerate(lines, start=1):
        stripped = line.strip()
        current_method, depth = update_method_context(line, current_method, depth)
        if stripped.startswith("//"):
            continue

        method_match = EVENT_METHOD_RE.search(line)
        if method_match and method_match.group("name") in EMPTY_EVENT_METHODS and is_empty_event_method(lines, index - 1):
            findings.append(
                Finding(
                    "info",
                    relative,
                    index,
                    "empty-unity-event-method",
                    "Remove empty Unity event methods so Unity does not dispatch unused callbacks across many MonoBehaviours.",
                    stripped,
                )
            )

        for severity, rule, pattern, message in GLOBAL_RULES:
            if pattern.search(line):
                findings.append(Finding(severity, relative, index, rule, message, stripped))

        if current_method in HOT_METHODS:
            for severity, rule, pattern, message in HOT_RULES:
                if pattern.search(line):
                    findings.append(Finding(severity, relative, index, rule, f"{message} Detected in {current_method}.", stripped))

    return findings


def summarize(findings: list[Finding]) -> str:
    counts = {"error": 0, "warn": 0, "info": 0}
    for finding in findings:
        counts[finding.severity] += 1

    lines = [
        "# Unity Performance Scan",
        "",
        f"Findings: {len(findings)} total ({counts['error']} error, {counts['warn']} warn, {counts['info']} info)",
        "",
    ]

    if not findings:
        lines.append("No common static performance patterns were found. Use Unity Profiler for runtime bottlenecks.")
        return "\n".join(lines)

    severity_order = {"error": 0, "warn": 1, "info": 2}
    for finding in sorted(findings, key=lambda item: (severity_order[item.severity], item.file, item.line)):
        lines.append(f"- [{finding.severity.upper()}] {finding.file}:{finding.line} `{finding.rule}`")
        lines.append(f"  {finding.message}")
        lines.append(f"  `{finding.snippet}`")

    return "\n".join(lines)


def main() -> int:
    parser = argparse.ArgumentParser(description="Scan Unity C# scripts for performance triage findings.")
    parser.add_argument("project", type=Path, help="Unity project root or Assets/Scripts directory")
    parser.add_argument("--json", action="store_true", help="Emit JSON instead of Markdown")
    parser.add_argument("--fail-on-error", action="store_true", help="Return exit code 1 when error findings exist")
    args = parser.parse_args()

    root = args.project.expanduser().resolve()
    if not root.exists():
        parser.error(f"path does not exist: {root}")

    findings: list[Finding] = []
    for path in iter_cs_files(root):
        findings.extend(scan_file(path, root))

    if args.json:
        print(json.dumps([asdict(finding) for finding in findings], ensure_ascii=False, indent=2))
    else:
        print(summarize(findings))

    if args.fail_on_error and any(f.severity == "error" for f in findings):
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
