using UnityEngine;

namespace VLCNP.Effects
{
    /**
     * 環境粒子の発生源をメインカメラの XY に追従させる。
     * 粒子自体はワールド空間でシミュレーションするので、カメラが動いても粒子は画面に張り付かない。
     * 追従率を 1 より小さくすると、カメラより遅れて動く(粒子の Custom シミュレーション空間に使うと遠景のような視差になる)。
     * BackGroundLoop(1000)と同様にカメラ更新の後に動かす。
     */
    [DefaultExecutionOrder(1001)]
    public class FollowMainCamera : MonoBehaviour
    {
        [SerializeField, Header("カメラからのずらし量")]
        private Vector2 offset = Vector2.zero;

        [SerializeField, Header("カメラの動きに付いていく割合(1 でカメラと同じ位置)")]
        private Vector2 followRatio = Vector2.one;

        private Transform mainCameraTransform;

        private void LateUpdate()
        {
            if (mainCameraTransform == null)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera == null) return;
                mainCameraTransform = mainCamera.transform;
            }

            Vector3 cameraPosition = mainCameraTransform.position;
            Vector3 current = transform.position;
            transform.position = new Vector3(
                cameraPosition.x * followRatio.x + offset.x,
                cameraPosition.y * followRatio.y + offset.y,
                current.z);
        }
    }
}
