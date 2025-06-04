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
        [Header("デバッグ情報")]
        [SerializeField]
        [Tooltip("現在適用中の状態効果一覧（読み取り専用）")]
        private List<string> activeEffectNames = new List<string>();

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
                UpdateDebugInfo();
                
                Debug.Log($"EnemyStatusManager: {gameObject.name} に '{effectName}' を適用しました");
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
                UpdateDebugInfo();
                Debug.Log($"EnemyStatusManager: {gameObject.name} から '{effectName}' を除去しました");
            }
        }

        public void RemoveAllStatusEffects()
        {
            activeEffects.Clear();
            UpdateDebugInfo();
            Debug.Log($"EnemyStatusManager: {gameObject.name} のすべての状態効果を除去しました");
        }

        public string[] GetActiveEffectNames()
        {
            string[] names = new string[activeEffects.Count];
            activeEffects.Keys.CopyTo(names, 0);
            return names;
        }

        private void UpdateDebugInfo()
        {
            activeEffectNames.Clear();
            foreach (var effectName in activeEffects.Keys)
            {
                activeEffectNames.Add(effectName);
            }
        }

        // パブリックメソッド：他のシステムから状態効果の除去通知を受け取る
        public void NotifyEffectRemoved(string effectName)
        {
            RemoveStatusEffect(effectName);
        }

        // Unity Editor用の情報表示
        void OnValidate()
        {
            UpdateDebugInfo();
        }

#if UNITY_EDITOR
        [Header("エディタ用テスト機能")]
        [SerializeField]
        [Tooltip("テスト用：この状態効果名を除去します")]
        private string testRemoveEffectName = "";

        [ContextMenu("Test: Remove Effect")]
        private void TestRemoveEffect()
        {
            if (!string.IsNullOrEmpty(testRemoveEffectName))
            {
                RemoveStatusEffect(testRemoveEffectName);
            }
        }

        [ContextMenu("Test: Remove All Effects")]
        private void TestRemoveAllEffects()
        {
            RemoveAllStatusEffects();
        }
#endif
    }
}