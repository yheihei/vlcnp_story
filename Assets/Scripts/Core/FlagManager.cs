using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VLCNP.Saving;

namespace VLCNP.Core
{
    public class FlagManager : MonoBehaviour, IJsonSaveable
    {
        [SerializeField]
        private Dictionary<Flag, bool> flagDictionary = new();
        public event Action<Flag> OnChangeFlag;

        public bool GetFlag(Flag flag)
        {
            // Noneは常にTrue
            if (flag == Flag.None) return true;
            // keyがなければfalseを返す
            if (!flagDictionary.ContainsKey(flag))
            {
                return false;
            }
            return flagDictionary[flag];
        }

        public void SetFlag(Flag flag, bool value)
        {
            print("SetFlag");
            // keyがなければ追加
            if (!flagDictionary.ContainsKey(flag))
            {
                print("yaru");
                flagDictionary.Add(flag, value);
                OnChangeFlag(flag);
                print("OnChangeFlag");
                return;
            }
            flagDictionary[flag] = value;
            OnChangeFlag(flag);
        }

        public JToken CaptureAsJToken()
        {
            return JToken.FromObject(flagDictionary);
        }

        public void RestoreFromJToken(JToken state)
        {
            print("Restore Flag");
            flagDictionary = state.ToObject<Dictionary<Flag, bool>>();
            print(flagDictionary);
        }
    }
}
