using UnityEngine;

namespace VLCNP.Core
{
    /// <summary>
    /// 敵の情報を管理し、NoEnemiesフラグを簡単に切り替えるためのコンポーネント
    /// デバッグ・テスト用途でインスペクターから直接フラグを操作可能
    /// </summary>
    public class EnemyInfo : MonoBehaviour
    {
        [Header("敵情報設定")]
        [SerializeField]
        [Tooltip("NoEnemiesフラグの現在の状態（敵がいない場合はtrue）")]
        private bool noEnemies = false;

        private FlagManager flagManager;
        private bool previousNoEnemies;

        private void Awake()
        {
            // FlagManagerを取得
            GameObject flagManagerObject = GameObject.FindWithTag("FlagManager");
            if (flagManagerObject != null)
            {
                flagManager = flagManagerObject.GetComponent<FlagManager>();
            }
        }

        private void Start()
        {
            // 初期状態を同期してフラグを設定
            if (flagManager != null)
            {
                flagManager.SetFlag(Flag.NoEnemies, noEnemies);
                previousNoEnemies = noEnemies;
                Debug.Log($"NoEnemiesフラグを {(noEnemies ? "ON" : "OFF")} に設定しました");
            }
        }

        /// <summary>
        /// NoEnemiesフラグを切り替える
        /// </summary>
        public void ToggleNoEnemiesFlag()
        {
            if (flagManager != null)
            {
                noEnemies = !noEnemies;
                flagManager.SetFlag(Flag.NoEnemies, noEnemies);
                previousNoEnemies = noEnemies;
                Debug.Log(
                    $"NoEnemiesフラグを切り替えました: {(noEnemies ? "ON (敵なし)" : "OFF (敵あり)")}"
                );
            }
        }

        /// <summary>
        /// NoEnemiesフラグを直接設定する
        /// </summary>
        /// <param name="value">設定する値</param>
        public void SetNoEnemiesFlag(bool value)
        {
            if (flagManager != null)
            {
                noEnemies = value;
                flagManager.SetFlag(Flag.NoEnemies, noEnemies);
                previousNoEnemies = noEnemies;
                Debug.Log(
                    $"NoEnemiesフラグを {(noEnemies ? "ON (敵なし)" : "OFF (敵あり)")} に設定しました"
                );
            }
        }

        /// <summary>
        /// 現在のNoEnemiesフラグの状態を取得
        /// </summary>
        /// <returns>NoEnemiesフラグの状態</returns>
        public bool GetNoEnemiesFlag()
        {
            return flagManager?.GetFlag(Flag.NoEnemies) ?? false;
        }
    }
}
