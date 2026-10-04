using System.Collections;
using Cinemachine;
using UnityEngine;

namespace VLCNP.Movie
{
    /**
     * イベント中だけ追従カメラ(CMCamera)を少し引き、カメラの移動範囲をイベントの場所に絞る。
     * 戻すときは引く前の大きさと移動範囲に戻す。戻しきる前にこの物が消える(シーンを離れる)ときは、その場で戻す。
     */
    public class EventCameraZoom : MonoBehaviour
    {
        [SerializeField, Min(0.1f), Tooltip("引いたときのカメラの大きさ(通常のカメラは5)")]
        float zoomedOrthographicSize = 6.5f;

        [SerializeField, Min(0f)]
        float zoomTime = 1f;

        [SerializeField, Tooltip("引いている間のカメラの移動範囲。空なら変えない")]
        Collider2D confineShape;

        CinemachineVirtualCamera virtualCamera;
        CinemachineConfiner confiner;
        Coroutine zoomRoutine;
        bool isZoomed;

        // 引く前の状態。戻しきるまで持っておき、戻す途中でもう一度引いても元の値を失わない
        bool hasDefault;
        float defaultOrthographicSize;
        Collider2D defaultConfineShape;

        public bool IsZoomed => isZoomed;

        public void ZoomOut()
        {
            if (isZoomed || !FindCamera())
                return;
            if (!hasDefault)
            {
                defaultOrthographicSize = virtualCamera.m_Lens.OrthographicSize;
                defaultConfineShape = confiner != null ? confiner.m_BoundingShape2D : null;
                hasDefault = true;
            }
            isZoomed = true;
            if (confiner != null && confineShape != null)
                SetConfineShape(confineShape);
            StartZoom(zoomedOrthographicSize, false);
        }

        public void Restore()
        {
            if (!isZoomed)
                return;
            isZoomed = false;
            if (virtualCamera == null)
            {
                hasDefault = false;
                return;
            }
            if (confiner != null && confineShape != null)
                SetConfineShape(defaultConfineShape);
            StartZoom(defaultOrthographicSize, true);
        }

        void OnDisable()
        {
            StopZoom();
            if (hasDefault && virtualCamera != null)
            {
                SetOrthographicSize(defaultOrthographicSize);
                if (confiner != null && confineShape != null && confiner.m_BoundingShape2D == confineShape)
                    SetConfineShape(defaultConfineShape);
            }
            isZoomed = false;
            hasDefault = false;
        }

        bool FindCamera()
        {
            if (virtualCamera != null)
                return true;
            GameObject cmCamera = GameObject.FindWithTag("CMCamera");
            if (cmCamera == null || !cmCamera.TryGetComponent(out virtualCamera))
            {
                Debug.LogWarning($"[{nameof(EventCameraZoom)}] CMCamera が見つからないため、カメラを引けません。", this);
                return false;
            }
            confiner = cmCamera.GetComponent<CinemachineConfiner>();
            return true;
        }

        void StartZoom(float target, bool isRestoring)
        {
            StopZoom();
            if (zoomTime <= 0f || !isActiveAndEnabled)
            {
                FinishZoom(target, isRestoring);
                return;
            }
            zoomRoutine = StartCoroutine(ZoomRoutine(target, isRestoring));
        }

        void StopZoom()
        {
            if (zoomRoutine == null)
                return;
            StopCoroutine(zoomRoutine);
            zoomRoutine = null;
        }

        // 時間を止める演出の間も止まらないよう、実時間で動かす
        IEnumerator ZoomRoutine(float target, bool isRestoring)
        {
            float start = virtualCamera.m_Lens.OrthographicSize;
            for (float t = 0f; t < zoomTime; t += Time.unscaledDeltaTime)
            {
                if (virtualCamera == null)
                    yield break;
                SetOrthographicSize(Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t / zoomTime)));
                yield return null;
            }
            zoomRoutine = null;
            FinishZoom(target, isRestoring);
        }

        void FinishZoom(float target, bool isRestoring)
        {
            if (virtualCamera != null)
                SetOrthographicSize(target);
            if (isRestoring)
                hasDefault = false;
        }

        void SetOrthographicSize(float size)
        {
            virtualCamera.m_Lens.OrthographicSize = size;
        }

        void SetConfineShape(Collider2D shape)
        {
            confiner.m_BoundingShape2D = shape;
            confiner.InvalidatePathCache();
        }
    }
}
