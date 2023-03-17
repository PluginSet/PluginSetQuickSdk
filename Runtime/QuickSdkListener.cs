#if UNITY_EDITOR || ENABLE_QUICKSDK
using System;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace PluginSet.QuickSDK
{
    internal interface IQuickSdkListener {
        void onSwitchUser(string userInfo);
        void onSwitchCancel();
        void onSwitchFailed(string message);

        void onExit();
    }
    
    internal class QuickSdkListener :
#if !UNITY_EDITOR && UNITY_ANDROID
        AndroidJavaProxy, 
#endif
        IQuickSdkListener
    {
        public event Action<string> OnSwitchUser;
        public event Action OnSwitchCancel;
        public event Action<string> OnSwitchFailed;
        public event Action OnExit;
        
        public QuickSdkListener()
#if !UNITY_EDITOR && UNITY_ANDROID
        :base("com.pluginset.quicksdk.IQuickSdkListener")
#endif
        {
        }

        public void onSwitchUser(string userInfo)
        {
            OnSwitchUser?.Invoke(userInfo);
        }

        public void onSwitchCancel()
        {
            OnSwitchCancel?.Invoke();
        }

        public void onSwitchFailed(string message)
        {
            OnSwitchFailed?.Invoke(message);
        }

        public void onExit()
        {
            OnExit?.Invoke();
        }
    }
}
#endif