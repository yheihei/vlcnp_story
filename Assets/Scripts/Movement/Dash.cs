using UnityEngine;

namespace VLCNP.Movement
{
    public class Dash : MonoBehaviour
    {
        [SerializeField] private float dashPower = 10f;
        [SerializeField] private float dashDuration = 0.5f;
        private Rigidbody2D rBody;
        private bool isDashing;

        public bool IsDashing { get => isDashing; }
        private float dashTimeLeft;
        [SerializeField] Leg leg;
        Mover mover;

        void Awake()
        {
            rBody = GetComponent<Rigidbody2D>();
            mover = GetComponent<Mover>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.X) && !isDashing && leg.IsGround)
            {
                StartDash();
            }

            if (isDashing)
            {
                if (dashTimeLeft > 0)
                {
                    dashTimeLeft -= Time.deltaTime;
                }
                // ダッシュ時間が0かつ、接地していればダッシュ終了
                else if (leg.IsGround)
                {
                    isDashing = false;
                }
            }
        }

        void FixedUpdate()
        {
            if (isDashing)
            {
                float vx = mover.IsLeft ? -1 * dashPower : 1 * dashPower;
                rBody.velocity = new Vector2(vx, rBody.velocity.y);
            }
        }

        private void StartDash()
        {
            isDashing = true;
            dashTimeLeft = dashDuration;
        }
    }
}
