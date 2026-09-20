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
     * - 敵への命中: Health から共有の短い揺れを発火し、同時ヒットの重複を抑える
     * - 停止中(イベントシーン中)は揺らさない
     * 被弾・死亡の振幅・長さ・形は CinemachineImpulseSource 側で決める。敵への命中は本クラスの定数で調整する。
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

        // 通常命中はプレイヤー被弾の約1/8の振幅。全敵で間隔を共有し、揺れを重ねない。
        private const float EnemyHitDuration = 0.06f;
        private const float EnemyHitInterval = 0.08f;
        private static readonly Vector3 EnemyHitVelocity = new Vector3(0.0125f, -0.01375f, 0f);
        private static CinemachineImpulseDefinition enemyHitImpulse;
        private static float nextEnemyHitTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetEnemyHitShake()
        {
            enemyHitImpulse = null;
            nextEnemyHitTime = 0f;
        }

        /** 実ダメージが確定した敵への命中を揺らす。敵ごとのコンポーネント追加は不要。 */
        internal static void ShakeEnemyHit(Health target, bool isLeft)
        {
            if (!target.CompareTag("Enemy") || Time.unscaledTime < nextEnemyHitTime)
                return;

            // ボスへのとどめには既存の撃破用の揺れだけを使う。
            if (target.GetHealthPoints() <= 0f
                && target.TryGetComponent(out HealthCameraShake deathShake)
                && deathShake.isActiveAndEnabled
                && deathShake.shakeOnDie)
                return;

            if (enemyHitImpulse == null)
            {
                enemyHitImpulse = new CinemachineImpulseDefinition
                {
                    m_ImpulseChannel = 1,
                    m_ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform,
                    m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump,
                    m_ImpulseDuration = EnemyHitDuration,
                };
            }

            Vector3 velocity = EnemyHitVelocity;
            if (isLeft)
                velocity.x = -velocity.x;

            CinemachineImpulseManager.Instance.IgnoreTimeScale = true;
            enemyHitImpulse.CreateEvent(target.transform.position, velocity);
            nextEnemyHitTime = Time.unscaledTime + EnemyHitInterval;
        }

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
