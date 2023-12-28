using UnityEngine;
using Fungus;
using System.Threading;
using System.Collections;
using VLCNP.Core;
using VLCNP.UI;
using VLCNP.Saving;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using System.Collections.Concurrent;

namespace VLCNP.Actions
{
    public class Chat : MonoBehaviour, ICollisionAction, IJsonSaveable
    {
        [SerializeField]
        public Flowchart flowChart;
        [SerializeField]
        public string BlockName = "Message";

        [SerializeField]
        public SerializableKeyPair<Flag, string>[] flagToBlockName;

        [Serializable]
        public class SerializableKeyPair<TKey, TValue>
        {
            [SerializeField] private TKey key;
            [SerializeField] private TValue value;
            // 会話終了時にセットするフラグ
            [SerializeField] public Flag afterChatSetFlag;

            public TKey Key => key;
            public TValue Value => value;
        }

        private IPostChat postChat;  // 会話が終わった後に実行する処理
        
        [SerializeField]
        public string TargetTag = "Player";
        [SerializeField]
        public string InformationText = null;

        [SerializeField]
        public bool IsOnce = false;

        [Header("触れると自動でスタートする")]
        [SerializeField]
        bool isAutoStart = false;  // TODO: なぜかController側がキック契機になっている

        [Header("自動でスタート")]
        [SerializeField]
        bool isFlagAwakeStart = false;

        bool isOnceDone = false;

        FlagManager flagManager;

        bool isAction = true;

        InformationText informationTextObject = null;

        public bool IsAction { get => isAction; set => isAction = value; }

        void Awake()
        {
            flagManager = GameObject.FindWithTag("FlagManager").GetComponent<FlagManager>();
            postChat = GetComponent<IPostChat>();
        }

        void Start()
        {
            // flagManager.OnChangeFlag += OnChangeFlag;
            FlagAwakeStart();
        }

        private void FlagAwakeStart()
        {
            if (isFlagAwakeStart)
            {
                Execute();
            }
        }

        public void Execute()
        {
            if (!isAction) return;
            if (flowChart == null) return;
            if (isOnceDone) return;
            if (flowChart.HasExecutingBlocks()) return;
            isAction = false;
            StartCoroutine(Talk());
        }

        public (Flag, string, Flag) GetCurrentBlockNameFromFlag()
        {
            // flagToBlockName を後ろから見ていって、最初に見つかったものを返す
            for (int i = flagToBlockName.Length - 1; i >= 0; i--)
            {
                if (flagManager.GetFlag(flagToBlockName[i].Key))
                {
                    return (flagToBlockName[i].Key, flagToBlockName[i].Value, flagToBlockName[i].afterChatSetFlag);
                }
            }
            return (Flag.None, BlockName, Flag.None);
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.tag != TargetTag) return;
            if (IsOnce)
            {
                if (isOnceDone) return;
                isOnceDone = true;
            }
        }

        IEnumerator Talk() {
            StopAll();
            (Flag currentFlag, string currentBlockName, Flag afterChatSetFlag) = GetCurrentBlockNameFromFlag();
            flowChart.ExecuteBlock(currentBlockName);
            yield return new WaitUntil(() => flowChart.HasExecutingBlocks() == false);
            // メニューが開いていたら閉じるまで待つ
            MenuDialog menuDialog = MenuDialog.ActiveMenuDialog;
            if (menuDialog != null)
            {
                yield return new WaitUntil(() => menuDialog.gameObject.activeSelf == false);
            }
            StartAll();
            isAction = true;
            // 会話終了時にフラグをセットする
            print("afterChatSetFlag: " + afterChatSetFlag);
            if (afterChatSetFlag != Flag.None) flagManager.SetFlag(afterChatSetFlag, true);
            postChat?.Execute(currentFlag);
        }

        public void ShowInformation()
        {
            if (InformationText == null) return;
            if (isOnceDone) return;
            InformationTextSpawner spawner = GetComponent<InformationTextSpawner>();
            if (spawner == null) return;
            if (informationTextObject != null) return;
            informationTextObject = spawner.Spawn(InformationText);
        }

        public void HideInformation()
        {
            if (informationTextObject == null) return;
            Destroy(informationTextObject.gameObject);
        }

        private void StopAll()
        {
            foreach (MonoBehaviour obj in FindObjectsOfType<MonoBehaviour>())
            {
                IStoppable stoppable = obj.GetComponent<IStoppable>();
                if (stoppable == null) continue;
                stoppable.IsStopped = true;
            }
        }

        private void StartAll()
        {
            foreach (MonoBehaviour obj in FindObjectsOfType<MonoBehaviour>())
            {
                IStoppable stoppable = obj.transform.GetComponent<IStoppable>();
                if (stoppable == null) continue;
                stoppable.IsStopped = false;
            }
        }

        public JToken CaptureAsJToken()
        {
            return JToken.FromObject(isOnceDone);
        }

        public void RestoreFromJToken(JToken state)
        {
            isOnceDone = state.ToObject<bool>();
        }

        public bool IsAutoStart()
        {
            return isAutoStart;
        }
    }
}
