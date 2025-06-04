using UnityEngine;
using Core.Status;
using System.Collections;

namespace Projectiles.StatusEffects
{
    /// <summary>
    /// 麻痺状態効果を管理するScriptableObject
    /// ターゲットの移動速度を指定した倍率で減少させ、一定時間後に回復する
    /// </summary>
    [CreateAssetMenu(fileName = "New Paralysis Effect", menuName = "Projectile Status Effects/Paralysis")]
    public class ParalysisStatusEffect : ScriptableObject, IProjectileStatusEffect
    {
        [Header("麻痺効果設定")]
        [SerializeField]
        [Tooltip("速度の倍率（0.5で半分の速度）")]
        [Range(0.1f, 1.0f)]
        private float speedMultiplier = 0.5f;

        [SerializeField]
        [Tooltip("効果の継続時間（秒）")]
        [Range(1f, 30f)]
        private float duration = 5f;

        [SerializeField]
        [Tooltip("エフェクトプレファブ（オプション）")]
        private GameObject effectPrefab = null;

        public string EffectName => "麻痺";

        public void ApplyEffect(GameObject target)
        {
            if (target == null)
            {
                Debug.LogWarning("ParalysisStatusEffect: ターゲットがnullです");
                return;
            }

            // EnemyStatusManagerとの連携を確認
            EnemyStatusManager statusManager = target.GetComponent<EnemyStatusManager>();
            if (statusManager != null && statusManager.HasStatusEffect(EffectName))
            {
                return;
            }

            // 既存の麻痺状態コンポーネントを確認
            ParalysisStatusController existingController = target.GetComponent<ParalysisStatusController>();
            if (existingController != null && existingController.IsParalyzed)
            {
                return;
            }

            // SpeedModifierコンポーネントを取得
            SpeedModifier speedModifier = target.GetComponent<SpeedModifier>();
            if (speedModifier == null)
            {
                Debug.LogWarning($"ParalysisStatusEffect: {target.name} にSpeedModifierコンポーネントが見つかりません。麻痺効果を適用できません");
                return;
            }

            // 麻痺状態コントローラーが存在しない場合は追加
            if (existingController == null)
            {
                existingController = target.AddComponent<ParalysisStatusController>();
            }

            // 麻痺効果を適用
            existingController.ApplyParalysis(speedModifier, speedMultiplier, duration, effectPrefab, statusManager);
        }
    }

    /// <summary>
    /// 麻痺状態を制御するコンポーネント
    /// 実際の速度変更と時間管理を行う
    /// </summary>
    public class ParalysisStatusController : MonoBehaviour
    {
        private SpeedModifier speedModifier;
        private Coroutine paralysisCoroutine;
        private GameObject currentEffect;
        
        public bool IsParalyzed { get; private set; } = false;

        private EnemyStatusManager statusManager;

        public void ApplyParalysis(SpeedModifier speedModifier, float speedMultiplier, float duration, GameObject effectPrefab, EnemyStatusManager statusManager = null)
        {
            // 既に麻痺状態の場合は現在の効果を停止
            if (paralysisCoroutine != null)
            {
                StopCoroutine(paralysisCoroutine);
                RemoveParalysis();
            }

            this.speedModifier = speedModifier;
            this.statusManager = statusManager;
            paralysisCoroutine = StartCoroutine(ParalysisCoroutine(speedMultiplier, duration, effectPrefab));
        }

        private IEnumerator ParalysisCoroutine(float speedMultiplier, float duration, GameObject effectPrefab)
        {
            IsParalyzed = true;

            // エフェクトを生成
            if (effectPrefab != null)
            {
                currentEffect = Instantiate(effectPrefab, transform.position, Quaternion.identity, transform);
            }

            // 速度を減少
            if (speedModifier != null)
            {
                speedModifier.SetModifier(speedMultiplier);
            }


            // 指定時間待機
            yield return new WaitForSeconds(duration);

            // 麻痺状態を解除
            RemoveParalysis();
        }

        private void RemoveParalysis()
        {
            IsParalyzed = false;

            // 速度を元に戻す
            if (speedModifier != null)
            {
                speedModifier.ResetModifier();
            }

            // エフェクトを削除
            if (currentEffect != null)
            {
                Destroy(currentEffect);
                currentEffect = null;
            }


            // EnemyStatusManagerに除去を通知
            if (statusManager != null)
            {
                statusManager.NotifyEffectRemoved("麻痺");
            }

            paralysisCoroutine = null;
        }

        private void OnDestroy()
        {
            // オブジェクト削除時にコルーチンを停止
            if (paralysisCoroutine != null)
            {
                StopCoroutine(paralysisCoroutine);
            }

            // エフェクトを削除
            if (currentEffect != null)
            {
                Destroy(currentEffect);
            }
        }
    }
}