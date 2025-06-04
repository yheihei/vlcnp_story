using UnityEngine;
using System.Collections.Generic;

namespace VLCNP.Effects
{
    /// <summary>
    /// 状態効果の視覚的エフェクトを制御するコンポーネント
    /// 麻痺、毒などの状態に応じてエフェクトを表示・管理する
    /// </summary>
    public class StatusEffectVisualController : MonoBehaviour
    {
        [Header("状態効果エフェクト設定")]
        [SerializeField]
        [Tooltip("麻痺状態のエフェクトプレファブ")]
        private GameObject paralysisEffectPrefab = null;

        [SerializeField]
        [Tooltip("毒状態のエフェクトプレファブ")]
        private GameObject poisonEffectPrefab = null;

        [SerializeField]
        [Tooltip("エフェクトを表示する位置のオフセット")]
        private Vector3 effectOffset = Vector3.zero;

        [SerializeField]
        [Tooltip("エフェクトを親オブジェクトにアタッチするかどうか")]
        private bool attachToParent = true;

        // 現在表示中のエフェクトを管理
        private Dictionary<string, GameObject> activeEffects = new Dictionary<string, GameObject>();

        /// <summary>
        /// 指定した状態効果のエフェクトを表示する
        /// </summary>
        /// <param name="effectType">効果タイプ（"麻痺", "毒"など）</param>
        public void ShowStatusEffect(string effectType)
        {
            GameObject effectPrefab = GetEffectPrefab(effectType);
            if (effectPrefab == null)
            {
                Debug.LogWarning($"StatusEffectVisualController: '{effectType}' 用のエフェクトプレファブが設定されていません");
                return;
            }

            // 既に同じエフェクトが表示されている場合は何もしない
            if (activeEffects.ContainsKey(effectType))
            {
                Debug.Log($"StatusEffectVisualController: '{effectType}' のエフェクトは既に表示されています");
                return;
            }

            // エフェクトを生成
            Vector3 effectPosition = transform.position + effectOffset;
            Quaternion effectRotation = Quaternion.identity;
            Transform parent = attachToParent ? transform : null;

            GameObject effectInstance = Instantiate(effectPrefab, effectPosition, effectRotation, parent);
            activeEffects[effectType] = effectInstance;
        }

        /// <summary>
        /// 指定した状態効果のエフェクトを非表示にする
        /// </summary>
        /// <param name="effectType">効果タイプ（"麻痺", "毒"など）</param>
        public void HideStatusEffect(string effectType)
        {
            if (!activeEffects.ContainsKey(effectType))
            {
                Debug.Log($"StatusEffectVisualController: '{effectType}' のエフェクトは表示されていません");
                return;
            }

            GameObject effectInstance = activeEffects[effectType];
            if (effectInstance != null)
            {
                Destroy(effectInstance);
            }

            activeEffects.Remove(effectType);
        }

        /// <summary>
        /// すべての状態効果エフェクトを非表示にする
        /// </summary>
        public void HideAllStatusEffects()
        {
            foreach (var effect in activeEffects.Values)
            {
                if (effect != null)
                {
                    Destroy(effect);
                }
            }

            activeEffects.Clear();
        }

        /// <summary>
        /// 指定した効果タイプが現在表示されているかどうかを確認
        /// </summary>
        /// <param name="effectType">効果タイプ</param>
        /// <returns>表示されている場合はtrue</returns>
        public bool IsEffectActive(string effectType)
        {
            return activeEffects.ContainsKey(effectType) && activeEffects[effectType] != null;
        }

        private GameObject GetEffectPrefab(string effectType)
        {
            switch (effectType)
            {
                case "麻痺":
                    return paralysisEffectPrefab;
                case "毒":
                    return poisonEffectPrefab;
                default:
                    return null;
            }
        }

        private void OnDestroy()
        {
            // オブジェクト削除時にすべてのエフェクトを削除
            HideAllStatusEffects();
        }
    }
}