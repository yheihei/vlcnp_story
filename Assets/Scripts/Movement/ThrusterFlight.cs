using UnityEngine;
using VLCNP.Control;
using VLCNP.Core;

namespace VLCNP.Movement
{
    /**
     * ナルカミ加入後に全プレイアブルキャラが使えるスラスター飛行。
     * 空中でジャンプボタンを新規押下すると、その瞬間に押していた方向キーの方向
     * (上/左右/下、ニュートラルは上)へ一定速度で噴射する。
     * 燃料は着地で全回復。噴射終了時(燃料切れ・ボタン解放)は噴射方向の速度が半減する。
     */
    [RequireComponent(typeof(Rigidbody2D))]
    public class ThrusterFlight : MonoBehaviour, IStoppable, IWaterEventListener
    {
        enum ThrustDirection
        {
            Up,
            Left,
            Right,
            Down,
        }

        [SerializeField]
        Leg leg = null;

        [SerializeField, Min(0f)]
        float maxFuelSeconds = 1f;

        [SerializeField, Min(0f)]
        float boostSpeed = 7.5f;

        [SerializeField]
        string jumpButton = "space";

        [SerializeField]
        bool disableInWater = true;

        [SerializeField]
        ParticleSystem thrustEffect = null;

        [SerializeField]
        AudioSource audioSource = null;

        [SerializeField]
        AudioClip thrustSe = null;

        [SerializeField]
        float thrustSeVolume = 0.2f;

        Rigidbody2D rBody;
        KabeKickEffectController kabeKickEffectController;
        FlagManager flagManager;
        float fuelSeconds;
        bool isThrusting = false;
        ThrustDirection thrustDirection = ThrustDirection.Up;
        float gravityScaleBeforeThrust = 0f;
        bool isStopped = false;
        bool isInWater = false;

        public bool IsThrusting => isThrusting;
        public bool IsHorizontalThrusting =>
            isThrusting
            && (thrustDirection == ThrustDirection.Left || thrustDirection == ThrustDirection.Right);
        public bool HasFuel => fuelSeconds > 0f;
        public float FuelSeconds => fuelSeconds;

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
                    StopThrust();
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
            if (audioSource == null)
            {
                audioSource = GetComponentInChildren<AudioSource>();
            }
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

        private void OnDisable()
        {
            StopThrust();
        }

        private void Update()
        {
            if (LoadCompleteManager.Instance != null && !LoadCompleteManager.Instance.IsLoaded)
                return;

            if (isStopped)
            {
                StopThrust();
                return;
            }

            if (leg != null && leg.IsGround)
            {
                RecoverFuel();
                StopThrust();
                return;
            }

            if (ShouldEndThrust())
            {
                EndThrustWithSlowdown();
                return;
            }

            if (!isThrusting && !IsGround() && PlayerInputAdapter.WasJumpPressed(jumpButton))
            {
                if (CanActivate())
                {
                    StartThrust();
                }
                else
                {
                    PerfLog.Log(
                        $"[ThrusterFlight] 発動不可 unlocked={IsUnlocked()} fuel={fuelSeconds:F2}"
                    );
                }
            }
        }

        private void FixedUpdate()
        {
            if (!isThrusting)
                return;

            fuelSeconds = Mathf.Max(0f, fuelSeconds - Time.fixedDeltaTime);
            ApplyThrustVelocity();

            if (fuelSeconds <= 0f)
            {
                EndThrustWithSlowdown();
            }
        }

        private void ApplyThrustVelocity()
        {
            switch (thrustDirection)
            {
                case ThrustDirection.Up:
                    rBody.velocity = new Vector2(rBody.velocity.x, boostSpeed);
                    break;
                case ThrustDirection.Down:
                    rBody.velocity = new Vector2(rBody.velocity.x, -boostSpeed);
                    break;
                case ThrustDirection.Left:
                    rBody.velocity = new Vector2(-boostSpeed, 0f);
                    break;
                case ThrustDirection.Right:
                    rBody.velocity = new Vector2(boostSpeed, 0f);
                    break;
            }
        }

        private void StartThrust()
        {
            thrustDirection = DetermineThrustDirection();
            isThrusting = true;
            gravityScaleBeforeThrust = rBody.gravityScale;
            rBody.gravityScale = 0f;
            ApplyThrustVelocity();
            PlayThrustSound();
            PlayThrustEffect();
            PerfLog.Log(
                $"[ThrusterFlight] 噴射開始 name={gameObject.name} dir={thrustDirection} fuel={fuelSeconds:F2}"
            );
        }

        private ThrustDirection DetermineThrustDirection()
        {
            if (PlayerInputAdapter.IsAimUpPressed())
                return ThrustDirection.Up;
            float horizontal = PlayerInputAdapter.GetMoveHorizontal();
            if (horizontal > 0.01f)
                return ThrustDirection.Right;
            if (horizontal < -0.01f)
                return ThrustDirection.Left;
            if (PlayerInputAdapter.IsAimDownPressed())
                return ThrustDirection.Down;
            // ニュートラルは上方噴射
            return ThrustDirection.Up;
        }

        private bool CanActivate()
        {
            return IsUnlocked()
                && fuelSeconds > 0f
                && !IsGround()
                && !IsWaterBlocking()
                && !IsKabeBlocking();
        }

        private bool ShouldEndThrust()
        {
            return isThrusting
                && (
                    fuelSeconds <= 0f
                    || PlayerInputAdapter.WasJumpReleased(jumpButton)
                    || !PlayerInputAdapter.IsJumpHeld(jumpButton)
                    || IsWaterBlocking()
                    || IsKabeBlocking()
                );
        }

        public bool IsUnlocked()
        {
            if (flagManager == null)
            {
                flagManager = FlagManager.FindInScene();
            }
            return flagManager != null && flagManager.GetFlag(Flag.VLNarukamiJoined);
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

        // キャラ切り替え時の燃料引き継ぎ用
        public void SetFuelSeconds(float value)
        {
            fuelSeconds = Mathf.Clamp(value, 0f, maxFuelSeconds);
        }

        private void OnLanded()
        {
            RecoverFuel();
            StopThrust();
        }

        // 噴射方向の速度を半減して噴射終了(下方噴射は原作準拠で半減しない)
        private void EndThrustWithSlowdown()
        {
            if (!isThrusting)
                return;

            switch (thrustDirection)
            {
                case ThrustDirection.Up:
                    rBody.velocity = new Vector2(rBody.velocity.x, rBody.velocity.y / 2f);
                    break;
                case ThrustDirection.Left:
                case ThrustDirection.Right:
                    rBody.velocity = new Vector2(rBody.velocity.x / 2f, rBody.velocity.y);
                    break;
            }
            PerfLog.Log(
                $"[ThrusterFlight] 噴射終了 name={gameObject.name} dir={thrustDirection} fuel={fuelSeconds:F2}"
            );
            StopThrust();
        }

        // 速度に触らず噴射状態だけ解除する(停止・着地・非アクティブ化用)
        private void StopThrust()
        {
            if (isThrusting)
            {
                rBody.gravityScale = gravityScaleBeforeThrust;
            }
            isThrusting = false;
            StopThrustEffect();
        }

        private void PlayThrustSound()
        {
            if (audioSource != null && thrustSe != null)
            {
                audioSource.PlayOneShot(thrustSe, thrustSeVolume);
            }
        }

        private void PlayThrustEffect()
        {
            if (thrustEffect != null)
            {
                thrustEffect.Play();
            }
        }

        private void StopThrustEffect()
        {
            if (thrustEffect != null)
            {
                thrustEffect.Stop();
            }
        }

        public void OnWaterEnter()
        {
            isInWater = true;
            StopThrust();
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
