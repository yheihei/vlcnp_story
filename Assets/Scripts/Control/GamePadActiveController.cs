using System.Collections;
using UnityEngine;
using VLCNP.Core;

namespace VLCNP.Control
{
    public class GamePadActiveController : MonoBehaviour
    {
        private enum ActiveCondition
        {
            WhenGamepad,
            WhenNotGamepad
        }

        [SerializeField] private ActiveCondition activeCondition = ActiveCondition.WhenGamepad;

        private FlagManager flagManager;
        private bool isLoaded = false;
        private bool isSubscribed = false;

        private void Awake()
        {
            TryCacheFlagManager();
        }

        private void Start()
        {
            SubscribeFlagChange();
            StartCoroutine(WaitForLoadComplete());
        }

        private IEnumerator WaitForLoadComplete()
        {
            while (LoadCompleteManager.Instance == null)
            {
                yield return null;
            }

            while (!LoadCompleteManager.Instance.IsLoaded)
            {
                yield return null;
            }

            isLoaded = true;
            ApplyActiveState();
        }

        private void OnChangeFlag(Flag flag, bool value)
        {
            if (!isLoaded) return;
            ApplyActiveState();
        }

        private void ApplyActiveState()
        {
            if (flagManager == null)
            {
                TryCacheFlagManager();
                if (flagManager == null)
                {
                    Debug.LogWarning("[GamePadActiveController] FlagManager (Tag: FlagManager) not found.");
                    return;
                }
                SubscribeFlagChange();
            }

            bool isGamePad = flagManager.GetFlag(Flag.IsGamePad);
            bool shouldBeActive = activeCondition == ActiveCondition.WhenGamepad ? isGamePad : !isGamePad;
            if (gameObject.activeSelf == shouldBeActive) return;
            gameObject.SetActive(shouldBeActive);
        }

        private void TryCacheFlagManager()
        {
            if (flagManager != null) return;

            GameObject flagManagerObject = GameObject.FindWithTag("FlagManager");
            if (flagManagerObject == null) return;

            flagManager = flagManagerObject.GetComponent<FlagManager>();
        }

        private void SubscribeFlagChange()
        {
            if (isSubscribed) return;
            if (flagManager == null) return;

            flagManager.OnChangeFlag += OnChangeFlag;
            isSubscribed = true;
        }

        private void OnDestroy()
        {
            if (flagManager == null) return;
            if (!isSubscribed) return;

            flagManager.OnChangeFlag -= OnChangeFlag;
        }
    }
}
