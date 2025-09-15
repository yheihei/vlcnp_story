using UnityEngine;
using UnityEngine.InputSystem;

namespace VLCNP.Input
{
    public class InputManager : MonoBehaviour
    {
        private static InputManager instance;
        public static InputManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("InputManager");
                    instance = go.AddComponent<InputManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private PlayerInputActions inputActions;
        private Vector2 moveInput;
        private Vector2 navigateInput;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            inputActions = new PlayerInputActions();
            inputActions.Enable();
        }

        private void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerInputActions();
            }
            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions?.Disable();
        }

        private void Update()
        {
            moveInput = inputActions.Player.Move.ReadValue<Vector2>();
            navigateInput = inputActions.UI.Navigate.ReadValue<Vector2>();
        }

        // Player Actions
        public bool IsMovingRight()
        {
            return moveInput.x > 0.1f;
        }

        public bool IsMovingLeft()
        {
            return moveInput.x < -0.1f;
        }

        public bool IsMovingUp()
        {
            return moveInput.y > 0.1f;
        }

        public bool IsMovingDown()
        {
            return moveInput.y < -0.1f;
        }

        public float GetHorizontalMovement()
        {
            return moveInput.x;
        }

        public float GetVerticalMovement()
        {
            return moveInput.y;
        }

        public bool IsJumpPressed()
        {
            return inputActions.Player.Jump.WasPressedThisFrame();
        }

        public bool IsJumpHeld()
        {
            return inputActions.Player.Jump.IsPressed();
        }

        public bool IsJumpReleased()
        {
            return inputActions.Player.Jump.WasReleasedThisFrame();
        }

        public bool IsAttackPressed()
        {
            return inputActions.Player.Attack.WasPressedThisFrame();
        }

        public bool IsInteractPressed()
        {
            return inputActions.Player.Interact.WasPressedThisFrame();
        }

        public bool IsSwitchCharacterPressed()
        {
            return inputActions.Player.SwitchCharacter.WasPressedThisFrame();
        }

        // UI Actions
        public bool IsNavigatingUp()
        {
            return navigateInput.y > 0.5f;
        }

        public bool IsNavigatingDown()
        {
            return navigateInput.y < -0.5f;
        }

        public bool IsSubmitPressed()
        {
            return inputActions.UI.Submit.WasPressedThisFrame();
        }

        // For UI navigation, we need to track when navigation was pressed (not held)
        private bool wasNavigatingUp = false;
        private bool wasNavigatingDown = false;

        public bool IsNavigateUpPressed()
        {
            bool isNavigatingNow = IsNavigatingUp();
            bool pressed = isNavigatingNow && !wasNavigatingUp;
            wasNavigatingUp = isNavigatingNow;
            return pressed;
        }

        public bool IsNavigateDownPressed()
        {
            bool isNavigatingNow = IsNavigatingDown();
            bool pressed = isNavigatingNow && !wasNavigatingDown;
            wasNavigatingDown = isNavigatingNow;
            return pressed;
        }

        // Switch between action maps
        public void SwitchToPlayerActions()
        {
            inputActions.UI.Disable();
            inputActions.Player.Enable();
        }

        public void SwitchToUIActions()
        {
            inputActions.Player.Disable();
            inputActions.UI.Enable();
        }
    }
}