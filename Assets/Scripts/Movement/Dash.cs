using System.Drawing.Drawing2D;
using Fungus;
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
        // 山荘
        GameObject dashObject;

        void Awake()
        {
            rBody = GetComponent<Rigidbody2D>();
            mover = GetComponent<Mover>();
            // 残像を用意
            dashObject = new GameObject("DashSprite");
            SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();
            dashObject.AddComponent<SpriteRenderer>().sprite = playerSprite.sprite;
            // 非表示にしておく
            dashObject.SetActive(false);
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
                    // 一定間隔で残像を生成
                    if (Time.frameCount % 40 == 0)
                    {
                        // 残像をつける
                        SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();
                        dashObject.GetComponent<SpriteRenderer>().sprite = playerSprite.sprite;
                        dashObject.transform.localScale = transform.localScale;
                        // 透明にする
                        Color color = dashObject.GetComponent<SpriteRenderer>().color;
                        color.a = 0.4f;
                        dashObject.GetComponent<SpriteRenderer>().color = color;
                        GameObject dashSprite = Instantiate(dashObject, transform.position, transform.rotation);
                        dashSprite.SetActive(true);
                        Destroy(dashSprite, 0.3f);
                    }
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
