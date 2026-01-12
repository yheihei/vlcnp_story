using Unity.VisualScripting;
using UnityEngine;
using VLCNP.Control;

namespace VLCNP.Combat.EnemyAction
{
    public class RangeDetect : MonoBehaviour, IDetect
    {
        [SerializeField]
        float enemyDetectionRange = 12f;

        [SerializeField]
        bool enableUndetectedAnimation = false;

        [SerializeField]
        Animator animator;

        // 一度発見状態になったかどうか
        public bool isDetected = false;

        [SerializeField] // 一度発見状態になった後の追跡距離
        float chaseRange = 15f;

        GameObject player;

        PartyCongroller partyCongroller;

        const string IsUndetectedParam = "isUndetected";

        private void Awake()
        {
            player = GameObject.FindGameObjectWithTag("Player");
            InitializePartyController();
            if (animator == null)
                animator = GetComponent<Animator>();
        }

        private void Start()
        {
            if (enableUndetectedAnimation)
                SetIsUndetected(true);
        }

        void OnEnable()
        {
            if (partyCongroller == null)
                return;
            partyCongroller.OnChangeCharacter += SetPlayer;
        }

        void OnDisable()
        {
            if (partyCongroller == null)
                return;
            partyCongroller.OnChangeCharacter -= SetPlayer;
        }

        private void InitializePartyController()
        {
            partyCongroller = GameObject
                .FindGameObjectWithTag("Party")
                ?.GetComponent<PartyCongroller>();
        }

        private void SetPlayer(GameObject _player)
        {
            player = _player;
        }

        private void SetIsUndetected(bool value)
        {
            if (!enableUndetectedAnimation || animator == null)
                return;
            animator.SetBool(IsUndetectedParam, value);
        }

        public bool IsDetect()
        {
            if (player == null)
                return false;
            float distance = Vector2.Distance(player.transform.position, transform.position);
            // 1度発見状態になったら追跡距離内にいるかどうかを返す
            if (isDetected)
            {
                bool inChaseRange = distance < chaseRange;
                SetIsUndetected(!inChaseRange);
                return inChaseRange;
            }
            // 未発見状態でプレイヤーが発見距離内にいるかどうかを返す
            isDetected = distance < enemyDetectionRange;
            SetIsUndetected(!isDetected);
            return isDetected;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, enemyDetectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chaseRange);
        }
    }
}
