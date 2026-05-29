using UnityEngine;
using UnityEngine.InputSystem;
using VLCNP.SceneManagement;

namespace VLCNP.Control
{
    /// <summary>
    /// キーボードとゲームパッド両対応の入力ヘルパークラス。
    /// Unity Input System パッケージが有効であることを前提とする。
    /// </summary>
    public static class PlayerInputAdapter
    {
        private const float StickDeadZone = 0.3f;
        private const float StickReverseBounceWindow = 0.08f;
        private const float StickReverseConfirmDuration = 0.05f;
        private const float StickStrongReverseThreshold = 0.65f;
        private const string LogTag = "[PlayerInputAdapter]";
        public static bool EnableDebugLogs = false;

        private static bool reportedNoGamepad = false;
        private static string lastGamepadId = "";
        private static float lastLoggedHorizontal = 0f;
        private static int lastAcceptedStickDirection = 0;
        private static int pendingReverseStickDirection = 0;
        private static float pendingReverseStickStartedAt = 0f;
        private static float lastStickNeutralTime = float.NegativeInfinity;

        private static bool IsGameplayInputBlocked()
        {
            return TransitionEvent.IsAnyTransitionRunning;
        }

        public static float GetMoveHorizontal()
        {
            if (IsGameplayInputBlocked())
            {
                ResetMoveStickFilter();
                return 0f;
            }

            float horizontal = 0f;
            if (Input.GetKey("right"))
            {
                horizontal = 1f;
            }
            if (Input.GetKey("left"))
            {
                horizontal = -1f;
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                float stickValue = GetFilteredMoveStickHorizontal(gamepad.leftStick.ReadValue().x);
                if (!Mathf.Approximately(stickValue, 0f))
                {
                    horizontal = stickValue;
                }
                else if (gamepad.dpad.right.isPressed)
                {
                    horizontal = 1f;
                }
                else if (gamepad.dpad.left.isPressed)
                {
                    horizontal = -1f;
                }
                ReportGamepad(gamepad, horizontal);
            }
            return Mathf.Clamp(horizontal, -1f, 1f);
        }

        private static float GetFilteredMoveStickHorizontal(float stickValue)
        {
            float now = Time.unscaledTime;
            float absoluteStickValue = Mathf.Abs(stickValue);

            if (absoluteStickValue <= StickDeadZone)
            {
                lastStickNeutralTime = now;
                pendingReverseStickDirection = 0;
                return 0f;
            }

            int direction = stickValue > 0f ? 1 : -1;
            if (lastAcceptedStickDirection == 0 || direction == lastAcceptedStickDirection)
            {
                AcceptMoveStickDirection(direction);
                return stickValue;
            }

            bool recentlyNeutral = now - lastStickNeutralTime <= StickReverseBounceWindow;
            if (!recentlyNeutral || absoluteStickValue >= StickStrongReverseThreshold)
            {
                AcceptMoveStickDirection(direction);
                return stickValue;
            }

            if (pendingReverseStickDirection != direction)
            {
                pendingReverseStickDirection = direction;
                pendingReverseStickStartedAt = now;
                return 0f;
            }

            if (now - pendingReverseStickStartedAt >= StickReverseConfirmDuration)
            {
                AcceptMoveStickDirection(direction);
                return stickValue;
            }

            return 0f;
        }

        private static void AcceptMoveStickDirection(int direction)
        {
            lastAcceptedStickDirection = direction;
            pendingReverseStickDirection = 0;
        }

        private static void ResetMoveStickFilter()
        {
            lastAcceptedStickDirection = 0;
            pendingReverseStickDirection = 0;
            lastStickNeutralTime = float.NegativeInfinity;
        }

        public static bool IsAimUpPressed()
        {
            if (IsGameplayInputBlocked())
            {
                return false;
            }

            if (Input.GetKey("up"))
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                if (gamepad.dpad.up.isPressed)
                {
                    return true;
                }

                if (
                    gamepad.leftStick.up.isPressed
                    && Mathf.Abs(gamepad.leftStick.ReadValue().y) > StickDeadZone
                )
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsAimDownPressed()
        {
            if (IsGameplayInputBlocked())
            {
                return false;
            }

            if (Input.GetKey("down"))
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                if (gamepad.dpad.down.isPressed)
                {
                    return true;
                }

                if (
                    gamepad.leftStick.down.isPressed
                    && Mathf.Abs(gamepad.leftStick.ReadValue().y) > StickDeadZone
                )
                {
                    return true;
                }
            }
            return false;
        }

        public static bool WasAttackPressed(string keyboardAttackButton)
        {
            if (IsGameplayInputBlocked())
            {
                return false;
            }

            if (
                !string.IsNullOrEmpty(keyboardAttackButton)
                && Input.GetKeyDown(keyboardAttackButton)
            )
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null && gamepad.buttonWest.wasPressedThisFrame)
            {
                return true;
            }
            return false;
        }

        public static bool WasInteractPressed()
        {
            if (IsGameplayInputBlocked())
            {
                return false;
            }

            if (Input.GetKeyDown("up"))
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                if (gamepad.dpad.up.wasPressedThisFrame)
                {
                    return true;
                }

                if (gamepad.leftStick.up.wasPressedThisFrame)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool WasCharacterSwitchPressed(KeyCode keyboardKey)
        {
            if (IsGameplayInputBlocked())
            {
                return false;
            }

            if (Input.GetKeyDown(keyboardKey))
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null && gamepad.buttonNorth.wasPressedThisFrame)
            {
                return true;
            }
            return false;
        }

        public static bool WasMenuDownPressed()
        {
            if (Input.GetKeyDown("down"))
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                if (gamepad.dpad.down.wasPressedThisFrame)
                {
                    return true;
                }

                if (gamepad.leftStick.down.wasPressedThisFrame)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool WasMenuUpPressed()
        {
            if (Input.GetKeyDown("up"))
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                if (gamepad.dpad.up.wasPressedThisFrame)
                {
                    return true;
                }

                if (gamepad.leftStick.up.wasPressedThisFrame)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool WasMenuSubmitPressed()
        {
            if (Input.GetKeyDown("return"))
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null && gamepad.buttonEast.wasPressedThisFrame)
            {
                return true;
            }
            return false;
        }

        public static bool WasJumpPressed(string keyboardJumpButton)
        {
            if (IsGameplayInputBlocked())
            {
                return false;
            }

            if (!string.IsNullOrEmpty(keyboardJumpButton) && Input.GetKeyDown(keyboardJumpButton))
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame)
            {
                return true;
            }
            return false;
        }

        public static bool WasJumpReleased(string keyboardJumpButton)
        {
            if (IsGameplayInputBlocked())
            {
                return false;
            }

            if (!string.IsNullOrEmpty(keyboardJumpButton) && Input.GetKeyUp(keyboardJumpButton))
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null && gamepad.buttonSouth.wasReleasedThisFrame)
            {
                return true;
            }
            return false;
        }

        public static bool IsJumpHeld(string keyboardJumpButton)
        {
            if (IsGameplayInputBlocked())
            {
                return false;
            }

            if (!string.IsNullOrEmpty(keyboardJumpButton) && Input.GetKey(keyboardJumpButton))
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null && gamepad.buttonSouth.isPressed)
            {
                return true;
            }
            return false;
        }

        public static bool WasDashPressed()
        {
            if (IsGameplayInputBlocked())
            {
                return false;
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                return true;
            }

            return false;
        }

        private static void ReportGamepad(Gamepad gamepad, float outputHorizontal)
        {
            if (!EnableDebugLogs)
                return;

            reportedNoGamepad = false;
            string currentId = gamepad.displayName ?? gamepad.name;
            if (currentId != lastGamepadId)
            {
                lastGamepadId = currentId;
                Debug.Log($"{LogTag} Gamepad detected: {currentId}");
            }

            float horizontal = gamepad.leftStick.ReadValue().x;
            bool dpadRight = gamepad.dpad.right.isPressed;
            bool dpadLeft = gamepad.dpad.left.isPressed;
            bool dpadUp = gamepad.dpad.up.isPressed;
            if (
                Mathf.Abs(horizontal - lastLoggedHorizontal) > 0.05f
                || dpadRight
                || dpadLeft
                || dpadUp
            )
            {
                lastLoggedHorizontal = horizontal;
                Debug.Log(
                    $"{LogTag} StickX={horizontal:F2}, DPadRight={dpadRight}, DPadLeft={dpadLeft}, DPadUp={dpadUp}, OutputHorizontal={Mathf.Clamp(outputHorizontal, -1f, 1f):F2}"
                );
            }
        }

        static PlayerInputAdapter()
        {
            Application.onBeforeRender += CheckNoGamepad;
        }

        private static void CheckNoGamepad()
        {
            if (!EnableDebugLogs)
                return;

            if (Gamepad.current == null && !reportedNoGamepad)
            {
                reportedNoGamepad = true;
                lastGamepadId = "";
                Debug.Log($"{LogTag} Gamepad.current is null");
            }
        }
    }
}
