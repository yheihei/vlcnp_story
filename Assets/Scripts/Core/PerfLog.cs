using System.Diagnostics;
using UnityEngine;

namespace VLCNP.Core
{
    public static class PerfLog
    {
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Log(string message)
        {
            UnityEngine.Debug.Log(message);
        }
    }
}
