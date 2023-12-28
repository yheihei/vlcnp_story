using System;
using System.Collections;
using System.Collections.Generic;
using Fungus;
using UnityEngine;
using VLCNP.Actions;
using VLCNP.UI;

namespace VLCNP.Core
{
    [RequireComponent(typeof(InformationTextSpawner))]
    public class GameEvent : MonoBehaviour, ICollisionAction
    {
        private FlagManager flagManager;
        private string defaultBlockName = "New Block";

        [SerializeField]
        public Flowchart flowChart;
        [SerializeField] FlagToBlockName[] flagToBlockName;
        [Serializable]
        public class FlagToBlockName
        {
            [SerializeField] private Flag flag;
            [SerializeField] private string blockName;
            // 自動で開始するかどうか
            [SerializeField] bool isAutoStart;
            [SerializeField] bool isCollisionStart;

            public Flag Flag => flag;
            public string BlockName => blockName;
            public bool IsAutoStart => isAutoStart;
            public bool IsCollisionStart => isCollisionStart;
        }

        [SerializeField]
        public string InformationText = "(↑)はなす";
        bool isAction = true;

        public bool IsAction { get => isAction; set => isAction = value; }
        InformationText informationTextObject = null;

        void Awake()
        {
            flagManager = GameObject.FindWithTag("FlagManager").GetComponent<FlagManager>();
        }

        public void Execute()
        {
            if (!isAction) return;
            if (flowChart == null) return;
            if (flowChart.HasExecutingBlocks()) return;
            isAction = false;
            StartCoroutine(EventExecute());
        }

        IEnumerator EventExecute() {
            FlagToBlockName flagToBlockName = GetCurrentBlockNameFromFlag();
            flowChart.ExecuteBlock(flagToBlockName.BlockName);
            yield return new WaitUntil(() => flowChart.HasExecutingBlocks() == false);
            // メニューが開いていたら閉じるまで待つ
            MenuDialog menuDialog = MenuDialog.ActiveMenuDialog;
            if (menuDialog != null)
            {
                yield return new WaitUntil(() => menuDialog.gameObject.activeSelf == false);
            }
            isAction = true;
        }

        public void HideInformation()
        {
            if (informationTextObject == null) return;
            Destroy(informationTextObject.gameObject);
        }

        // TODO: Chat.csからGameEvent.csに置き換えられたタイミングで IsCollisionStart() に直す
        public bool IsAutoStart()
        {
            return GetCurrentBlockNameFromFlag().IsCollisionStart;
        }

        public void ShowInformation()
        {
            if (InformationText == null) return;
            InformationTextSpawner spawner = GetComponent<InformationTextSpawner>();
            if (spawner == null) return;
            if (informationTextObject != null) return;
            informationTextObject = spawner.Spawn(InformationText);
        }

        public FlagToBlockName GetCurrentBlockNameFromFlag()
        {
            // flagToBlockName を後ろから見ていって、現在有効なフラグのBlockNameを返す
            for (int i = flagToBlockName.Length - 1; i >= 0; i--)
            {
                if (flagManager.GetFlag(flagToBlockName[i].Flag))
                {
                    return flagToBlockName[i];
                }
            }
            // 設定がなければException
            throw new Exception("flagToBlockNameが設定されていません");
        }
        
    }    
}
