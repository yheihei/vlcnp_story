using Unity.VisualScripting;
using UnityEngine;
using VLCNP.Control;

namespace VLCNP.Combat.EnemyAction
{
    public class RangeDetect : MonoBehaviour, IDetect
    {
        [SerializeField]
        float enemyDetectionRange = 12f;

        GameObject player;

        PartyCongroller partyCongroller;

        private void Awake()
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        void OnEnable()
        {
            // PartyタグのオブジェクトからPartyCongrollerを取得
            partyCongroller = GameObject
                .FindGameObjectWithTag("Party")
                ?.GetComponent<PartyCongroller>();
            if (partyCongroller == null)
                return;
            partyCongroller.OnChangeCharacter += SetPlayer;
        }

        void OnDisable()
        {
            partyCongroller = GameObject
                .FindGameObjectWithTag("Party")
                ?.GetComponent<PartyCongroller>();
            if (partyCongroller == null)
                return;
            partyCongroller.OnChangeCharacter -= SetPlayer;
        }

        private void SetPlayer(GameObject _player)
        {
            player = _player;
        }

        public bool IsDetect()
        {
            // playerとの距離を出す
            if (player == null)
                return false;
            float distance = Vector2.Distance(player.transform.position, transform.position);
            return distance < enemyDetectionRange;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, enemyDetectionRange);
        }
    }
}
