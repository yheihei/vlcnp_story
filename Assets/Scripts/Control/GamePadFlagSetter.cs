using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using VLCNP.Core;

namespace VLCNP.Control
{
    /// <summary>
    /// ロード完了時点でゲームパッドが利用可能かを確認し、Flag.IsGamePad を設定する。
    /// 常時監視は行わず、1回だけ判定する。
    /// </summary>
    public class GamePadFlagSetter : MonoBehaviour
    {
        private const float TimeoutSeconds = 20f;

        [SerializeField] private bool enableDebugLog = false;

        private bool hasSet = false;

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

            bool isGamePad = IsGamePadAvailable();
            TrySetFlag(isGamePad);
            hasSet = true;
        }

        private static bool IsGamePadAvailable()
        {
            if (Gamepad.current != null) return true;
            return Gamepad.all.Count > 0;
        }

        private void TrySetFlag(bool isGamePad)
        {
            GameObject flagManagerObject = GameObject.FindWithTag("FlagManager");
            if (flagManagerObject == null)
            {
                Debug.LogWarning("[GamePadFlagSetter] FlagManager (Tag: FlagManager) not found. Skip setting Flag.IsGamePad.");
                return;
            }

            FlagManager flagManager = flagManagerObject.GetComponent<FlagManager>();
            if (flagManager == null)
            {
                Debug.LogWarning("[GamePadFlagSetter] FlagManager component not found. Skip setting Flag.IsGamePad.");
                return;
            }

            flagManager.SetFlag(Flag.IsGamePad, isGamePad);

            if (enableDebugLog)
            {
                Debug.Log($"[GamePadFlagSetter] Flag.IsGamePad -> {isGamePad}");
            }
        }
    }
}
