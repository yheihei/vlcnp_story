using System;
using Cinemachine;
using UnityEngine;
using VLCNP.Attributes;

namespace VLCNP.Movie
{
    /**
     * Health の被弾・死亡に合わせて、同じオブジェクトの CinemachineImpulseSource でカメラを揺らす(#661)。
     *
     * - 被弾: ダメージ 1 以上のときだけ揺らす。ガード(ダメージ 0)では揺らさない
     * - 死亡: 死亡が確定した瞬間に一度だけ揺らす(ボス撃破用)
     * - 停止中(イベントシーン中)は揺らさない
     * 揺れの振幅・長さ・形は CinemachineImpulseSource 側(Default Velocity と Impulse Definition)で決める。
     * 受け手は Core の CMCamera に付いた CinemachineImpulseListener。
     */
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class HealthCameraShake : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("ダメージ 1 以上の被弾で揺らす(プレイヤー用)")]
        private bool shakeOnDamage = false;

        [SerializeField]
        [Tooltip("死亡が確定したときに一度だけ揺らす(ボス撃破用)")]
        private bool shakeOnDie = false;

        private Health health;
        private CinemachineImpulseSource impulseSource;

        private void Awake()
        {
            health = GetComponent<Health>();
            impulseSource = GetComponent<CinemachineImpulseSource>();
            // ボス撃破の演出はスローモーション中に走るので、揺れの長さは実時間で数える
            CinemachineImpulseManager.Instance.IgnoreTimeScale = true;
        }

        private void OnEnable()
        {
            health.takeDamage.AddListener(OnTakeDamage);
            health.onDieStarted += OnDieStarted;
        }

        private void OnDisable()
        {
            health.takeDamage.RemoveListener(OnTakeDamage);
            health.onDieStarted -= OnDieStarted;
        }

        private void OnTakeDamage(float damage)
        {
            if (!shakeOnDamage)
                return;
            if (damage <= 0f)
                return;
            Shake();
        }

        private void OnDieStarted()
        {
            if (!shakeOnDie)
                return;
            Shake();
        }

        private void Shake()
        {
            if (health.IsStopped)
                return;
            if (impulseSource == null)
                return;
            impulseSource.GenerateImpulse();
        }
    }
}
