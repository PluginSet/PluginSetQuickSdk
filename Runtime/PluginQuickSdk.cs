#if UNITY_EDITOR || ENABLE_QUICKSDK

using System;
using System.Collections;
using System.Collections.Generic;
using PluginSet.Core;
using PluginSet.Core.MiniJSON;
using quicksdk;
using UnityEngine;
using Logger = PluginSet.Core.Logger;
using Object = UnityEngine.Object;

namespace PluginSet.QuickSDK
{
#if !UNITY_EDITOR
    [PluginRegister]
#endif
    public partial class PluginQuickSdk: PluginBase, IStartPlugin, ILoginPlugin
        , IPaymentPlugin, IUserSet, IExitPlugin
        // , IRealNamePlugin
        // , IAnalytics
        , IPrivacyAuthorizationCallback
        , IQuickSDKListener
    {
        private static readonly Logger Logger = LoggerManager.GetLogger("QuickSdk");
        
        private struct EventData
        {
            public string eventName;
            public string eventData;
        }
        
        public override string Name => "QuickSdk";

        public int StartOrder => PluginsStartOrder.SdkDefault;
        
        public bool IsRunning { get; private set; }

        public bool IsEnableLogin => IsRunning;

        public bool IsLoggedIn => !string.IsNullOrEmpty(_userLoginData);

        public bool IsEnablePayment => IsRunning;

        public bool HasExitDialog => quicksdk.QuickSDK.getInstance().isChannelHasExitDialog();

        public bool IsEnableVerifyRealName => quicksdk.QuickSDK.getInstance().isFunctionSupported(FuncType.QUICK_SDK_FUNC_TYPE_REAL_NAME_REGISTER);

        // private List<EventData> _unreportedEvents;

        private string _userLoginData = null;

        private bool _inited = false;
        
        private Action<Result> _loginCallback;
        private Action<Result> _logoutCallback;
        private Action<Result> _paymentCallback;

        private QuickSDKListener _quickSdkListener;
        private GameRoleInfo _gameRoleInfo;

        protected override void Init(PluginSetConfig config)
        {
            var obj = new GameObject("QuickSDKListener");
            var mono = obj.AddComponent<MonoQuickSDKListener>();
            mono.SetInitListener(this);
            DontDestroyOnLoad(obj);

            _quickSdkListener = mono;
            quicksdk.QuickSDK.getInstance().setListener(_quickSdkListener);
        }

        public IEnumerator StartPlugin()
        {
            if (IsRunning)
                yield break;
            IsRunning = true;
        }

        public void DisposePlugin(bool isAppQuit = false)
        {
        }
        
        public void OnPrivacyAuthorization()
        {
            quicksdk.QuickSDK.getInstance().init();
        }

        private void OnSdkInitSuccess()
        {
            _inited = true;

            // if (_unreportedEvents != null)
            // {
            //     foreach (var data in _unreportedEvents)
            //     {
            //         QuickSdkApi.UploadEvent(data.eventName, data.eventData);
            //     }
            // }
        }

        public void OnSdkInitFailure()
        {
            StartCoroutine(OnPluginInitError());
        }

        public void OnLoginSuccess(UserInfo userInfo)
        {
            _userLoginData = JsonUtility.ToJson(userInfo);
            Logger.Debug($"OnLoginSuccess ::: uid = {userInfo.uid}, data = {_userLoginData} ");
            if (_loginCallback != null)
            {
                _loginCallback.Invoke(new Result()
                {
                    PluginName = Name,
                    Success = true,
                    Code = PluginConstants.SuccessCode,
                    Data = _userLoginData
                });
            }
        }

        public void OnSwitchAccountSuccess(UserInfo userInfo)
        {
            _userLoginData = JsonUtility.ToJson(userInfo);
            if (_loginCallback != null)
            {
                _loginCallback.Invoke(new Result()
                {
                    PluginName = Name,
                    Success = true,
                    Code = PluginConstants.SuccessCode,
                    Data = _userLoginData
                });
            }
        }

        public void OnLoginFailure(string errMsg)
        {
            if (_loginCallback != null)
            {
                _loginCallback.Invoke(new Result()
                {
                    PluginName = Name,
                    Success = false,
                    Code = PluginConstants.FailDefaultCode,
                    Error = errMsg
                });
            }
        }

        public void OnLogoutSuccess()
        {
            if (_logoutCallback != null)
            {
                _logoutCallback.Invoke(new Result()
                {
                    PluginName = Name,
                    Success = true,
                    Code = PluginConstants.SuccessCode,
                });
            }
        }

        public void OnPaySuccess(PayResult payResult)
        {
            if (_paymentCallback != null)
            {
                _paymentCallback.Invoke(new Result()
                {
                    PluginName = Name,
                    Success = true,
                    Code = PluginConstants.SuccessCode,
                    Data = JsonUtility.ToJson(payResult),
                });
            }
        }

        public void OnPayFailure(PayResult payResult)
        {
            if (_paymentCallback != null)
            {
                _paymentCallback.Invoke(new Result()
                {
                    PluginName = Name,
                    Success = false,
                    Code = PluginConstants.FailDefaultCode,
                    Data = JsonUtility.ToJson(payResult),
                    Error = "SDK Error"
                });
            }
        }

        public void OnPayCancel(PayResult payResult)
        {
            if (_paymentCallback != null)
            {
                _paymentCallback.Invoke(new  Result()
                {
                    PluginName = Name,
                    Success = false,
                    Code = PluginConstants.CancelCode,
                    Data = JsonUtility.ToJson(payResult),
                    Error = "User cancel"
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback">
        /// 回调玩家数据：
        /// uid string
        /// userName string
        /// token string
        /// channelToken string
        /// age string
        /// extra string
        /// isQGPay boolean
        /// platformPassword string
        /// platformStatus string
        /// platformUid string
        /// platformUsername string
        /// realName string
        /// stopCreateTime string
        /// </param>
        public void Login(Action<Result> callback = null, string _ = null)
        {
            _loginCallback = callback;
            quicksdk.QuickSDK.getInstance().login();
        }

        public void Logout(Action<Result> callback = null, string _ = null)
        {
            if (!IsLoggedIn)
            {
                callback?.Invoke(new Result
                {
                    PluginName = Name,
                    Success = true,
                    Code = PluginConstants.CancelCode,
                });
                return;
            }
            
            _logoutCallback = callback;
            quicksdk.QuickSDK.getInstance().logout();
        }

        public string GetUserLoginData()
        {
            return _userLoginData;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="callback"></param>
        /// <param name="jsonData">
        /// orderID  string 自定义订单号 
        /// goodsName string 商品名称 = product.name or "",
        /// goodsID string 商品ID
        /// goodsDesc string 商品介绍
        /// count int 商品数量
        /// quantifier string 商品单位
        /// amount = string 价格
        /// price = string 价格
        /// callbackUrl string 回调服务器地址
        /// extrasParams string 扩展传参
        /// externalParams string 扩展传参
        /// </param>
        public void Pay(string productId, Action<Result> callback = null, string jsonData = null)
        {
            _paymentCallback = callback;
            OrderInfo orderInfo = JsonUtility.FromJson<OrderInfo>(jsonData);
            Logger.Debug($"pay start with order: {orderInfo.cpOrderID}");
            quicksdk.QuickSDK.getInstance().pay(orderInfo, _gameRoleInfo);
        }

        public void SetUserInfo(bool isNewUser, string userId, Dictionary<string, object> pairs = null)
        {
            if (pairs == null)
                return;

            if (!IsLoggedIn)
                return;

            var jsonData = Json.Serialize(pairs);
            _gameRoleInfo = JsonUtility.FromJson<GameRoleInfo>(jsonData);
            if (isNewUser)
            {
                quicksdk.QuickSDK.getInstance().createRole(_gameRoleInfo);
            }
            else
            {
                quicksdk.QuickSDK.getInstance().updateRole(_gameRoleInfo);
            }
            Logger.Debug($"SetUserInfo::: json: {jsonData}, roleId:{_gameRoleInfo.gameRoleID} ");
        }

        public void ClearUserInfo()
        {
            _gameRoleInfo = new GameRoleInfo();
        }

        public void FlushUserInfo()
        {
        }

        public void ExitApplication()
        {
            quicksdk.QuickSDK.getInstance().exit();
        }

        // public void VerifyRealName(Action<Result> callback = null)
        // {
        //     if (!IsLoggedIn)
        //     {
        //         callback?.Invoke(new Result
        //         {
        //             PluginName = Name,
        //             Code = PluginConstants.FailDefaultCode,
        //             Success = false,
        //             Error = "No login"
        //         });
        //         return;
        //     }
        //     
        //     QuickSdkApi.VerifyRealName(delegate(string result)
        //     {
        //         Logger.Debug("QuickSdk VerifyRealName success " + result);
        //         callback?.Invoke(new Result
        //         {
        //             PluginName = Name,
        //             Code = PluginConstants.SuccessCode,
        //             Success = true,
        //             Data = result
        //         });
        //     }, delegate(int code, string message)
        //     {
        //         Logger.Debug("QuickSdk VerifyRealName fail " + message);
        //         callback?.Invoke(new Result
        //         {
        //             PluginName = Name,
        //             Code = code,
        //             Success = false,
        //             Error = message
        //         });
        //     });
        // }
        
        // public void CustomEvent(string customEventName, Dictionary<string, object> eventData = null)
        // {
        //     string jsonData = "{}";
        //     if (eventData != null)
        //     {
        //         jsonData = Json.Serialize(eventData);
        //     }
        //
        //     Logger.Debug($"CustomEvent::: event=>{customEventName}, params=>{jsonData}");
        //     if (_inited)
        //     {
        //         QuickSdkApi.UploadEvent(customEventName, jsonData);
        //     }
        //     else
        //     {
        //         if (_unreportedEvents == null)
        //             _unreportedEvents = new List<EventData>();
        //         _unreportedEvents.Add(new EventData
        //         {
        //             eventName = customEventName,
        //             eventData = jsonData,
        //         });
        //     }
        // }

        private void OnSwitchUserSuccess(string userInfo)
        {
            _userLoginData = userInfo;
            SendNotification(PluginConstants.NOTIFY_LOGIN_STATUS_CHANGED);
        }

        private IEnumerator OnPluginInitError()
        {
            var context = PluginsEventContext.Get(this);
            var opCheck = WaitingForOperation.Get(OnPluginInitErrorConfirm, OnPluginInitErrorCancel);
            context.Confirm = opCheck.Confirm;
            context.Cancel = opCheck.Cancel;
            if (NotifyAnyOne(PluginConstants.PLUGIN_INIT_ERROR, context))
            {
                yield return opCheck.Wait();
            }
            else
            {
                yield return OnPluginInitErrorCancel();
            }
            PluginsEventContext.Return(context);
            WaitingForOperation.Return(opCheck);
        }

        private IEnumerator OnPluginInitErrorConfirm()
        {
            _managerInstance.Restart();
            yield break;
        }

        private IEnumerator OnPluginInitErrorCancel()
        {
            Application.Quit(1);
            yield break;
        }

        void IQuickSDKListener.OnSdkInitSuccess()
        {
            OnSdkInitSuccess();
        }
    }
}
#endif