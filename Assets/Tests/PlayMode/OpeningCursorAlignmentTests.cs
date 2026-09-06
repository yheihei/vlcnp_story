using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace VLCNP.Tests
{
    /// <summary>
    /// Opening シーンのタイトルメニューで、Akim アイコン(SelectButton)が
    /// 「はじめから」「つづきから」のラベルと同じ高さに置かれることを検証する。
    ///
    /// 背景: ラベルは Screen Space - Camera の Canvas、アイコンはワールド空間の
    /// スプライトなので、シーンに保存されたカメラ位置が変わるとアイコンだけがずれる。
    /// #646 の差分取り込みで CMCamera の追従オフセット Y が 0.5 → 1 に変わり、
    /// カメラが 0.5 上がってアイコンだけが下にずれた。
    ///
    /// ゲーム側のコードは Assembly-CSharp にあり asmdef から参照できないため、
    /// 名前検索と SendMessage で操作する。
    /// </summary>
    public class OpeningCursorAlignmentTests
    {
        const string SceneName = "Opening";
        const string CursorName = "SelectButton";
        const string GameSelectName = "GameSelect";
        const string NewGameLabelText = "はじめから";
        const string ContinueGameLabelText = "つづきから";

        // ラベル中心とアイコン中心の高さの差の許容値(ワールド単位)。
        // 設計上のオフセットは 0.17、今回の不具合では 0.5 ずれていた。
        const float VerticalTolerance = 0.3f;

        [UnityTest]
        public IEnumerator Cursor_AlignsWithSelectedLabel()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            // Canvas のレイアウトとカーソル配置(LateUpdate)が落ち着くまで数フレーム待つ
            for (int i = 0; i < 3; i++) yield return null;

            GameObject cursor = GameObject.Find(CursorName);
            Assert.IsNotNull(cursor, $"{CursorName} が見つからない");

            GameObject gameSelect = GameObject.Find(GameSelectName);
            Assert.IsNotNull(gameSelect, $"{GameSelectName} が見つからない");

            RectTransform newGameLabel = FindLabel(NewGameLabelText);
            RectTransform continueGameLabel = FindLabel(ContinueGameLabelText);

            AssertAligned(cursor.transform, newGameLabel, NewGameLabelText);

            gameSelect.SendMessage("MoveCursorToContinueGame");
            yield return null;
            AssertAligned(cursor.transform, continueGameLabel, ContinueGameLabelText);

            gameSelect.SendMessage("MoveCursorToNewGame");
            yield return null;
            AssertAligned(cursor.transform, newGameLabel, NewGameLabelText);

            Debug.Log("[OpeningCursorAlignmentTests] PASSED Cursor_AlignsWithSelectedLabel");
        }

        static RectTransform FindLabel(string text)
        {
            foreach (var t in Object.FindObjectsOfType<UnityEngine.UI.Text>(true))
            {
                if (t.text == text) return t.rectTransform;
            }
            Assert.Fail($"ラベル「{text}」が見つからない");
            return null;
        }

        static void AssertAligned(Transform cursor, RectTransform label, string labelName)
        {
            SpriteRenderer renderer = cursor.GetComponent<SpriteRenderer>();
            Assert.IsNotNull(renderer, "SelectButton に SpriteRenderer がない");
            Vector3 cursorCenter = renderer.bounds.center;
            Vector3 labelCenter = label.position;

            float dy = cursorCenter.y - labelCenter.y;
            bool ok = Mathf.Abs(dy) <= VerticalTolerance && cursorCenter.x < labelCenter.x;
            // unicli 経由で実行すると結果を受け取れないことがあるため、Console にも残す
            Debug.Log(
                $"[OpeningCursorAlignmentTests] {(ok ? "OK" : "NG")} 「{labelName}」 cursor=({cursorCenter.x:F2}, {cursorCenter.y:F2}) label=({labelCenter.x:F2}, {labelCenter.y:F2}) dy={dy:F2}"
            );
            Assert.LessOrEqual(
                Mathf.Abs(dy),
                VerticalTolerance,
                $"カーソルが「{labelName}」の高さからずれている: cursor.y={cursorCenter.y:F2} label.y={labelCenter.y:F2} dy={dy:F2}"
            );
            Assert.Less(
                cursorCenter.x,
                labelCenter.x,
                $"カーソルが「{labelName}」の左側にない: cursor.x={cursorCenter.x:F2} label.x={labelCenter.x:F2}"
            );
        }
    }
}
