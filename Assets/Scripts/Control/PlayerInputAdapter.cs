using UnityEngine;
using UnityEngine.InputSystem;

namespace VLCNP.Control
{
    /// <summary>
    /// キーボードとゲームパッド両対応の入力ヘルパークラス。
    /// Unity Input System パッケージが有効であることを前提とする。
    /// </summary>
    public static class PlayerInputAdapter
    {
        private const float StickDeadZone = 0.3f;
        private const string LogTag = "[PlayerInputAdapter]";
        public static bool EnableDebugLogs = false;

        private static bool reportedNoGamepad = false;
        private static string lastGamepadId = "";
        private static float lastLoggedHorizontal = 0f;

        public static float GetMoveHorizontal()
        {
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
                ReportGamepad(gamepad);
                float stickValue = gamepad.leftStick.ReadValue().x;
                if (Mathf.Abs(stickValue) > StickDeadZone)
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
            }
            return Mathf.Clamp(horizontal, -1f, 1f);
        }

        public static bool IsAimUpPressed()
        {
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
            if (Input.GetKeyDown(KeyCode.X))
            {
                return true;
            }

            return false;
        }

        private static void ReportGamepad(Gamepad gamepad)
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
                    $"{LogTag} StickX={horizontal:F2}, DPadRight={dpadRight}, DPadLeft={dpadLeft}, DPadUp={dpadUp}, OutputHorizontal={Mathf.Clamp(horizontal, -1f, 1f):F2}"
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
