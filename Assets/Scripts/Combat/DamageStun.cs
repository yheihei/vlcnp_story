using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Combat
{
    public class DamageStun : MonoBehaviour
    {
        // Stunの持続時間や強さなどのパラメータを設定可能に
        public float stunDuration = 0.2f;
        public float shakeAmount = 0.1f;

        private bool isStunned = false;
        private bool invalidStun = false;
        private float stunTimer;

        // Stun関数を外部からコールできるように公開
        public void Stun()
        {
            if (!isStunned) // すでにStunned状態でないことを確認
            {
                isStunned = true;
                stunTimer = stunDuration;
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

            while (stunTimer > 0 && !invalidStun)
            {
                transform.position = originalPosition + Random.insideUnitSphere * shakeAmount;

                stunTimer -= Time.deltaTime;
                yield return null; // 1フレーム待つ
            }

            // transform.position = originalPosition; // 元の位置に戻す
            isStunned = false;
        }
    }
}
