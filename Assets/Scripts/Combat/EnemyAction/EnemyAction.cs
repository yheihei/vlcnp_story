using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Core.Status;

namespace VLCNP.Combat.EnemyAction
{
    public abstract class EnemyAction : MonoBehaviour, IEnemyAction
    {
        bool isDone = false;
        bool isExecuting = false;
        public bool IsDone { get => isDone; set => isDone = value; }
        public bool IsExecuting { get => isExecuting; set => isExecuting = value; }

        public abstract void Execute();

        /**
         * 行動実行後 再度実行可能にする
         */
        public void Reset()
        {
            isDone = false;
            isExecuting = false;
        }

        public void Stop()
        {
            // 何もしない
        }

        /// <summary>
        /// SpeedModifierを考慮した速度を取得する
        /// SpeedModifierコンポーネントがアタッチされている場合は倍率を適用した速度を返す
        /// </summary>
        /// <param name="baseSpeed">基準速度</param>
        /// <returns>修正後の速度</returns>
        protected float GetModifiedSpeed(float baseSpeed)
        {
            SpeedModifier modifier = GetComponent<SpeedModifier>();
            if (modifier != null)
            {
                return modifier.CalculateModifiedSpeed(baseSpeed);
            }
            return baseSpeed;
        }
    }
}
