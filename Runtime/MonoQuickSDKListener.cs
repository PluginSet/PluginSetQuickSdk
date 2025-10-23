using PluginSet.Core;
using quicksdk;

namespace PluginSet.QuickSDK
{
    internal interface IQuickSDKListener
    {
        void OnSdkInitSuccess();
        void OnSdkInitFailure();
        
        void OnLoginSuccess(UserInfo userInfo);
        void OnSwitchAccountSuccess(UserInfo userInfo);
        void OnLoginFailure(string errMsg);
        
        void OnLogoutSuccess();

        void OnPaySuccess(PayResult payResult);
        void OnPayFailure(PayResult payResult);
        void OnPayCancel(PayResult payResult);
    }
    
    public class MonoQuickSDKListener: QuickSDKListener
    {
        private static Logger Logger = LoggerManager.GetLogger("QuickSdk");
        private IQuickSDKListener _listener;

        internal void SetInitListener(IQuickSDKListener listener)
        {
            _listener = listener;
        }
        
        public override void onInitSuccess()
        {
            Logger.Debug("QuickSDK init success");
            _listener?.OnSdkInitSuccess();
        }

        public override void onInitFailed(ErrorMsg message)
        {
            Logger.Debug($"QuickSDK init failed, error {message.errMsg}");
            _listener?.OnSdkInitFailure();
        }

        public override void onLoginSuccess(UserInfo userInfo)
        {
            Logger.Debug($"QuickSDK onLoginSuccess: {userInfo.userName}");
            _listener?.OnLoginSuccess(userInfo);
        }

        public override void onSwitchAccountSuccess(UserInfo userInfo)
        {
            Logger.Debug($"QuickSDK onSwitchAccountSuccess  >>>>> {userInfo.userName}");
            _listener?.OnSwitchAccountSuccess(userInfo);
        }

        public override void onLoginFailed(ErrorMsg errMsg)
        {
            Logger.Debug($"QuickSDK onLoginFailed, error : {errMsg.errMsg}");
            _listener?.OnLoginFailure(errMsg.errMsg);
        }

        public override void onLogoutSuccess()
        {
            Logger.Debug("QuickSDK onLogoutSuccess");
            _listener?.OnLogoutSuccess();
        }

        public override void onSucceed(string infos)
        {
        }

        public override void onFailed(string message)
        {
        }

        public override void onPaySuccess(PayResult payResult)
        {
            Logger.Debug("QuickSDK onPaySuccess");
            _listener?.OnPaySuccess(payResult);
        }

        public override void onPayFailed(PayResult payResult)
        {
            Logger.Debug("QuickSDK onPayFailed");
            _listener?.OnPayFailure(payResult);
        }

        public override void onPayCancel(PayResult payResult)
        {
            Logger.Debug("QuickSDK onPayCancel");
            _listener?.OnPayCancel(payResult);
        }

        public override void onExitSuccess()
        {
            Logger.Debug("QuickSDK onExitSuccess");
        }

        public override void onPrivaceAgree()
        {
            Logger.Debug("QuickSDK onPrivaceAgree");
        }

        public override void onPrivaceRefuse()
        {
            Logger.Debug("QuickSDK onPrivaceRefuse");
        }

        public override void onCallbackSuccess(string msg)
        {
        }

        public override void onCallbackFaild(string msg)
        {
        }
    }
}