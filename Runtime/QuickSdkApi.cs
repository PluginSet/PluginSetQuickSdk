#if UNITY_EDITOR || ENABLE_QUICKSDK
using System;
using System.Collections.Generic;
using PluginSet.Core;
using PluginSet.Core.MiniJSON;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace PluginSet.QuickSDK
{
    internal static class QuickSdkApi
    {
        private static readonly string EmptyJson = "{}";
        
#if UNITY_ANDROID
        private static AndroidJavaClass ApiClass = null;
#else
        private static IAndroidCallable ApiClass = null;
#endif
        static QuickSdkApi()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            ApiClass = new AndroidJavaClass("com.pluginset.quicksdk.QuickSdkApi");
#endif
        }
        
#if UNITY_ANDROID
        private static AndroidJavaObject CurrentActivity => AndroidHelper.CurrentActivity;
#else
        private static object CurrentActivity = null;
#endif

        public static void SetQuickSdkListener(QuickSdkListener listener)
        {
            ApiClass?.CallStatic("SetQuickSdkListener", listener);
        }

        public static void InitSdk(string productCode, string productKey, SuccessCallback success, FailedCallback failed)
        {
            ApiClass?.CallStatic("InitSdk", CurrentActivity, productCode, productKey, new PluginSetCallback(success, failed));
        }

        public static void Login(SuccessCallback success, FailedCallback failed)
        {
            ApiClass?.CallStatic("Login", CurrentActivity, new PluginSetCallback(success, failed));
        }
        
        public static void Logout(SuccessCallback success, FailedCallback failed)
        {
            ApiClass?.CallStatic("Logout", CurrentActivity, new PluginSetCallback(success, failed));
        }

        public static void SetGameRoleInfo(bool isNewUser, string json)
        {
            ApiClass?.CallStatic("SetGameRoleInfo", CurrentActivity, isNewUser, json);
        }

        public static void Pay(string jsonData, SuccessCallback success, FailedCallback failed)
        {
            ApiClass?.CallStatic("Pay", CurrentActivity, jsonData ?? EmptyJson, new PluginSetCallback(success, failed));
        }

        public static bool HasExitDialog()
        {
            return ApiClass != null && ApiClass.CallStatic<bool>("HasExitDialog", CurrentActivity);
        }

        public static void Exit()
        {
            ApiClass?.CallStatic("Exit", CurrentActivity);
        }

        public static int GetChannelId()
        {
            if (ApiClass == null)
                return -1;

            return ApiClass.CallStatic<int>("GetChannelId", CurrentActivity);
        }
        
        public static int GetParentChannelId()
        {
            if (ApiClass == null)
                return -1;

            return ApiClass.CallStatic<int>("GetParentChannelId", CurrentActivity);
        }
        
        public static string GetQuickSdkDeviceId()
        {
            return ApiClass?.CallStatic<string>("GetQuickSdkDeviceId", CurrentActivity);
        }
        
        public static string GetExtrasConfig(string key)
        {
            return ApiClass?.CallStatic<string>("GetExtrasConfig", CurrentActivity, key);
        }

        public static void UploadEvent(string eventName, string json)
        {
            ApiClass?.CallStatic("UploadEvent", CurrentActivity, eventName, json);
        }
        
        public static bool SupportVerifyRealName()
        {
            return ApiClass != null && ApiClass.CallStatic<bool>("SupportVerifyRealName", CurrentActivity);
        }

        public static void VerifyRealName(SuccessCallback success, FailedCallback failed)
        {
            ApiClass?.CallStatic("VerifyRealName", CurrentActivity, new PluginSetCallback(success, failed));
        }
    }
}
#endif