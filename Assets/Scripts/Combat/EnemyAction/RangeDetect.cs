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

        GameObject player;

        PartyCongroller partyCongroller;

        const string IsUndetectedParam = "isUndetected";
        bool isWaitingDetectConfirm = false;

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
            {
                SetIsUndetected(true);
                isWaitingDetectConfirm = false;
            }
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
            // 発見時のアニメーションをOnする場合 Animation EventでOnUndetectedAnimationFinishedを呼び出すこと
            animator.SetBool(IsUndetectedParam, value);
        }

        public bool IsDetect()
        {
            if (player == null)
                return false;
            float distance = Vector2.Distance(player.transform.position, transform.position);
            // 一度発見状態になったら常に発見状態を維持する
            if (isDetected)
            {
                SetIsUndetected(false);
                return true;
            }
            // 発見アニメ待機中は検出しない
            if (isWaitingDetectConfirm)
            {
                return false;
            }
            if (enableUndetectedAnimation)
            {
                if (distance < enemyDetectionRange)
                {
                    isWaitingDetectConfirm = true;
                    SetIsUndetected(false);
                }
                else
                {
                    SetIsUndetected(true);
                }
                return false;
            }
            // 未発見状態でプレイヤーが発見距離内にいるかどうかを返す
            isDetected = distance < enemyDetectionRange;
            SetIsUndetected(!isDetected);
            return isDetected;
        }

        // AnimationからEventで必ず呼び出すこと
        public void OnUndetectedAnimationFinished()
        {
            if (!enableUndetectedAnimation)
                return;
            if (!isWaitingDetectConfirm)
                return;
            isWaitingDetectConfirm = false;
            isDetected = true;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, enemyDetectionRange);
        }
    }
}
