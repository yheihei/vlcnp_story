using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using VLCNP.Core;

namespace VLCNP.Control
{
    /// <summary>
    /// ロード完了後にゲームパッド利用可否を判定し、Flag.IsGamePad を更新する。
    /// WebGL の遅延認識対策として接続変化監視とポーリング再判定を行う。
    /// </summary>
    public class GamePadFlagSetter : MonoBehaviour
    {
        private const float TimeoutSeconds = 20f;
        private const float PollIntervalSeconds = 3f;

        [SerializeField] private bool enableDebugLog = false;

        private bool hasSet = false;
        private bool isReady = false;
        private bool hasLastState = false;
        private bool lastIsGamePad = false;
        private bool hasLoggedMissingFlagManager = false;
        private Coroutine monitorCoroutine;

        private void OnEnable()
        {
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        private void OnDisable()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;

            if (monitorCoroutine != null)
            {
                StopCoroutine(monitorCoroutine);
                monitorCoroutine = null;
            }
        }

        private void Start()
        {
            StartCoroutine(SetFlagOnLoadComplete());
        }

        private IEnumerator SetFlagOnLoadComplete()
        {
            if (hasSet) yield break;

            float startTime = Time.realtimeSinceStartup;

            while (LoadCompleteManager.Instance == null)
            {
                if (Time.realtimeSinceStartup - startTime >= TimeoutSeconds)
                {
                    Debug.LogWarning(
                        $"[GamePadFlagSetter] Timeout ({TimeoutSeconds:0}s). LoadCompleteManager.Instance not found. Skip setting Flag.IsGamePad."
                    );
                    hasSet = true;
                    yield break;
                }
                yield return null;
            }
            while (!LoadCompleteManager.Instance.IsLoaded)
            {
                if (Time.realtimeSinceStartup - startTime >= TimeoutSeconds)
                {
                    Debug.LogWarning(
                        $"[GamePadFlagSetter] Timeout ({TimeoutSeconds:0}s). LoadCompleteManager.IsLoaded did not become true. Skip setting Flag.IsGamePad."
                    );
                    hasSet = true;
                    yield break;
                }
                yield return null;
            }

            isReady = true;
            RefreshGamePadFlag("load-complete");

            if (monitorCoroutine == null)
            {
                monitorCoroutine = StartCoroutine(PollGamePadState());
            }

            hasSet = true;
        }

        private IEnumerator PollGamePadState()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(PollIntervalSeconds);

                if (!isReady) continue;
                RefreshGamePadFlag("poll");
            }
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (!isReady) return;
            if (!(device is Gamepad)) return;

            RefreshGamePadFlag($"device-change:{change}");
        }

        private static bool IsGamePadAvailable()
        {
            if (Gamepad.current != null) return true;
            return Gamepad.all.Count > 0;
        }

        private void RefreshGamePadFlag(string source)
        {
            bool isGamePad = IsGamePadAvailable();
            if (hasLastState && lastIsGamePad == isGamePad) return;
            if (!TrySetFlag(isGamePad, source)) return;

            lastIsGamePad = isGamePad;
            hasLastState = true;
        }

        private bool TrySetFlag(bool isGamePad, string source)
        {
            GameObject flagManagerObject = GameObject.FindWithTag("FlagManager");
            if (flagManagerObject == null)
            {
                if (!hasLoggedMissingFlagManager)
                {
                    Debug.LogWarning("[GamePadFlagSetter] FlagManager (Tag: FlagManager) not found. Skip setting Flag.IsGamePad.");
                    hasLoggedMissingFlagManager = true;
                }
                return false;
            }

            FlagManager flagManager = flagManagerObject.GetComponent<FlagManager>();
            if (flagManager == null)
            {
                if (!hasLoggedMissingFlagManager)
                {
                    Debug.LogWarning("[GamePadFlagSetter] FlagManager component not found. Skip setting Flag.IsGamePad.");
                    hasLoggedMissingFlagManager = true;
                }
                return false;
            }

            hasLoggedMissingFlagManager = false;
            flagManager.SetFlag(Flag.IsGamePad, isGamePad);

            if (enableDebugLog)
            {
                Debug.Log($"[GamePadFlagSetter] Flag.IsGamePad -> {isGamePad} ({source})");
            }

            return true;
        }
    }
}
