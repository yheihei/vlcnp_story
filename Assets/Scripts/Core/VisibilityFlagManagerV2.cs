using System;
using UnityEngine;

namespace VLCNP.Core
{
    /** AND優先の複数フラグ条件で、最後に成立した要素の表示設定を適用する。 */
    [DisallowMultipleComponent]
    public class VisibilityFlagManagerV2 : MonoBehaviour
    {
        public enum LogicalOperator { AND, OR }

        [Serializable]
        public class AdditionalFlag
        {
            [SerializeField] private LogicalOperator operation;
            [SerializeField] private Flag flag;

            public LogicalOperator Operation => operation;
            public Flag Flag => flag;
        }

        [Serializable]
        public class VisibilityFlag
        {
            [SerializeField, Tooltip("None は常に true です。")]
            private Flag flag;
            [SerializeField] private AdditionalFlag[] additionalFlags = Array.Empty<AdditionalFlag>();
            [SerializeField] private bool isVisible;

            public bool IsVisible => isVisible;

            public bool Matches(FlagManager manager)
            {
                // ORで区切られたANDのまとまりを評価する。
                bool group = manager.GetFlag(flag);
                if (additionalFlags == null) return group;
                foreach (AdditionalFlag additional in additionalFlags)
                {
                    if (additional == null) return false;
                    if (additional.Operation == LogicalOperator.OR)
                    {
                        if (group) return true;
                        group = manager.GetFlag(additional.Flag);
                    }
                    else
                    {
                        group = group && manager.GetFlag(additional.Flag);
                    }
                }
                return group;
            }

            public bool Contains(Flag changedFlag)
            {
                if (flag == changedFlag) return true;
                if (additionalFlags == null) return false;
                foreach (AdditionalFlag additional in additionalFlags)
                {
                    if (additional != null && additional.Flag == changedFlag) return true;
                }
                return false;
            }
        }

        [SerializeField, Tooltip("条件が true の要素のうち、最後の要素を適用します。全て false なら状態を維持します。")]
        private VisibilityFlag[] visibilityFlags = Array.Empty<VisibilityFlag>();

        private FlagManager flagManager;

        private void Start()
        {
            flagManager = FlagManager.FindInScene();
            if (flagManager == null)
            {
                Debug.LogError("VisibilityFlagManagerV2: FlagManager が見つかりません。", this);
                return;
            }
            flagManager.OnChangeFlag += OnChangeFlag;
            CheckVisibility();
        }

        private void OnDestroy()
        {
            if (flagManager != null) flagManager.OnChangeFlag -= OnChangeFlag;
        }

        private void OnChangeFlag(Flag flag, bool value)
        {
            // 自身を非表示にした後も通知を受け、再表示できるよう購読を維持する。
            if (visibilityFlags == null) return;
            foreach (VisibilityFlag rule in visibilityFlags)
            {
                if (rule != null && rule.Contains(flag))
                {
                    CheckVisibility();
                    return;
                }
            }
        }

        private void CheckVisibility()
        {
            if (flagManager == null || visibilityFlags == null) return;
            for (int i = visibilityFlags.Length - 1; i >= 0; i--)
            {
                VisibilityFlag rule = visibilityFlags[i];
                if (rule == null || !rule.Matches(flagManager)) continue;
                gameObject.SetActive(rule.IsVisible);
                // 既存版と同じく、直下の子にも表示設定を適用する。
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(rule.IsVisible);
                }
                return;
            }
        }
    }
}
