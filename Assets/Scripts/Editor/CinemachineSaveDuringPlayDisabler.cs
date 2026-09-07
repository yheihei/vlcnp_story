using UnityEditor;
using UnityEngine;

namespace VLCNP.Editor
{
    /// <summary>
    /// Cinemachine の Save During Play を常に OFF に保つ。
    /// ON のままプレイモードを抜けると vcam の実行時値(追従オフセットや OrthographicSize)が
    /// 編集中シーンへ書き戻され、意図しないカメラ差分が混入するため。
    /// 設定は EditorPrefs(マシン全体)で、Inspector のチェックボックスから誰でも ON にできるので、
    /// エディタ起動・スクリプトリロード時とプレイモード突入前に強制的に OFF へ戻す。
    /// </summary>
    [InitializeOnLoad]
    public static class CinemachineSaveDuringPlayDisabler
    {
        static CinemachineSaveDuringPlayDisabler()
        {
            ForceDisable("エディタ読み込み時");
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                ForceDisable("プレイモード突入前");
            }
        }

        private static void ForceDisable(string timing)
        {
            if (!SaveDuringPlay.SaveDuringPlay.Enabled)
            {
                return;
            }

            SaveDuringPlay.SaveDuringPlay.Enabled = false;
            Debug.LogWarning(
                $"[CinemachineSaveDuringPlayDisabler] Cinemachine の Save During Play が ON になっていたため OFF に戻しました({timing})。");
        }
    }
}
