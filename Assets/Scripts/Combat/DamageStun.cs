using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Combat
{
    public class DamageStun : MonoBehaviour
    {
        // Stunの持続時間や強さなどのパラメータを設定可能に
        public float stanFrame = 3f;
        public float shakeAmount = 0.15f;

        private bool isStunned = false;
        private bool invalidStun = false;
        private float stunTimer;
        private Rigidbody2D rb;
        private Vector3 originalVelocity;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogError("Rigidbodyが見つかりません");
            }
        }

        // Stun関数を外部からコールできるように公開
        public void Stun()
        {
            if (!isStunned) // すでにStunned状態でないことを確認
            {
                isStunned = true;
                originalVelocity = rb.velocity; // 現在の速度を保存
                StartCoroutine(Shake()); // ブルブル効果開始
            }
        }

        // 外部からstun状態を無効化する
        public void InvalidStan()
        {
            invalidStun = true;
        }

        public void ValidStan()
        {
            invalidStun = false;
        }

        private IEnumerator Shake()
        {
            Vector3 originalPosition = transform.position;

            // stanFrameフレームだけブルブルさせる
            float _stanFrame = stanFrame;
            for (int i = 0; i <= _stanFrame; i++)
            {
                // _stanFrameが奇数のときは-1x * shakeAmount、偶数のときは1x * shakeAmount
                transform.position = new Vector3(originalPosition.x + (i % 2 == 0 ? 1 : -1) * shakeAmount, originalPosition.y, originalPosition.z);
                yield return null;
            }

            transform.position = originalPosition; // 元の位置に戻す
            rb.velocity = originalVelocity; // 保存した速度を復元
            isStunned = false;
        }
    }
}
