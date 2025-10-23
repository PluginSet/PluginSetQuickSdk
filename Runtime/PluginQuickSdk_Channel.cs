#if UNITY_EDITOR || ENABLE_QUICKSDK
using PluginSet.Core;

#if ENABLE_QUICK_CHANNEL
namespace PluginSet.QuickSDK
{
    public partial class PluginQuickSdk: IChannel
    {
        public int ChannelPriority => IsRunning ? 1 : int.MinValue;
        
        public string GetChannelName()
        {
            return null;
        }

        public int GetChannelId()
        {
            return quicksdk.QuickSDK.getInstance().channelType();
        }
    }
}
#endif
#endif