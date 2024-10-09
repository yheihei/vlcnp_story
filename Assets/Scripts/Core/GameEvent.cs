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

        void Start()
        {
            AutoStartBlock();
            flagManager.OnChangeFlag += OnChangeFlag;
        }

        // Flagの変更通知時、自動実行のBlockNameが設定されていたら即時実行
        void OnChangeFlag(Flag flag, bool value)
        {
            if (!value) return;
            if (flag != GetCurrentValidFlag()) return;
            AutoStartBlock();
        }

        private void AutoStartBlock()
        {
            FlagToBlockName flagToBlockName = GetCurrentBlockNameFromFlag();
            if (flagToBlockName == null) return;
            if (flagToBlockName.IsAutoStart)
            {
                StartCoroutine(EventExecute(flagToBlockName));
            }
        }

        /***
         * 外部からイベントを実行する
         */
        public void Execute()
        {
            FlagToBlockName flagToBlockName = GetCurrentBlockNameFromFlag();
            // 現在のBlockNameが即時実行なら実行しない
            if (flagToBlockName != null && flagToBlockName.IsAutoStart) return;
            StartCoroutine(EventExecute(flagToBlockName));
        }

        public void ExecuteCollisionStart()
        {
            FlagToBlockName flagToBlockName = GetCurrentBlockNameFromFlag();
            // 現在のBlockNameがcollisionStartでないなら実行しない
            if (flagToBlockName != null && !flagToBlockName.IsCollisionStart) return;
            StartCoroutine(EventExecute(flagToBlockName));
        }

        IEnumerator EventExecute(FlagToBlockName flagToBlockName)
        {
            if (!isAction) yield break;
            // ブロック名が空文字のときは実行しない
            if (flagToBlockName == null || string.IsNullOrEmpty(flagToBlockName.BlockName)) yield break;
            yield return ExecuteBlock(flagToBlockName != null ? flagToBlockName.BlockName : defaultBlockName);
        }

        private IEnumerator ExecuteBlock(string blockName)
        {
            if (flowChart == null) yield break;
            if (flowChart.HasExecutingBlocks()) yield break;
            isAction = false;
            print("ExecuteBlock: " + this + ": " + blockName);
            flowChart.ExecuteBlock(blockName);
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

        public bool IsCollisionStart()
        {
            FlagToBlockName flagToBlockName = GetCurrentBlockNameFromFlag();
            if (flagToBlockName == null) return false;
            return flagToBlockName.IsCollisionStart;
        }

        public void ShowInformation()
        {
            if (InformationText == null) return;
            InformationTextSpawner spawner = GetComponent<InformationTextSpawner>();
            if (spawner == null) return;
            if (informationTextObject != null) return;
            informationTextObject = spawner.Spawn(InformationText);
        }

        public FlagToBlockName? GetCurrentBlockNameFromFlag()
        {
            // flagToBlockName を後ろから見ていって、現在有効なフラグのBlockNameを返す
            for (int i = flagToBlockName.Length - 1; i >= 0; i--)
            {
                if (flagManager.GetFlag(flagToBlockName[i].Flag))
                {
                    return flagToBlockName[i];
                }
            }
            // 設定がなければnullを返す
            return null;
        }

        public Flag GetCurrentValidFlag()
        {
            // flagToBlockName を後ろから見ていって、現在有効なフラグのFlagを返す
            for (int i = flagToBlockName.Length - 1; i >= 0; i--)
            {
                if (flagManager.GetFlag(flagToBlockName[i].Flag))
                {
                    return flagToBlockName[i].Flag;
                }
            }
            return Flag.None;
        }
    }
}
