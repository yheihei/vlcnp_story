using UnityEngine;
using VLCNP.Control;
using VLCNP.Core;

namespace VLCNP.Movement
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class VLMitamaFloat : MonoBehaviour, IStoppable, IWaterEventListener
    {
        [SerializeField]
        Leg leg = null;

        [SerializeField, Min(0f)]
        float maxFuelSeconds = 1.2f;

        [SerializeField]
        float floatVelocityY = 0.35f;

        [SerializeField]
        float activationVelocityY = 0.05f;

        [SerializeField]
        string jumpButton = "space";

        [SerializeField]
        bool disableInWater = true;

        Rigidbody2D rBody;
        KabeKickEffectController kabeKickEffectController;
        float fuelSeconds;
        bool isFloating = false;
        bool isStopped = false;
        bool isInWater = false;

        public bool IsStopped
        {
            get => isStopped;
            set
            {
                if (isStopped == value)
                    return;

                isStopped = value;
                if (isStopped)
                {
                    StopFloating();
                }
            }
        }

        private void Awake()
        {
            rBody = GetComponent<Rigidbody2D>();
            if (leg == null)
            {
                leg = GetComponentInChildren<Leg>();
            }
            kabeKickEffectController = GetComponentInChildren<KabeKickEffectController>();
            fuelSeconds = maxFuelSeconds;

            if (leg != null)
            {
                leg.OnLanded += OnLanded;
            }
        }

        private void OnDestroy()
        {
            if (leg != null)
            {
                leg.OnLanded -= OnLanded;
            }
        }

        private void Update()
        {
            if (LoadCompleteManager.Instance != null && !LoadCompleteManager.Instance.IsLoaded)
                return;

            if (isStopped)
            {
                StopFloating();
                return;
            }

            if (leg != null && leg.IsGround)
            {
                RecoverFuel();
                StopFloating();
                return;
            }

            if (ShouldStopFloating())
            {
                StopFloating();
                return;
            }

            if (CanFloat() && PlayerInputAdapter.IsJumpHeld(jumpButton))
            {
                isFloating = true;
            }
        }

        private void FixedUpdate()
        {
            if (!isFloating)
                return;

            fuelSeconds = Mathf.Max(0f, fuelSeconds - Time.fixedDeltaTime);
            rBody.velocity = new Vector2(rBody.velocity.x, floatVelocityY);

            if (fuelSeconds <= 0f)
            {
                StopFloating();
            }
        }

        private void OnLanded()
        {
            RecoverFuel();
            StopFloating();
        }

        private bool CanFloat()
        {
            return fuelSeconds > 0f
                && !IsGround()
                && !IsWaterBlocking()
                && !IsKabeBlocking()
                && rBody.velocity.y <= activationVelocityY;
        }

        private bool ShouldStopFloating()
        {
            return isFloating
                && (
                    fuelSeconds <= 0f
                    || PlayerInputAdapter.WasJumpReleased(jumpButton)
                    || !PlayerInputAdapter.IsJumpHeld(jumpButton)
                    || IsWaterBlocking()
                    || IsKabeBlocking()
                );
        }

        private bool IsGround()
        {
            return leg != null && leg.IsGround;
        }

        private bool IsWaterBlocking()
        {
            return disableInWater && isInWater;
        }

        private bool IsKabeBlocking()
        {
            return kabeKickEffectController != null
                && (kabeKickEffectController.IsKabekick() || kabeKickEffectController.IsGrabbing());
        }

        private void RecoverFuel()
        {
            fuelSeconds = maxFuelSeconds;
        }

        private void StopFloating()
        {
            isFloating = false;
        }

        public void OnWaterEnter()
        {
            isInWater = true;
            StopFloating();
        }

        public void OnWaterExit()
        {
            isInWater = false;
        }

        public void OnWaterStay()
        {
            isInWater = true;
        }
    }
}
