using PluginSet.Core.Editor;
using UnityEditor;

namespace PluginSet.QuickSDK.Editor
{
    [InitializeOnLoad]
    public static class PluginQuickSdkFilter
    {
        static PluginQuickSdkFilter()
        {
            var fileter = PluginFilter.IsBuildParamsEnable<BuildQuickSdkParams>();
            PluginFilter.RegisterFilter("com.pluginset.quicksdk/Plugins/Android", fileter);
        }
    }
    
}