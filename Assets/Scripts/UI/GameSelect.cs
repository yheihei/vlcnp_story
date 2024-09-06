using UnityEngine;
using VLCNP.SceneManagement;

namespace VLCNP.UI
{
    public class GameSelect : MonoBehaviour
    {
        [SerializeField] GameObject selectButton;
        [SerializeField] NewGame newGame;
        [SerializeField] ContinueGame continueGame;
        enum Select
        {
            Start,
            Load
        }

        Select currentSelect = Select.Start;
        Vector3 selectButtonOriginalPosition;


        private void Awake()
        {
            selectButtonOriginalPosition = selectButton.transform.position;
        }

        public void FixedUpdate()
        {
            if (Input.GetKey("down"))
            {
                ChangeSelect(Select.Load);
            }
            else if (Input.GetKey("up"))
            {
                ChangeSelect(Select.Start);
            }
            else if (Input.GetKey("return"))
            {
                SelectGameMode();
            }
        }

        private void ChangeSelect(Select select)
        {
            switch (select)
            {
                case Select.Start:
                    currentSelect = Select.Start;
                    selectButton.transform.position = selectButtonOriginalPosition;
                    break;
                case Select.Load:
                    currentSelect = Select.Load;
                    // 元の位置のY座標から-1.5fした位置に移動
                    selectButton.transform.position = new Vector3(selectButtonOriginalPosition.x, selectButtonOriginalPosition.y - 0.9f, selectButtonOriginalPosition.z);
                    break;
            }
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
