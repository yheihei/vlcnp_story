using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Control;
using VLCNP.Core;

namespace VLCNP.UI
{
    public class StatusCharacterIcon : MonoBehaviour
    {
        // Activeになるときのプレイヤーオブジェクト
        [SerializeField]
        GameObject playerObject;

        // Activeなスプライト
        [SerializeField]
        Sprite activeSprite;

        // Inactiveなスプライト
        [SerializeField]
        Sprite inactiveSprite;

        [SerializeField]
        GameObject selectIcon;

        PartyCongroller partyCongroller;

        [SerializeField]
        Flag activeFlag;
        FlagManager flagManager;

        private SpriteRenderer spriteRenderer;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError($"SpriteRenderer not found on {gameObject.name}");
                return;
            }
            // PartyタグからPartyCongrollerを取得
            partyCongroller = GameObject
                .FindGameObjectWithTag("Party")
                .GetComponent<PartyCongroller>();
            partyCongroller.OnChangeCharacter += OnChangeCharacter;
            // FlagManagerタグからFlagManagerを取得
            flagManager = GameObject
                .FindGameObjectWithTag("FlagManager")
                .GetComponent<FlagManager>();
            flagManager.OnChangeFlag += OnChangeFlag;
        }

        void Start()
        {
            CheckFlagAndIconShow();
            SetIcon(partyCongroller.GetCurrentPlayer());
            UpdateSelectIcon();
        }

        void UpdateSelectIcon()
        {
            selectIcon.SetActive(partyCongroller.GetCurrentPlayer() == playerObject);
        }

        void CheckFlagAndIconShow()
        {
            bool shouldShow = activeFlag == Flag.None || flagManager.GetFlag(activeFlag);
            if (shouldShow)
            {
                ShowIcon();
            }
            else
            {
                HiddenIcon();
            }
        }

        void ShowIcon()
        {
            spriteRenderer.enabled = true;
        }

        void HiddenIcon()
        {
            spriteRenderer.enabled = false;
            selectIcon.SetActive(false);
        }

        void OnChangeCharacter(GameObject character)
        {
            SetIcon(character);
            UpdateSelectIcon();
        }

        void OnChangeFlag(Flag flag, bool value)
        {
            if (flag != activeFlag)
                return;
            CheckFlagAndIconShow();
            UpdateSelectIcon();
        }

        private void SetIcon(GameObject character)
        {
            // プレイヤーオブジェクトが変更されたら、スプライトを変更する
            if (character == playerObject)
            {
                spriteRenderer.sprite = activeSprite;
            }
            else
            {
                spriteRenderer.sprite = inactiveSprite;
            }
        }
    }
}
