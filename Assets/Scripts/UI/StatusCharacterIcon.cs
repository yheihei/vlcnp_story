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

        void Awake()
        {
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
            if (partyCongroller.GetCurrentPlayer() == playerObject)
            {
                selectIcon.SetActive(true);
            }
            else
            {
                selectIcon.SetActive(false);
            }
        }

        void CheckFlagAndIconShow()
        {
            if (activeFlag == Flag.None)
            {
                ShowIcon();
                return;
            }
            if (flagManager.GetFlag(activeFlag))
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
            GetComponent<SpriteRenderer>().enabled = true;
        }

        void HiddenIcon()
        {
            GetComponent<SpriteRenderer>().enabled = false;
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
                GetComponent<SpriteRenderer>().sprite = activeSprite;
            }
            else
            {
                GetComponent<SpriteRenderer>().sprite = inactiveSprite;
            }
        }
    }
}
