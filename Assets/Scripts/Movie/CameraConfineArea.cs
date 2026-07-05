using Cinemachine;
using UnityEngine;

namespace VLCNP.Movie
{
    // ステージごとのカメラ移動可能範囲をCMCameraのConfinerに適用する
    [RequireComponent(typeof(PolygonCollider2D))]
    public class CameraConfineArea : MonoBehaviour
    {
        void Start()
        {
            GameObject cmCamera = GameObject.FindWithTag("CMCamera");
            if (cmCamera == null)
            {
                Debug.LogError("CMCamera is not found.");
                return;
            }
            CinemachineConfiner confiner = cmCamera.GetComponent<CinemachineConfiner>();
            if (confiner == null)
            {
                Debug.LogError("CinemachineConfiner is not found on CMCamera.");
                return;
            }
            confiner.m_ConfineMode = CinemachineConfiner.Mode.Confine2D;
            confiner.m_BoundingShape2D = GetComponent<PolygonCollider2D>();
            confiner.InvalidatePathCache();
        }
    }
}
