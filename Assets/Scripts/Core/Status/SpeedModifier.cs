using UnityEngine;

namespace Core.Status
{
    /// <summary>
    /// 速度の倍率を管理するコンポーネント
    /// 敵キャラクターなどにアタッチして使用し、移動速度の一時的な変更を可能にする
    /// </summary>
    public class SpeedModifier : MonoBehaviour
    {
        [Header("速度倍率設定")]
        [SerializeField]
        [Tooltip("現在の速度倍率（1.0が通常速度）")]
        [Range(0.1f, 3.0f)]
        private float currentModifier = 1.0f;


        /// <summary>
        /// 現在の速度倍率を取得
        /// </summary>
        public float CurrentModifier => currentModifier;

        /// <summary>
        /// 速度倍率が通常（1.0）以外に設定されているか
        /// </summary>
        public bool IsModified => !Mathf.Approximately(currentModifier, 1.0f);

        /// <summary>
        /// 速度倍率を設定
        /// </summary>
        /// <param name="modifier">新しい速度倍率</param>
        public void SetModifier(float modifier)
        {
            currentModifier = Mathf.Clamp(modifier, 0.1f, 3.0f);
        }

        /// <summary>
        /// 速度倍率をリセット（1.0に戻す）
        /// </summary>
        public void ResetModifier()
        {
            currentModifier = 1.0f;
        }

        /// <summary>
        /// 基準速度に倍率を適用した速度を計算
        /// </summary>
        /// <param name="baseSpeed">基準速度</param>
        /// <returns>倍率適用後の速度</returns>
        public float CalculateModifiedSpeed(float baseSpeed)
        {
            return baseSpeed * currentModifier;
        }
    }
}