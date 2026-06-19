# Steam Cloud Multi-Device Verification

## Purpose
- Target issue: #627
- Goal: prove that Demo App `4861250` syncs `autoSave.json` / `save.json` across two devices through Steam Cloud.
- Windows real-device verification remains in #637. This check can be done with two Macs, or with one Mac and one later Windows device.

## Required Evidence
- Device A upload:
  - Demo App `4861250` is launched from the Steam client.
  - `autoSave.json` or `save.json` is updated and the game is closed.
  - Steam `cloud_log.txt` shows AppID `4861250` upload success.
  - The updated save file SHA-256 is recorded.
- Device B download:
  - Demo App `4861250` is launched from the Steam client with the same Steam account.
  - Steam `cloud_log.txt` shows AppID `4861250` download success during launch sync.
  - Device B save file SHA-256 matches Device A.
  - Player.log or the save file content confirms the game loaded the synced data.

## macOS Evidence Collection
Run this on each Mac after the relevant launch or exit sync:

```bash
Assets/DocsForAI/Plan/SteamCloudCollectSyncEvidence.sh device-a
Assets/DocsForAI/Plan/SteamCloudCollectSyncEvidence.sh device-b
```

The script writes reports under `/tmp/vlcnp_steam_cloud_sync_evidence` by default.

Useful environment overrides:

```bash
SAVE_DIR="$HOME/Library/Application Support/YheiWebDesign/VlcnpStory" \
STEAM_DIR="$HOME/Library/Application Support/Steam" \
PLAYER_LOG="$HOME/Library/Logs/YheiWebDesign/VlcnpStory/Player.log" \
Assets/DocsForAI/Plan/SteamCloudCollectSyncEvidence.sh device-a
```

## Pass Criteria
- Device A report contains AppID `4861250` upload success in the Steam Cloud log excerpt.
- Device B report contains AppID `4861250` download success in the Steam Cloud log excerpt.
- `autoSave.json` or `save.json` SHA-256 is identical between the two reports.
- #627 can then check off `複数端末で同期できることを動作確認`.

## Current Status
- macOS single-device Steam client launch, Cloud upload, Cloud download, and restart load are verified.
- True multi-device sync is not verified yet because only one local device has been checked.
