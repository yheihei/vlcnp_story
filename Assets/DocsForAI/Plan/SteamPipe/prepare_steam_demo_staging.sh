#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "$script_dir/../../../../" && pwd)"

mac_build="${1:-/tmp/vlcnpStory_SteamDemoMacSteamPipe}"
windows_build="${2:-/tmp/vlcnpStory_SteamDemoWindowsSteamPipe}"
stage_dir="${3:-/tmp/vlcnpStory_SteamPipeDemo_$(date +%Y%m%d_%H%M%S)}"

app_build_vdf="$script_dir/app_build_demo_template.vdf"
windows_depot_vdf="$script_dir/depot_build_demo_windows_template.vdf"
macos_depot_vdf="$script_dir/depot_build_demo_macos_template.vdf"

mac_app="$mac_build/VlcnpStory.app"
windows_exe="$windows_build/VlcnpStory.exe"

require_path() {
    local path="$1"
    if [[ ! -e "$path" ]]; then
        echo "Missing required path: $path" >&2
        exit 1
    fi
}

require_path "$app_build_vdf"
require_path "$windows_depot_vdf"
require_path "$macos_depot_vdf"
require_path "$mac_app"
require_path "$mac_app/Contents/MacOS/VlcnpStory"
require_path "$mac_app/Contents/PlugIns/steam_api.bundle/Contents/MacOS/libsteam_api.dylib"
require_path "$windows_exe"
require_path "$windows_build/UnityPlayer.dll"
require_path "$windows_build/VlcnpStory_Data/Plugins/x86_64/steam_api64.dll"

rm -rf "$stage_dir"
mkdir -p "$stage_dir/scripts" "$stage_dir/content/windows" "$stage_dir/content/macos" "$stage_dir/builder" "$stage_dir/output"

cp "$app_build_vdf" "$stage_dir/scripts/"
cp "$windows_depot_vdf" "$stage_dir/scripts/"
cp "$macos_depot_vdf" "$stage_dir/scripts/"

rsync -a --exclude 'steam_appid.txt' "$windows_build/" "$stage_dir/content/windows/"
rsync -a --exclude 'steam_appid.txt' "$mac_app" "$stage_dir/content/macos/"

if find "$stage_dir/content" -name 'steam_appid.txt' -print -quit | grep -q .; then
    echo "steam_appid.txt must not be uploaded through SteamPipe:" >&2
    find "$stage_dir/content" -name 'steam_appid.txt' >&2
    exit 1
fi

require_path "$stage_dir/content/windows/VlcnpStory.exe"
require_path "$stage_dir/content/windows/UnityPlayer.dll"
require_path "$stage_dir/content/windows/VlcnpStory_Data/Plugins/x86_64/steam_api64.dll"
require_path "$stage_dir/content/macos/VlcnpStory.app/Contents/MacOS/VlcnpStory"
require_path "$stage_dir/content/macos/VlcnpStory.app/Contents/PlugIns/steam_api.bundle/Contents/MacOS/libsteam_api.dylib"

echo "SteamPipe staging prepared:"
echo "  $stage_dir"
echo
echo "Copy or create this directory layout under <Steamworks SDK>/tools/ContentBuilder, then run:"
echo "  cd <Steamworks SDK>/tools/ContentBuilder/builder"
echo "  steamcmd +login <builder_account> +run_app_build ../scripts/app_build_demo_template.vdf +quit"
echo
echo "Source builds:"
echo "  macOS:   $mac_build"
echo "  Windows: $windows_build"
echo "Repo: $repo_root"
