using Cinemachine;
using UnityEngine;

namespace VLCNP.Movie
{
    /**
     * 追従カメラ(CMCamera)の縦位置を、操作キャラのいるカメラ区画(CameraRoom)の決まりに合わせる Cinemachine の拡張。
     * 区画は毎フレーム操作キャラの位置で選ぶ。今の区画の範囲にいる間は変えないので、重なった所で切り替わり続けない。
     * 区画が変わったら、直前の位置と速さから続けて、新しい区画の高さとのずれを blendTime ほどで消す。
     * 区画の中では、操作キャラの中心を画面の上下の端から edgeMargin より内側に置くことを、区画の高さやつなぎより優先する。
     * 穴へ落ちるときや区画のつなぎの最中も、キャラが画面から出ない。
     * どの区画にも入っていなければ、いつもの追従のまま。カメラが位置を引き継がないとき(シーン開始など)はその場で合わせる。
     * CameraRoomGroup がシーン開始時に付けて設定を渡す。
     */
    public class CameraRoomConfiner : CinemachineExtension
    {
        [SerializeField, Min(0f), Tooltip("区画が変わったとき、前の位置とのずれをほぼ消すまでの時間(秒)")]
        float blendTime = 0.4f;

        [SerializeField, Min(0f), Tooltip("区画の中で、操作キャラの中心から画面の上下の端までに残す長さ")]
        float edgeMargin = 1.2f;

        CameraRoom currentRoom;
        bool hasPrevious;
        float previousOutputY;
        float previousOutputVelocity;
        float previousFollowY;
        float previousTargetY;
        float previousHalfHeight;
        // 新しい区画の高さとのずれ。0 へ近づけてつなぐ
        float offset;
        float offsetVelocity;

        public CameraRoom CurrentRoom => currentRoom;

        public void Setup(float blendTime, float edgeMargin)
        {
            this.blendTime = Mathf.Max(0f, blendTime);
            this.edgeMargin = Mathf.Max(0f, edgeMargin);
        }

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage,
            ref CameraState state,
            float deltaTime
        )
        {
            if (stage != CinemachineCore.Stage.Body)
                return;
            Transform target = vcam.Follow;
            if (target == null)
            {
                hasPrevious = false;
                return;
            }

            Vector3 position = state.CorrectedPosition;
            Vector2 targetPosition = target.position;
            float halfHeight = state.Lens.OrthographicSize;
            bool isReset = !hasPrevious || deltaTime < 0f || !vcam.PreviousStateIsValid;

            CameraRoom room = currentRoom;
            if (room == null || !room.isActiveAndEnabled || !room.Contains(targetPosition))
                room = CameraRoom.Find(targetPosition);
            float roomY = ComputeY(room, position.y, targetPosition.y, halfHeight);

            if (isReset || blendTime <= 0f)
            {
                offset = 0f;
                offsetVelocity = 0f;
            }
            else if (room != currentRoom)
            {
                // 新しい区画に1フレーム前からいたとした高さとの差から、新しい区画の高さの動く速さを出す
                float previousRoomY = ComputeY(room, previousFollowY, previousTargetY, previousHalfHeight);
                float roomVelocity = deltaTime > 0f ? (roomY - previousRoomY) / deltaTime : 0f;
                offset = previousOutputY + previousOutputVelocity * deltaTime - roomY;
                offsetVelocity = previousOutputVelocity - roomVelocity;
            }
            else if (deltaTime > 0f && (offset != 0f || offsetVelocity != 0f))
            {
                offset = Mathf.SmoothDamp(offset, 0f, ref offsetVelocity, blendTime / 3f, Mathf.Infinity, deltaTime);
                if (Mathf.Abs(offset) < 0.001f && Mathf.Abs(offsetVelocity) < 0.01f)
                {
                    offset = 0f;
                    offsetVelocity = 0f;
                }
            }
            currentRoom = room;

            float outputY = roomY + offset;
            if (room != null)
            {
                float margin = Mathf.Min(edgeMargin, halfHeight);
                float limitedY = Mathf.Clamp(outputY, targetPosition.y + margin - halfHeight, targetPosition.y - margin + halfHeight);
                if (limitedY != outputY)
                {
                    // 押さえた分はずれとして持ち、押さえが外れたらなめらかに区画の高さへ戻す
                    offset = limitedY - roomY;
                    offsetVelocity = 0f;
                    outputY = limitedY;
                }
            }
            if (isReset)
                previousOutputVelocity = 0f;
            else if (deltaTime > 0f)
                previousOutputVelocity = (outputY - previousOutputY) / deltaTime;
            previousOutputY = outputY;
            previousFollowY = position.y;
            previousTargetY = targetPosition.y;
            previousHalfHeight = halfHeight;
            hasPrevious = true;

            state.PositionCorrection += new Vector3(0f, outputY - position.y, 0f);
        }

        float ComputeY(CameraRoom room, float followY, float targetY, float halfHeight)
        {
            return room != null ? room.ComputeCameraY(followY, targetY, halfHeight, edgeMargin) : followY;
        }
    }
}
