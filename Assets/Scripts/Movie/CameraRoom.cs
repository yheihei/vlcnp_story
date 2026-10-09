using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Movie
{
    /**
     * カメラ区画。操作キャラがこの区画の範囲にいる間、追従カメラ(CMCamera)の上下の動きをこの区画の決まりに合わせる。
     * 横方向は扱わない(シーンの CameraConfineArea のまま)。
     * 範囲と映す下端・上端は、この物の位置からの値で持つ。物を動かすと区画ごと動く。
     * 範囲はコライダーにしない。トリガーにすると、出入りで接地を外す処理(MoveHorizontal など)に触れるため。
     * 判定と補正は追従カメラに付く CameraRoomConfiner が行う(CameraRoomGroup が付ける)。
     */
    public class CameraRoom : MonoBehaviour
    {
        public enum CameraMode
        {
            // 映す下端を画面の下端に合わせて止める。操作キャラが画面の上へ出そうなときだけ、映す上端まで押し上げる
            Fixed,
            // 映す下端と上端の間で、いつもの追従どおりに操作キャラを追う
            Follow,
        }

        static readonly List<CameraRoom> rooms = new List<CameraRoom>();

        [SerializeField]
        CameraMode mode = CameraMode.Fixed;

        [SerializeField, Tooltip("この区画で画面に映してよい下端の高さ")]
        float viewBottom;

        [SerializeField, Tooltip("この区画で画面に映してよい上端の高さ。固定のときは、操作キャラが画面の上へ出そうならここまで押し上げる")]
        float viewTop = 10f;

        [SerializeField, Tooltip("操作キャラの中心がこの中にいると、この区画になる。複数の矩形を合わせて使える")]
        Rect[] areas = { new Rect(0f, 0f, 10f, 10f) };

        [SerializeField, Tooltip("重なった区画では大きい方を使う。縦穴の手前で下をのぞく区画のように、層の区画の上に重ねて置くときに上げる")]
        int priority;

        public CameraMode Mode => mode;
        public int Priority => priority;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetRooms()
        {
            rooms.Clear();
        }

        void OnEnable()
        {
            rooms.Add(this);
        }

        void OnDisable()
        {
            rooms.Remove(this);
        }

        /** point を範囲に含む区画。重なっていれば優先度の大きい方、同じなら先に有効になった方。どこにも入らなければ null */
        public static CameraRoom Find(Vector2 point)
        {
            CameraRoom found = null;
            foreach (CameraRoom room in rooms)
            {
                if ((found == null || room.priority > found.priority) && room.Contains(point))
                    found = room;
            }
            return found;
        }

        public bool Contains(Vector2 point)
        {
            Vector2 local = point - (Vector2)transform.position;
            foreach (Rect area in areas)
            {
                if (area.Contains(local))
                    return true;
            }
            return false;
        }

        /**
         * この区画でのカメラ中心の高さ。
         * followY は追従カメラが出した高さ、targetY は操作キャラの中心の高さ、halfHeight は画面の高さの半分。
         * edgeMargin は、固定で押し上げるときに操作キャラの中心から画面の上端までに残す長さ。
         * 映す下端と上端の差が画面より狭ければ下端に合わせる(カメラを引いても床が画面の下端に残る)。
         */
        public float ComputeCameraY(float followY, float targetY, float halfHeight, float edgeMargin)
        {
            float origin = transform.position.y;
            float min = origin + viewBottom + halfHeight;
            float max = Mathf.Max(min, origin + viewTop - halfHeight);
            float y = mode == CameraMode.Fixed ? targetY + edgeMargin - halfHeight : followY;
            return Mathf.Clamp(y, min, max);
        }

        void OnDrawGizmos()
        {
            DrawGizmos(false);
        }

        void OnDrawGizmosSelected()
        {
            DrawGizmos(true);
        }

        // 固定は青、追従は橙。選ぶと塗りと映す下端・上端の線を出す
        void DrawGizmos(bool selected)
        {
            if (areas == null || areas.Length == 0)
                return;
            Color color = mode == CameraMode.Fixed ? new Color(0.23f, 0.63f, 1f) : new Color(1f, 0.6f, 0.18f);
            Vector3 origin = transform.position;
            float xMin = float.MaxValue;
            float xMax = float.MinValue;
            foreach (Rect area in areas)
            {
                Vector3 center = origin + (Vector3)area.center;
                Vector3 size = new Vector3(area.width, area.height, 0f);
                if (selected)
                {
                    Gizmos.color = new Color(color.r, color.g, color.b, 0.15f);
                    Gizmos.DrawCube(center, size);
                }
                Gizmos.color = new Color(color.r, color.g, color.b, selected ? 1f : 0.4f);
                Gizmos.DrawWireCube(center, size);
                xMin = Mathf.Min(xMin, origin.x + area.xMin);
                xMax = Mathf.Max(xMax, origin.x + area.xMax);
            }
            if (!selected)
                return;
            Gizmos.color = color;
            Gizmos.DrawLine(new Vector3(xMin, origin.y + viewBottom), new Vector3(xMax, origin.y + viewBottom));
            Gizmos.DrawLine(new Vector3(xMin, origin.y + viewTop), new Vector3(xMax, origin.y + viewTop));
        }
    }
}
