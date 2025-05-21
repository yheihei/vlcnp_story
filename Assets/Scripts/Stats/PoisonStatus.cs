using System; // Action を使用するために追加
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Movement; // Moverクラスを参照するために追加

namespace VLCNP.Stats
{
    public class PoisonStatus : MonoBehaviour
    {
        [SerializeField]
        float poisonDuration = 5f; // 毒の継続時間
        float remainingTime = 0f; // 毒の残り時間
        bool isPoisoned = false; // 毒状態かどうか
        Mover mover; // Moverコンポーネントのキャッシュ

        public event Action OnPoisonStarted;
        public event Action OnPoisonCured;

        // 外部から毒状態を設定するためのプロパティ
        public bool IsPoisoned
        {
            get { return isPoisoned; }
        }

        void Awake()
        {
            mover = GetComponent<Mover>();
        }

        void Update()
        {
            if (!isPoisoned)
                return;

            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0)
            {
                Cure();
            }
        }

        // 毒状態を開始するメソッド
        public void ActivatePoison()
        {
            isPoisoned = true;
            remainingTime = poisonDuration;
            // TODO: Moverに移動速度低下を通知
            if (mover != null)
            {
                mover.SetSpeedModifier(0.5f); // 例: 速度を50%に
            }
            Debug.Log($"{gameObject.name} is poisoned.");
            OnPoisonStarted?.Invoke();
        }

        // 毒状態を治療するメソッド
        public void Cure()
        {
            isPoisoned = false;
            remainingTime = 0;
            // TODO: Moverに移動速度を元に戻すよう通知
            if (mover != null)
            {
                mover.SetSpeedModifier(1f); // 例: 速度を100%に
            }
            Debug.Log($"{gameObject.name} is cured from poison.");
            OnPoisonCured?.Invoke();
        }

        // キャラクターが無効になったときに呼び出される
        void OnDisable()
        {
            // 毒のカウントダウンを一時停止する処理はUpdateが呼ばれなくなるため不要
            // 必要に応じて、キャラクターが再度有効になった時に残り時間を調整するロジックをOnEnableに追加することも検討
        }

        // キャラクターが有効になったときに呼び出される
        void OnEnable()
        {
            // プレイヤー切り替え時にタイマーを再開する
            // isPoisoned が true の場合のみタイマーを進める（Update内で処理される）
        }

        // 外部から毒のカウントダウンを一時停止/再開するためのメソッド
        public void PauseTimer()
        {
            // Updateループを止めることでタイマーは停止する
            // enabled = false; だと他のコンポーネントから参照できなくなる可能性があるため、
            // Update内の処理を isPoisoned フラグで制御することで対応
            // より明示的に制御したい場合は、isActiveフラグなどを追加してUpdate内の時間更新を制御する
            Debug.Log($"{gameObject.name} poison timer paused.");
        }

        public void ResumeTimer()
        {
            // enabled = true;
            Debug.Log($"{gameObject.name} poison timer resumed.");
        }
    }
}
