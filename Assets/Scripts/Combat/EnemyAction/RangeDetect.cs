using Unity.VisualScripting;
using UnityEngine;
using VLCNP.Control;

namespace VLCNP.Combat.EnemyAction
{
    public class RangeDetect : MonoBehaviour, IDetect
    {
        [SerializeField]
        float enemyDetectionRange = 12f;

        // 1度発見状態になったらずっと発見状態にするか
        [SerializeField]
        bool isPermanentlyDiscovered = true;

        // 一度発見状態になったかどうか
        public bool isDetected = false;

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
            // 1度発見状態になったらずっと発見状態
            if (isPermanentlyDiscovered && isDetected)
                return true;
            if (player == null)
                return false;
            float distance = Vector2.Distance(player.transform.position, transform.position);
            isDetected = distance < enemyDetectionRange;
            return isDetected;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, enemyDetectionRange);
        }
    }
}
