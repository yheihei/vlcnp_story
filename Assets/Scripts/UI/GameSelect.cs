using UnityEngine;
using VLCNP.Input;
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
        bool canSelect = false;

        public void EnableSelect()
        {
            canSelect = true;
            InputManager.Instance.SwitchToUIActions();
        }

        private void Awake()
        {
            selectButtonOriginalPosition = selectButton.transform.position;
        }

        public void Update()
        {
            if (!canSelect) return;
            if (InputManager.Instance.IsNavigateDownPressed())
            {
                ChangeSelect(Select.Load);
            }
            else if (InputManager.Instance.IsNavigateUpPressed())
            {
                ChangeSelect(Select.Start);
            }
            else if (InputManager.Instance.IsSubmitPressed())
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
