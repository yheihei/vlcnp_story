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

if [[ "$mode" == "--dry-run" ]]; then
    echo
    echo "Dry run only. Upload command:"
    echo "  cd '$stage_dir/builder'"
    echo "  '$steamcmd_path' +login '$builder_account' +run_app_build ../scripts/app_build_demo_template.vdf +quit"
    exit 0
fi

cd "$stage_dir/builder"
exec "$steamcmd_path" +login "$builder_account" +run_app_build ../scripts/app_build_demo_template.vdf +quit
