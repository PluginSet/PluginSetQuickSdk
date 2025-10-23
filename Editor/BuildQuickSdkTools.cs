using System.Collections.Generic;
using System.IO;
using System.Xml;
using PluginSet.Core;
using PluginSet.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace PluginSet.QuickSDK.Editor
{
    [BuildTools]
    public static class BuildQuickSdkTools
    {
        [OnSyncEditorSetting]
        public static void OnSyncEditorSetting(BuildProcessorContext context)
        {
            if (context.BuildTarget != BuildTarget.Android && context.BuildTarget != BuildTarget.iOS)
                return;
                
            var buildParams = context.BuildChannels.Get<BuildQuickSdkParams>();
            if (!buildParams.Enable)
                return;
            
            context.Symbols.Add("ENABLE_QUICKSDK");
            if (buildParams.UseQuickChannel)
                context.Symbols.Add("ENABLE_QUICK_CHANNEL");
            
            context.AddLinkAssembly("PluginSet.QuickSDK");
        }
        
        [AndroidProjectModify(int.MinValue)]
        public static void OnAndroidProjectModify(BuildProcessorContext context, AndroidProjectManager projectManager)
        {
            var buildParams = context.BuildChannels.Get<BuildQuickSdkParams>();
            Debug.Log("Modify android project in QuickSdk check::: " + buildParams.Enable);
            if (!buildParams.Enable)
                return;

            var assetPath = Path.Combine(projectManager.LibraryPath, "src", "main", "assets");
            var configFile = Path.Combine(assetPath, "quicksdk.xml");
            File.WriteAllText(configFile, $"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<resources>\n  <string name=\"product_code\">{buildParams.ProductCode}</string>\n  <string name=\"product_key\">{buildParams.ProductKey}</string>\n</resources>");
            
            Debug.Log("Start Modify android project in QuickSdk!!!");
            
            projectManager.SetApplication("com.pluginset.quicksdk.PluginSetQuickApplication");
            projectManager.SetMainActivity("com.pluginset.quicksdk.SplashActivity");

            var element = projectManager.LibraryManifest.FindOrAddActivity("com.pluginset.quicksdk.PluginSetQuickActivity");
            element.SetAttribute("theme", AndroidConst.NS_URI, "@style/UnityThemeSelector");
            element.SetAttribute("launchMode", AndroidConst.NS_URI, "singleTask");
            element.SetAttribute("configChanges", AndroidConst.NS_URI, "mcc|mnc|locale|touchscreen|keyboard|keyboardHidden|navigation|orientation|screenLayout|uiMode|screenSize|smallestScreenSize|fontScale|layoutDirection|density");
            
            projectManager.LibraryManifest.SetMetaData("unityplayer.ForwardNativeEventsToDalvik", "true", "Set in QuickSdk");
            projectManager.LibraryManifest.SetMetaData("unityplayer.UnityActivity", "true", "Set in QuickSdk");

            switch (PlayerSettings.defaultInterfaceOrientation)
            {
                case UIOrientation.Portrait:
                case UIOrientation.PortraitUpsideDown:
                    SetActivitiesOrientation(projectManager, "portrait");
                    break;
                case UIOrientation.LandscapeLeft:
                case UIOrientation.LandscapeRight:
                    SetActivitiesOrientation(projectManager, "landscape");
                    break;
                case UIOrientation.AutoRotation:
                    if ((PlayerSettings.allowedAutorotateToLandscapeLeft ||
                         PlayerSettings.allowedAutorotateToLandscapeRight)
                        && (PlayerSettings.allowedAutorotateToPortrait ||
                            PlayerSettings.allowedAutorotateToPortraitUpsideDown))
                    {
                        SetActivitiesOrientation(projectManager, "sensor");
                    }
                    else if (PlayerSettings.allowedAutorotateToLandscapeLeft ||
                             PlayerSettings.allowedAutorotateToLandscapeRight)
                    {
                        SetActivitiesOrientation(projectManager, "sensorLandscape");
                    }
                    else if (PlayerSettings.allowedAutorotateToPortrait ||
                             PlayerSettings.allowedAutorotateToPortraitUpsideDown)
                    {
                        SetActivitiesOrientation(projectManager, "sensorPortrait");
                    }
                    else
                    {
                        // default 正常数据不会进入这里
                        SetActivitiesOrientation(projectManager, "sensor");
                    }
                    break;
            }
            
        }

        private static void SetActivitiesOrientation(AndroidProjectManager projectManager, string orientation)
        {
            var activity1 = projectManager.LibraryManifest.FindOrAddActivity("com.pluginset.quicksdk.PluginSetQuickActivity");
            activity1.SetAttribute("screenOrientation", AndroidConst.NS_URI, orientation);

            var activity2 = projectManager.LibraryManifest.FindOrAddActivity("com.pluginset.quicksdk.SplashActivity");
            activity2.SetAttribute("screenOrientation", AndroidConst.NS_URI, orientation);
        }
    }
}
