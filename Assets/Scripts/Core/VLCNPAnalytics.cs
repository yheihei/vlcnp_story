using System;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.UI;

namespace VLCNP.Core
{
    /**
     * Unity Analytics へ送るカスタムイベントの窓口。
     * イベント名・パラメータ名は Unity Dashboard の Event Manager に登録したものと一致させる。
     * エディタと Development Build では送らない(#645)。
     */
    public class VLCNPAnalytics : MonoBehaviour
    {
        public const string AreaEnteredEvent = "areaEntered";
        public const string BossDefeatedEvent = "bossDefeated";
        public const string GameOverEvent = "gameOver";
        public const string TrialEndEvent = "trialEnd";

        public void ReachedTrial1End()
        {
            // Developmentビルド・エディタでは収集を開始していないため送信しない
            if (Debug.isDebugBuild) return;
            AnalyticsService.Instance.RecordEvent("trial1End");
            Debug.Log("Trial completion event recorded");
        }

        /** エリア名バナーの表示時。同一エリア内の部屋移動ではバナーが出ないので送られない */
        public static void RecordAreaEntered(string areaName)
        {
            Record(AreaEnteredEvent, new CustomEvent(AreaEnteredEvent) { { "areaName", areaName ?? "" } });
        }

        /** defeatedFlag を持つボスの死亡時。bossName はフラグ名 */
        public static void RecordBossDefeated(string bossName)
        {
            Record(BossDefeatedEvent, new CustomEvent(BossDefeatedEvent) { { "bossName", bossName ?? "" } });
        }

        /** ゲームオーバー演出の開始時 */
        public static void RecordGameOver(string sceneName, string areaName)
        {
            Record(
                GameOverEvent,
                new CustomEvent(GameOverEvent)
                {
                    { "sceneName", sceneName ?? "" },
                    { "areaName", areaName ?? "" },
                }
            );
        }

        /** 体験版完了挨拶シーン到達時 */
        public static void RecordTrialEnd()
        {
            Record(TrialEndEvent, new CustomEvent(TrialEndEvent));
        }

        /** Core の AreaName バナーに入っているエリア名。見つからなければ空文字 */
        public static string GetCurrentAreaName()
        {
            GameObject areaNameObject = GameObject.FindWithTag("AreaName");
            if (areaNameObject == null) return "";
            Text text = areaNameObject.GetComponentInChildren<Text>(true);
            return text == null ? "" : text.text;
        }

        static void Record(string eventName, CustomEvent customEvent)
        {
            // 発火点の確認用にエディタでもログは出す。送信は非 Development ビルドのみ
            Debug.Log($"[Analytics] {eventName}{(Debug.isDebugBuild ? " (skipped: debug build)" : "")}");
            if (Debug.isDebugBuild) return;
            try
            {
                AnalyticsService.Instance.RecordEvent(customEvent);
            }
            catch (Exception e)
            {
                // 計測の失敗でゲームを止めない
                Debug.LogWarning($"[Analytics] failed to record {eventName}: {e.Message}");
            }
        }
    }
}
