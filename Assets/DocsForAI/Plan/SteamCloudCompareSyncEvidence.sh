#!/usr/bin/env bash
set -euo pipefail

if [[ $# -ne 2 ]]; then
  echo "Usage: $0 <device-a-report.txt> <device-b-report.txt>" >&2
  exit 2
fi

device_a_report="$1"
device_b_report="$2"
app_id="${APP_ID:-4861250}"

require_file() {
  local file="$1"
  if [[ ! -f "$file" ]]; then
    echo "missing report: $file" >&2
    exit 2
  fi
}

extract_hash() {
  local report="$1"
  local filename="$2"
  awk -v filename="$filename" '
    $1 == filename {
      for (i = 1; i <= NF; i++) {
        if ($i ~ /^sha256=/) {
          sub(/^sha256=/, "", $i)
          print $i
          exit
        }
      }
    }
  ' "$report"
}

has_upload_success() {
  local report="$1"
  grep -Eq "\\[AppID ${app_id}\\].*(Upload OK|Upload complete, result OK|Upload complete in build list)" "$report"
}

has_download_success() {
  local report="$1"
  grep -Eq "\\[AppID ${app_id}\\].*(Download OK|Download complete, result OK|Download complete in build list|Successfully synced to ChangeNumber)" "$report"
}

require_file "$device_a_report"
require_file "$device_b_report"

if [[ "$(cd "$(dirname "$device_a_report")" && pwd -P)/$(basename "$device_a_report")" == "$(cd "$(dirname "$device_b_report")" && pwd -P)/$(basename "$device_b_report")" ]]; then
  echo "device_a_report and device_b_report must be different files" >&2
  exit 2
fi

status=0

echo "# Steam Cloud Multi-Device Evidence Comparison"
echo "device_a_report: $device_a_report"
echo "device_b_report: $device_b_report"
echo "app_id: $app_id"
echo

if has_upload_success "$device_a_report"; then
  echo "device_a_upload: ok"
else
  echo "device_a_upload: missing"
  status=1
fi

if has_download_success "$device_b_report"; then
  echo "device_b_download: ok"
else
  echo "device_b_download: missing"
  status=1
fi

matching_files=0
for filename in autoSave.json save.json; do
  hash_a="$(extract_hash "$device_a_report" "$filename")"
  hash_b="$(extract_hash "$device_b_report" "$filename")"

  if [[ -z "$hash_a" || -z "$hash_b" ]]; then
    echo "$filename: missing hash"
    status=1
    continue
  fi

  if [[ "$hash_a" == "$hash_b" ]]; then
    echo "$filename: hash match $hash_a"
    matching_files=$((matching_files + 1))
  else
    echo "$filename: hash mismatch device_a=$hash_a device_b=$hash_b"
    status=1
  fi
done

if [[ "$matching_files" -eq 0 ]]; then
  status=1
fi

echo
if [[ "$status" -eq 0 ]]; then
  echo "result: pass"
else
  echo "result: fail"
fi

exit "$status"
