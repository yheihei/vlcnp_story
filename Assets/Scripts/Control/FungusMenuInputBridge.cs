using Fungus;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VLCNP.Control
{
    /// <summary>
    /// Fungus の MenuDialog に対してゲームパッド入力をブリッジする常駐コンポーネント。
    /// </summary>
    public class FungusMenuInputBridge : MonoBehaviour
    {
        const string BridgeObjectName = "[FungusMenuInputBridge]";

        EventSystem suppressedEventSystem;
        bool previousSendNavigationEvents;
        bool hasSuppressedNavigationEvents;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (FindObjectOfType<FungusMenuInputBridge>() != null)
                return;

            GameObject go = new GameObject(BridgeObjectName);
            DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideInHierarchy;
            go.AddComponent<FungusMenuInputBridge>();
        }

        void Update()
        {
            MenuDialog menu = MenuDialog.ActiveMenuDialog;
            if (!HasDisplayedOptions(menu))
            {
                RestoreNavigationEvents();
                return;
            }

            EventSystem eventSystem = GetActiveEventSystem();
            if (eventSystem == null)
            {
                RestoreNavigationEvents();
                return;
            }

            SuppressNavigationEvents(eventSystem);
            EnsureSelection(menu, eventSystem);
            HandleNavigation(menu, eventSystem);
            HandleSubmit(eventSystem);
        }

        void OnDisable()
        {
            RestoreNavigationEvents();
        }

        void OnDestroy()
        {
            RestoreNavigationEvents();
        }

        void SuppressNavigationEvents(EventSystem eventSystem)
        {
            if (suppressedEventSystem == eventSystem)
            {
                eventSystem.sendNavigationEvents = false;
                return;
            }

            RestoreNavigationEvents();
            suppressedEventSystem = eventSystem;
            previousSendNavigationEvents = eventSystem.sendNavigationEvents;
            hasSuppressedNavigationEvents = true;
            eventSystem.sendNavigationEvents = false;
        }

        void RestoreNavigationEvents()
        {
            if (!hasSuppressedNavigationEvents)
                return;

            if (suppressedEventSystem != null)
            {
                suppressedEventSystem.sendNavigationEvents = previousSendNavigationEvents;
            }

            suppressedEventSystem = null;
            previousSendNavigationEvents = false;
            hasSuppressedNavigationEvents = false;
        }

        static bool HasDisplayedOptions(MenuDialog menu)
        {
            if (menu == null || !menu.IsActive())
                return false;

            return menu.DisplayedOptionsCount > 0;
        }

        static EventSystem GetActiveEventSystem()
        {
            EventSystem current = EventSystem.current;
            if (current != null && current.isActiveAndEnabled)
                return current;

            foreach (EventSystem eventSystem in FindObjectsOfType<EventSystem>())
            {
                if (eventSystem != null && eventSystem.isActiveAndEnabled)
                    return eventSystem;
            }

            return null;
        }

        static void EnsureSelection(MenuDialog menu, EventSystem eventSystem)
        {
            GameObject current = eventSystem.currentSelectedGameObject;
            if (current != null && current.activeInHierarchy)
            {
                return;
            }

            foreach (Button button in menu.CachedButtons)
            {
                if (button == null)
                    continue;
                if (!button.gameObject.activeInHierarchy || !button.interactable)
                    continue;
                eventSystem.SetSelectedGameObject(button.gameObject);
                break;
            }
        }

        static void HandleNavigation(MenuDialog menu, EventSystem eventSystem)
        {
            int direction = 0;
            if (PlayerInputAdapter.WasMenuDownPressed())
            {
                direction = 1;
            }
            else if (PlayerInputAdapter.WasMenuUpPressed())
            {
                direction = -1;
            }

            if (direction == 0)
                return;

            Button[] buttons = menu.CachedButtons;
            if (buttons == null || buttons.Length == 0)
                return;

            GameObject current = eventSystem.currentSelectedGameObject;
            int currentIndex = -1;
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == null)
                    continue;
                if (buttons[i].gameObject == current)
                {
                    currentIndex = i;
                    break;
                }
            }

            // current が見つからなかった場合は EnsureSelection が設定済みのはずなので再帰的に呼び出さない
            int startIndex = currentIndex >= 0 ? currentIndex : 0;
            int count = buttons.Length;
            for (int step = 1; step <= count; step++)
            {
                int targetIndex = (startIndex + direction * step + count) % count;
                Button candidate = buttons[targetIndex];
                if (candidate == null)
                    continue;
                if (!candidate.gameObject.activeInHierarchy || !candidate.interactable)
                    continue;
                eventSystem.SetSelectedGameObject(candidate.gameObject);
                break;
            }
        }

        static void HandleSubmit(EventSystem eventSystem)
        {
            if (!PlayerInputAdapter.WasMenuSubmitPressed())
            {
                return;
            }

            GameObject current = eventSystem.currentSelectedGameObject;
            if (current == null)
            {
                return;
            }

            ExecuteEvents.Execute(current, new BaseEventData(eventSystem), ExecuteEvents.submitHandler);
        }
    }
}
