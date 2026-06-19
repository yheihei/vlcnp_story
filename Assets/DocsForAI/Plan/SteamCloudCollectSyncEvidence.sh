#!/usr/bin/env bash
set -euo pipefail

label="${1:-device}"
out_dir="${2:-/tmp/vlcnp_steam_cloud_sync_evidence}"
app_id="${APP_ID:-4861250}"
save_dir="${SAVE_DIR:-$HOME/Library/Application Support/YheiWebDesign/VlcnpStory}"
steam_dir="${STEAM_DIR:-$HOME/Library/Application Support/Steam}"
player_log="${PLAYER_LOG:-$HOME/Library/Logs/YheiWebDesign/VlcnpStory/Player.log}"
cloud_log="${CLOUD_LOG:-$steam_dir/logs/cloud_log.txt}"
connection_log="${CONNECTION_LOG:-$steam_dir/logs/connection_log.txt}"
timestamp="$(date '+%Y%m%d_%H%M%S')"
report="$out_dir/${label}_${timestamp}.txt"

mkdir -p "$out_dir"

append_file_excerpt() {
  local title="$1"
  local file="$2"
  local pattern="$3"
  local max_lines="${4:-120}"

  {
    echo
    echo "## $title"
    echo "path: $file"
  } >> "$report"

  if [[ ! -f "$file" ]]; then
    echo "missing" >> "$report"
    return
  fi

  if [[ -n "$pattern" ]]; then
    grep -E "$pattern" "$file" | tail -n "$max_lines" >> "$report" || echo "no matching lines" >> "$report"
  else
    tail -n "$max_lines" "$file" >> "$report"
  fi
}

{
  echo "# Steam Cloud Sync Evidence"
  echo "label: $label"
  echo "timestamp: $(date '+%Y-%m-%d %H:%M:%S %Z')"
  echo "hostname: $(hostname)"
  echo "user: $(whoami)"
  echo "app_id: $app_id"
  echo "save_dir: $save_dir"
  echo "steam_dir: $steam_dir"
  echo "player_log: $player_log"
  echo "cloud_log: $cloud_log"
  echo
  echo "## OS"
  if command -v sw_vers >/dev/null 2>&1; then
    sw_vers
  else
    uname -a
  fi
  echo
  echo "## Save Files"
} > "$report"

if [[ -d "$save_dir" ]]; then
  find "$save_dir" -maxdepth 1 -type f -name '*.json' -print | sort | while IFS= read -r file; do
    size="$(wc -c < "$file" | tr -d ' ')"
    hash="$(shasum -a 256 "$file" | awk '{print $1}')"
    mtime="$(stat -f '%Sm' -t '%Y-%m-%d %H:%M:%S %Z' "$file" 2>/dev/null || stat -c '%y' "$file")"
    echo "$(basename "$file") sha256=$hash bytes=$size mtime=$mtime" >> "$report"
  done
else
  echo "save directory missing" >> "$report"
fi

append_file_excerpt "Steam Cloud Log" "$cloud_log" "AppID $app_id|$app_id|Auto-Cloud|Upload OK|Download OK|Download complete|Upload complete|Need to download|Need to upload|Successfully synced|sync failed" 180
append_file_excerpt "Player Log" "$player_log" "SteamBootstrap|SteamCloudSaveSync|Steam initialized|persistentDataPath|SteamAPI" 120
append_file_excerpt "Steam Connection Log" "$connection_log" "LogOn|Logged|Access Denied|RecvMsgClientLogOnResponse|steamid" 80

echo "$report"
