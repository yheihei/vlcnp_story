using UnityEngine;
using VLCNP.Control;
using VLCNP.SceneManagement;

namespace VLCNP.UI
{
    public class GameSelect : MonoBehaviour
    {
        [SerializeField] GameObject selectButton;
        [SerializeField] NewGame newGame;
        [SerializeField] ContinueGame continueGame;

        [Header("カーソル配置")]
        [Tooltip("「はじめから」のラベル。カーソルはこのラベルを基準に配置する")]
        [SerializeField] RectTransform newGameLabel;
        [Tooltip("「つづきから」のラベル。カーソルはこのラベルを基準に配置する")]
        [SerializeField] RectTransform continueGameLabel;
        [Tooltip("ラベル中心からカーソルまでのワールド座標オフセット")]
        [SerializeField] Vector2 cursorOffset = new Vector2(-1.84f, 0.17f);

        enum Select
        {
            Start,
            Load
        }

        Select currentSelect = Select.Start;
        Vector3 selectButtonOriginalPosition;
        bool canSelect = false;

        public void EnableSelect()
        {
            canSelect = true;
        }

        private void Awake()
        {
            selectButtonOriginalPosition = selectButton.transform.position;
        }

        public void Update()
        {
            if (!canSelect) return;
            if (PlayerInputAdapter.WasMenuDownPressed())
            {
                ChangeSelect(Select.Load);
            }
            else if (PlayerInputAdapter.WasMenuUpPressed())
            {
                ChangeSelect(Select.Start);
            }
            else if (PlayerInputAdapter.WasMenuSubmitPressed())
            {
                SelectGameMode();
            }
        }

        // ラベルは Screen Space - Camera の Canvas 上にあり、カメラ位置に追従する。
        // カーソルはワールド空間のスプライトなので、シーンに保存されたカメラ位置が
        // 変わるとラベルとの相対位置が崩れる。毎フレームラベル基準で置き直して防ぐ。
        private void LateUpdate()
        {
            PlaceCursor();
        }

        public void MoveCursorToNewGame()
        {
            ChangeSelect(Select.Start);
        }

        public void MoveCursorToContinueGame()
        {
            ChangeSelect(Select.Load);
        }

        private void ChangeSelect(Select select)
        {
            currentSelect = select;
            PlaceCursor();
        }

        private void PlaceCursor()
        {
            RectTransform label = currentSelect == Select.Start ? newGameLabel : continueGameLabel;
            if (label == null)
            {
                // ラベル未設定時は従来どおりシーン配置の位置を使う
                float offsetY = currentSelect == Select.Start ? 0f : -0.9f;
                selectButton.transform.position = selectButtonOriginalPosition + new Vector3(0f, offsetY, 0f);
                return;
            }
            Vector3 labelPosition = label.position;
            selectButton.transform.position = new Vector3(
                labelPosition.x + cursorOffset.x,
                labelPosition.y + cursorOffset.y,
                selectButton.transform.position.z
            );
        }

        private void SelectGameMode()
        {
            switch (currentSelect)
            {
                case Select.Start:
                    newGame.Execute();
                    break;
                case Select.Load:
                    continueGame.Execute();
                    break;
            }
        }
    }
}
