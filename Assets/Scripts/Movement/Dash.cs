using UnityEngine;

namespace VLCNP.Movement
{
    public class Dash : MonoBehaviour
    {
        [SerializeField] private float dashPower = 10f;
        [SerializeField] private float dashDuration = 0.3f;
        private Rigidbody2D rBody;
        private bool isDashing;
        public bool IsDashing { get => isDashing; }

        private float dashTimeLeft;
        private SpriteRenderer playerSprite;
        private GameObject dashObject;

        [SerializeField] private Leg leg;
        private Mover mover;
        Animator animator;
        KabeKickEffectController kabeKickEffectController;

        void Awake()
        {
            InitializeComponents();
            PrepareDashEffect();
        }

        void Update()
        {
            HandleInput();
            HandleDashEffect();
            CheckDashEndCondition();
        }

        void FixedUpdate()
        {
            ApplyDashMovement();
        }

        private void InitializeComponents()
        {
            rBody = GetComponent<Rigidbody2D>();
            mover = GetComponent<Mover>();
            playerSprite = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            // 子コンポーネントからKabekiKickEffectControllerを取得
            kabeKickEffectController = GetComponentInChildren<KabeKickEffectController>();
            print(kabeKickEffectController);
        }

        private void PrepareDashEffect()
        {
            dashObject = new GameObject("DashSprite");
            SpriteRenderer dashSpriteRenderer = dashObject.AddComponent<SpriteRenderer>();
            dashSpriteRenderer.sprite = playerSprite.sprite;
            dashSpriteRenderer.sortingLayerName = "Player";
            // sorting orderを-1にする
            dashSpriteRenderer.sortingOrder = -1;
            dashObject.SetActive(false);
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.X) && !isDashing && leg.IsGround)
            {
                StartDash();
            }
        }

        private void HandleDashEffect()
        {
            if (isDashing)
            {
                dashTimeLeft -= Time.deltaTime;
                if (Time.frameCount % 40 == 0)
                {
                    CreateDashEffect();
                }
            }
        }

        private void CheckDashEndCondition()
        {
            if ((isDashing && dashTimeLeft <= 0 && leg.IsGround) || kabeKickEffectController.IsKabekick())
            {
                SetIsDashing(false);
            }
        }

        private void ApplyDashMovement()
        {
            if (isDashing)
            {
                float vx = mover.IsLeft ? -dashPower : dashPower;
                rBody.velocity = new Vector2(vx, rBody.velocity.y);
            }
        }

        private void StartDash()
        {
            SetIsDashing(true);
            dashTimeLeft = dashDuration;
        }

        private void SetIsDashing(bool value)
        {
            isDashing = value;
            animator.SetBool("isDash", value);
        }

        private void CreateDashEffect()
        {
            dashObject.GetComponent<SpriteRenderer>().sprite = playerSprite.sprite;
            dashObject.transform.localScale = transform.localScale;

            Color color = dashObject.GetComponent<SpriteRenderer>().color;
            color.a = 0.6f;
            // 少し青みを帯びた色にする
            color.r = 0.5f;
            color.g = 0.5f;
            dashObject.GetComponent<SpriteRenderer>().color = color;

            GameObject dashSprite = Instantiate(dashObject, transform.position, transform.rotation);
            dashSprite.SetActive(true);
            Destroy(dashSprite, 0.3f);
        }
    }
}
