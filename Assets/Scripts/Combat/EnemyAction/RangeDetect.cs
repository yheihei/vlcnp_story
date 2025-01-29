using Unity.VisualScripting;
using UnityEngine;
using VLCNP.Control;

namespace VLCNP.Combat.EnemyAction
{
    public class RangeDetect : MonoBehaviour, IDetect
    {
        [SerializeField]
        float enemyDetectionRange = 12f;

        // 一度発見状態になったかどうか
        public bool isDetected = false;

        [SerializeField] // 一度発見状態になった後の追跡距離
        float chaseRange = 15f;

        GameObject player;

        PartyCongroller partyCongroller;

        private void Awake()
        {
            player = GameObject.FindGameObjectWithTag("Player");
            InitializePartyController();
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

        public bool IsDetect()
        {
            if (player == null)
                return false;
            float distance = Vector2.Distance(player.transform.position, transform.position);
            // 1度発見状態になったら追跡距離内にいるかどうかを返す
            if (isDetected)
                return distance < chaseRange;
            // 未発見状態でプレイヤーが発見距離内にいるかどうかを返す
            isDetected = distance < enemyDetectionRange;
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
