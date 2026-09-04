using UnityEngine;

namespace VLCNP.Movie
{
    /**
     * イベント演出用に Rigidbody2D の物理を切り替える。
     * Fungus の InvokeMethod から呼ぶ(プロパティは InvokeMethod で直接触れないため)。
     */
    [RequireComponent(typeof(Rigidbody2D))]
    public class Rigidbody2DSwitch : MonoBehaviour
    {
        // true で Kinematic にして速度を止める(以降は Transform 移動で動かす)。false で Dynamic に戻す
        public void SetKinematic(bool isKinematic)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.bodyType = isKinematic ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
        }

        public void SetGravityScale(float scale)
        {
            GetComponent<Rigidbody2D>().gravityScale = scale;
        }
    }
}
