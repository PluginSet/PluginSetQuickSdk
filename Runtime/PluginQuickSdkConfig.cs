#if UNITY_EDITOR || ENABLE_QUICKSDK
using PluginSet.Core;
using UnityEngine;

namespace PluginSet.QuickSDK
{
    [PluginSetConfig("QuickSdk")]
    public class PluginQuickSdkConfig: ScriptableObject
    {
        public string ProductCode;
        public string ProductKey;

        public bool InitOnLaunch;
    }
}
#endif