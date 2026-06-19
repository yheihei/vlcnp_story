#!/usr/bin/env bash
set -euo pipefail

usage() {
    cat <<'USAGE'
Usage:
  upload_steam_demo_build.sh <stage_dir> <builder_account> [--dry-run]

Environment:
  STEAMCMD  Path to steamcmd or steamcmd.sh. If omitted, the script checks:
            steamcmd, steamcmd.sh, /tmp/steamcmd_osx_20260619/steamcmd.sh

Notes:
  The script does not accept or store a password. SteamCMD will prompt for the
  builder account credentials and Steam Guard code when required.
USAGE
}

stage_dir="${1:-}"
builder_account="${2:-}"
mode="${3:-}"

if [[ -z "$stage_dir" || -z "$builder_account" || "$mode" != "" && "$mode" != "--dry-run" ]]; then
    usage >&2
    exit 1
fi

if [[ -d "$stage_dir" ]]; then
    stage_dir="$(cd "$stage_dir" && pwd -P)"
fi

resolve_steamcmd() {
    if [[ -n "${STEAMCMD:-}" ]]; then
        echo "$STEAMCMD"
        return
    fi

    if command -v steamcmd >/dev/null 2>&1; then
        command -v steamcmd
        return
    fi

    if command -v steamcmd.sh >/dev/null 2>&1; then
        command -v steamcmd.sh
        return
    fi

    if [[ -x /tmp/steamcmd_osx_20260619/steamcmd.sh ]]; then
        echo /tmp/steamcmd_osx_20260619/steamcmd.sh
        return
    fi

    return 1
}

require_path() {
    local path="$1"
    if [[ ! -e "$path" ]]; then
        echo "Missing required path: $path" >&2
        exit 1
    fi
}

vdf_escape() {
    local value="$1"
    value="${value//\\/\\\\}"
    value="${value//\"/\\\"}"
    printf '%s' "$value"
}

write_runtime_vdfs() {
    local runtime_dir="$stage_dir/builder/generated"
    mkdir -p "$runtime_dir"

    local app_build_vdf="$runtime_dir/app_build_demo_runtime.vdf"
    local windows_depot_vdf="$runtime_dir/depot_build_demo_windows_runtime.vdf"
    local macos_depot_vdf="$runtime_dir/depot_build_demo_macos_runtime.vdf"

    cat > "$windows_depot_vdf" <<EOF
"DepotBuildConfig"
{
    "DepotID" "4861251"
    "ContentRoot" "$(vdf_escape "$stage_dir/content/windows")"

    "FileMapping"
    {
        "LocalPath" "*"
        "DepotPath" "."
        "recursive" "1"
    }

    "FileExclusion" "steam_appid.txt"
}
EOF

    cat > "$macos_depot_vdf" <<EOF
"DepotBuildConfig"
{
    "DepotID" "4861252"
    "ContentRoot" "$(vdf_escape "$stage_dir/content/macos")"

    "FileMapping"
    {
        "LocalPath" "*"
        "DepotPath" "."
        "recursive" "1"
    }

    "FileExclusion" "steam_appid.txt"
}
EOF

    cat > "$app_build_vdf" <<EOF
"AppBuild"
{
    "AppID" "4861250"
    "Desc" "VLCNP Story demo build"
    "Preview" "0"
    "Local" ""
    "SetLive" ""
    "ContentRoot" "$(vdf_escape "$stage_dir/content")"
    "BuildOutput" "$(vdf_escape "$stage_dir/output")"

    "Depots"
    {
        "4861251" "$(vdf_escape "$windows_depot_vdf")"
        "4861252" "$(vdf_escape "$macos_depot_vdf")"
    }
}
EOF

    printf '%s' "$app_build_vdf"
}

steamcmd_path="$(resolve_steamcmd || true)"
if [[ -z "$steamcmd_path" || ! -x "$steamcmd_path" ]]; then
    echo "steamcmd not found. Set STEAMCMD=/path/to/steamcmd or install SteamCMD." >&2
    exit 1
fi

require_path "$stage_dir/builder"
require_path "$stage_dir/scripts/app_build_demo_template.vdf"
require_path "$stage_dir/scripts/depot_build_demo_windows_template.vdf"
require_path "$stage_dir/scripts/depot_build_demo_macos_template.vdf"
require_path "$stage_dir/content/windows/VlcnpStory.exe"
require_path "$stage_dir/content/windows/UnityPlayer.dll"
require_path "$stage_dir/content/macos/VlcnpStory.app/Contents/MacOS/VlcnpStory"

if find "$stage_dir/content" -name 'steam_appid.txt' -print -quit | grep -q .; then
    echo "steam_appid.txt must not be uploaded through SteamPipe:" >&2
    find "$stage_dir/content" -name 'steam_appid.txt' >&2
    exit 1
fi

echo "SteamCMD: $steamcmd_path"
echo "Stage: $stage_dir"
echo "Builder account: $builder_account"

app_build_vdf="$(write_runtime_vdfs)"
echo "Runtime app build VDF: $app_build_vdf"

if [[ "$mode" == "--dry-run" ]]; then
    echo
    echo "Dry run only. Upload command:"
    echo "  '$steamcmd_path' +login '$builder_account' +run_app_build '$app_build_vdf' +quit"
    exit 0
fi

exec "$steamcmd_path" +login "$builder_account" +run_app_build "$app_build_vdf" +quit
