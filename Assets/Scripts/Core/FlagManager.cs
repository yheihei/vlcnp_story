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
        public event Action<Flag, bool> OnChangeFlag;

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
            // keyがなければ追加
            if (!flagDictionary.ContainsKey(flag))
            {
                flagDictionary.Add(flag, value);
                OnChangeFlag?.Invoke(flag, value);
                return;
            }
            flagDictionary[flag] = value;
            OnChangeFlag?.Invoke(flag, value);
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
            // 全フラグOnChangeFlagを発火
            foreach (KeyValuePair<Flag, bool> pair in flagDictionary)
            {
                OnChangeFlag?.Invoke(pair.Key, pair.Value);
            }
        }
    }
}
