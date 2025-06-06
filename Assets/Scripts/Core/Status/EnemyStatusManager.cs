using System.Collections.Generic;
using UnityEngine;
using Projectiles.StatusEffects;

namespace Core.Status
{
    /// <summary>
    /// 敵キャラクターの状態効果を総合的に管理するコンポーネント
    /// 複数の状態効果の適用、解除、重複チェックなどを行う
    /// </summary>
    public class EnemyStatusManager : MonoBehaviour
    {
        private Dictionary<string, IProjectileStatusEffect> activeEffects = new Dictionary<string, IProjectileStatusEffect>();

        public bool HasStatusEffect(string effectName)
        {
            return activeEffects.ContainsKey(effectName);
        }

        public void ApplyStatusEffect(IProjectileStatusEffect effect)
        {
            if (effect == null)
            {
                Debug.LogWarning("EnemyStatusManager: 状態効果がnullです");
                return;
            }

            string effectName = effect.EffectName;

            // 重複チェック
            if (HasStatusEffect(effectName))
            {
                Debug.Log($"EnemyStatusManager: {gameObject.name} には既に '{effectName}' が適用されています");
                return;
            }

            // 状態効果を適用
            try
            {
                effect.ApplyEffect(gameObject);
                
                // 適用成功時のみ記録
                activeEffects[effectName] = effect;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"EnemyStatusManager: '{effectName}' の適用中にエラーが発生しました。エラー: {e.Message}");
            }
        }

        public void RemoveStatusEffect(string effectName)
        {
            if (activeEffects.ContainsKey(effectName))
            {
                activeEffects.Remove(effectName);
            }
        }

        public void RemoveAllStatusEffects()
        {
            activeEffects.Clear();
        }

        public string[] GetActiveEffectNames()
        {
            string[] names = new string[activeEffects.Count];
            activeEffects.Keys.CopyTo(names, 0);
            return names;
        }

        // パブリックメソッド：他のシステムから状態効果の除去通知を受け取る
        public void NotifyEffectRemoved(string effectName)
        {
            RemoveStatusEffect(effectName);
        }
    }
}