namespace Core.Status
{
    /// <summary>
    /// 速度の修正が可能なオブジェクトを表すインターフェース
    /// 敵キャラクターの移動速度制御などに使用する
    /// </summary>
    public interface ISpeedModifiable
    {
        /// <summary>
        /// 速度の倍率を設定する
        /// </summary>
        /// <param name="modifier">速度倍率（1.0が基準速度、0.5で半分、2.0で2倍）</param>
        void SetSpeedModifier(float modifier);
        
        /// <summary>
        /// 現在の実効速度を取得する（基準速度 × 倍率）
        /// </summary>
        /// <returns>現在の実効速度</returns>
        float GetCurrentSpeed();
    }
}