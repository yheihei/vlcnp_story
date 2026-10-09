using UnityEngine;

namespace VLCNP.Movie
{
    /**
     * カメラ区画(CameraRoom)をまとめる親。シーン開始時に追従カメラ(CMCamera)へ CameraRoomConfiner を付け、設定を渡す。
     * Core.prefab には付けず、区画を置いたシーンだけで使う。
     * この物ごと無効にすると区画がすべて外れ、いつもの追従へなめらかに戻る。
     */
    public class CameraRoomGroup : MonoBehaviour
    {
        [SerializeField, Min(0f), Tooltip("区画が変わったとき、前の位置とのずれをほぼ消すまでの時間(秒)")]
        float blendTime = 0.4f;

        [SerializeField, Min(0f), Tooltip("区画の中で、操作キャラの中心から画面の上下の端までに残す長さ")]
        float edgeMargin = 1.2f;

        void Start()
        {
            GameObject cmCamera = GameObject.FindWithTag("CMCamera");
            if (cmCamera == null)
            {
                Debug.LogWarning($"[{nameof(CameraRoomGroup)}] CMCamera が見つからないため、カメラ区画を使えません。", this);
                return;
            }
            if (!cmCamera.TryGetComponent(out CameraRoomConfiner confiner))
                confiner = cmCamera.AddComponent<CameraRoomConfiner>();
            confiner.Setup(blendTime, edgeMargin);
        }
    }
}
