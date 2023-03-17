using PluginSet.Core;
using PluginSet.Core.Editor;
using UnityEngine;

namespace PluginSet.QuickSDK.Editor
{
    [BuildChannelsParams("QuickSdk", "QuickSDK参数")]
    [VisibleCaseBoolValue("SupportAndroid", true)]
    [VisibleCaseBoolValue("SupportIOS", true)]
    public class BuildQuickSdkParams: ScriptableObject
    {
        [Tooltip("启用QuickSDK")]
        public bool Enable;

        [Tooltip("是否使用QuickSdk返回的渠道号")]
        [VisibleCaseBoolValue("Enable", true)]
        public bool UseQuickChannel;

        [Tooltip("QuickSdk提供的参数")]
        public string ProductCode;
        
        [Tooltip("QuickSdk提供的参数")]
        public string ProductKey;
        
        [Tooltip("启动时初始化SDK")]
        public bool InitOnLaunch;
    }
}