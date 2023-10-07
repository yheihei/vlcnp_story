using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Core;

namespace VLCNP.Actions.PostChat
{
    public class PostChat1 : MonoBehaviour, IPostChat
    {
        public void Execute(Flag flag)
        {
            FlagManager flagManager = GameObject.FindWithTag("FlagManager").GetComponent<FlagManager>();
            if (flag == Flag.None) flagManager.SetFlag(Flag.BabyLongOnceChat, true);
        }
    }
}
