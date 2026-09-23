using TNRD;
using UnityEngine;
using VLCNP.Core;

namespace VLCNP.Combat.EnemyAction
{
    /**
     * プレイヤーが縄張りの左端より右にいる間だけ、内側の検知で追跡させる。
     * プレイヤーが縄張りの外に出たら、初期位置へゆっくり泳いで戻る
     */
    public class TerritoryDetect : MonoBehaviour, IDetect, IStoppable
    {
        private static readonly int VxHash = Animator.StringToHash("vx");

        [Header("縄張りの中で使う検知。未設定なら縄張りの中では常に追跡する")]
        [SerializeField]
        SerializableInterface<IDetect> detect = null;

        [Header("この位置より右にプレイヤーがいる間だけ追跡する")]
        [SerializeField]
        Transform territoryLeftEdge = null;

        [Header("初期位置へ戻る速さ")]
        [SerializeField]
        float returnSpeed = 2f;

        [SerializeField]
        float arriveDistance = 0.1f;

        bool isStopped = false;
        public bool IsStopped
        {
            get => isStopped;
            set => isStopped = value;
        }

        Rigidbody2D rbody;
        Animator animator;
        Transform cachedTransform;
        Transform playerTransform;
        Vector2 homePosition;
        bool isHomeFacingLeft;
        bool isReturning = false;

        private void Awake()
        {
            rbody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            cachedTransform = transform;
            homePosition = rbody.position;
            isHomeFacingLeft = cachedTransform.localScale.x > 0;
        }

        public bool IsDetect()
        {
            if (!IsPlayerInTerritory())
                return false;
            if (detect == null || detect.Value == null)
                return true;
            return detect.Value.IsDetect();
        }

        private void FixedUpdate()
        {
            if (isStopped || IsPlayerInTerritory())
            {
                if (isReturning)
                    EndReturn();
                return;
            }
            ReturnHome();
        }

        private void ReturnHome()
        {
            Vector2 toHome = homePosition - rbody.position;
            if (toHome.magnitude <= arriveDistance)
            {
                // 着いたら元の向きで待つ。泳ぎの残りの速度もここで止める
                if (isReturning)
                {
                    EndReturn();
                    SetFacingLeft(isHomeFacingLeft);
                }
                rbody.linearVelocity = Vector2.zero;
                return;
            }
            isReturning = true;
            rbody.linearVelocity = toHome.normalized * returnSpeed;
            if (Mathf.Abs(toHome.x) > arriveDistance)
                SetFacingLeft(toHome.x < 0);
            animator?.SetFloat(VxHash, returnSpeed);
        }

        private void EndReturn()
        {
            isReturning = false;
            rbody.linearVelocity = Vector2.zero;
            animator?.SetFloat(VxHash, 0);
        }

        private bool IsPlayerInTerritory()
        {
            if (territoryLeftEdge == null)
                return true;
            if (!TryGetPlayerTransform(out Transform player))
                return false;
            return player.position.x > territoryLeftEdge.position.x;
        }

        // 元絵は左向き。正の X scale で左を向く
        private void SetFacingLeft(bool isLeft)
        {
            Vector3 localScale = cachedTransform.localScale;
            float x = Mathf.Abs(localScale.x);
            cachedTransform.localScale = new Vector3(isLeft ? x : -x, localScale.y, localScale.z);
        }

        private bool TryGetPlayerTransform(out Transform player)
        {
            if (
                playerTransform == null
                || !playerTransform.gameObject.activeInHierarchy
                || !playerTransform.CompareTag("Player")
            )
            {
                GameObject playerObject = GameObject.FindWithTag("Player");
                playerTransform = playerObject != null ? playerObject.transform : null;
            }

            player = playerTransform;
            return player != null;
        }

        private void OnDrawGizmosSelected()
        {
            if (territoryLeftEdge == null)
                return;
            Gizmos.color = Color.red;
            Vector3 edge = territoryLeftEdge.position;
            Gizmos.DrawLine(edge + Vector3.up * 10f, edge + Vector3.down * 10f);
        }
    }
}
