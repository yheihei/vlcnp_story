using System;
using System.Collections;
using Fungus;
using UnityEngine;
using VLCNP.Actions;
using VLCNP.UI;

namespace VLCNP.Core
{
    [RequireComponent(typeof(InformationTextSpawner))]
    public class MultiFlagGameEvent : MonoBehaviour, ICollisionAction
    {
        FlagManager flagManager;
        bool isAction = true;
        InformationText informationTextObject = null;

        [SerializeField]
        public Flowchart flowChart;

        [SerializeField]
        public FlagConditionToBlockName[] flagConditionToBlockName;

        [Serializable]
        public class FlagCondition
        {
            [SerializeField] Flag flag;
            [SerializeField] bool expectedValue = true;

            public Flag Flag => flag;
            public bool ExpectedValue => expectedValue;
        }

        [Serializable]
        public class FlagConditionToBlockName
        {
            [SerializeField] FlagCondition[] conditions;
            [SerializeField] string blockName;
            [SerializeField] bool isAutoStart;
            [SerializeField] bool isCollisionStart;

            public FlagCondition[] Conditions => conditions;
            public string BlockName => blockName;
            public bool IsAutoStart => isAutoStart;
            public bool IsCollisionStart => isCollisionStart;

            public bool ContainsFlag(Flag flag)
            {
                if (conditions == null) return false;
                for (int i = 0; i < conditions.Length; i++)
                {
                    if (conditions[i] != null && conditions[i].Flag == flag)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        [SerializeField]
        public string InformationText = "(↑)はなす";

        public bool IsAction { get => isAction; set => isAction = value; }

        void Awake()
        {
            flagManager = GameObject.FindWithTag("FlagManager").GetComponent<FlagManager>();
        }

        void Start()
        {
            AutoStartBlock();
            flagManager.OnChangeFlag += OnChangeFlag;
        }

        void OnDestroy()
        {
            if (flagManager == null) return;
            flagManager.OnChangeFlag -= OnChangeFlag;
        }

        void OnChangeFlag(Flag flag, bool value)
        {
            FlagConditionToBlockName current = GetCurrentBlockNameFromConditions();
            if (current == null) return;
            if (!current.IsAutoStart) return;
            if (!current.ContainsFlag(flag)) return;
            StartCoroutine(EventExecute(current));
        }

        void AutoStartBlock()
        {
            FlagConditionToBlockName current = GetCurrentBlockNameFromConditions();
            if (current == null) return;
            if (!current.IsAutoStart) return;
            StartCoroutine(EventExecute(current));
        }

        public void Execute()
        {
            FlagConditionToBlockName current = GetCurrentBlockNameFromConditions();
            if (current == null) return;
            if (current.IsAutoStart) return;
            StartCoroutine(EventExecute(current));
        }

        public void ExecuteCollisionStart()
        {
            FlagConditionToBlockName current = GetCurrentBlockNameFromConditions();
            if (current == null) return;
            if (!current.IsCollisionStart) return;
            StartCoroutine(EventExecute(current));
        }

        IEnumerator EventExecute(FlagConditionToBlockName current)
        {
            if (!isAction) yield break;
            if (current == null) yield break;
            if (string.IsNullOrEmpty(current.BlockName)) yield break;
            yield return ExecuteBlock(current.BlockName);
        }

        IEnumerator ExecuteBlock(string blockName)
        {
            if (flowChart == null) yield break;
            if (flowChart.HasExecutingBlocks()) yield break;
            isAction = false;
            print("ExecuteBlock: " + this + ": " + blockName);
            flowChart.ExecuteBlock(blockName);
            yield return new WaitUntil(() => flowChart.HasExecutingBlocks() == false);
            MenuDialog menuDialog = MenuDialog.ActiveMenuDialog;
            if (menuDialog != null)
            {
                yield return new WaitUntil(() => menuDialog.gameObject.activeSelf == false);
            }
            isAction = true;
        }

        public void ShowInformation()
        {
            if (InformationText == null) return;
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

        public bool IsCollisionStart()
        {
            FlagConditionToBlockName current = GetCurrentBlockNameFromConditions();
            if (current == null) return false;
            return current.IsCollisionStart;
        }

        public FlagConditionToBlockName GetCurrentBlockNameFromConditions()
        {
            if (flagConditionToBlockName == null) return null;
            for (int i = flagConditionToBlockName.Length - 1; i >= 0; i--)
            {
                FlagConditionToBlockName current = flagConditionToBlockName[i];
                if (IsConditionMatched(current))
                {
                    return current;
                }
            }
            return null;
        }

        bool IsConditionMatched(FlagConditionToBlockName current)
        {
            if (current == null) return false;
            FlagCondition[] conditions = current.Conditions;
            if (conditions == null || conditions.Length == 0) return false;
            for (int i = 0; i < conditions.Length; i++)
            {
                if (conditions[i] == null) return false;
                if (flagManager.GetFlag(conditions[i].Flag) != conditions[i].ExpectedValue)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
