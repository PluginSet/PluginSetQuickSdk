#if UNITY_EDITOR || ENABLE_QUICKSDK

using System;
using System.Collections;
using System.Collections.Generic;
using PluginSet.Core;
using PluginSet.Core.MiniJSON;
using UnityEngine;
using Logger = PluginSet.Core.Logger;

namespace PluginSet.QuickSDK
{
#if !UNITY_EDITOR
    [PluginRegister]
#endif
    public partial class PluginQuickSdk: PluginBase, IStartPlugin, ILoginPlugin, IPaymentPlugin, IUserSet, IExitPlugin, IRealNamePlugin, IAnalytics
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

        public bool HasExitDialog => QuickSdkApi.HasExitDialog();

        public bool IsEnableVerifyRealName => QuickSdkApi.SupportVerifyRealName();

        private List<EventData> _unreportedEvents;

        private string _userLoginData = null;

        private string _productCode;
        private string _productKey;
        private bool _initOnLaunch;

        private bool _inited = false;

        private bool _initFailed;

        private QuickSdkListener _quickSdkListener;

        protected override void Init(PluginSetConfig config)
        {
            var data = config.Get<PluginQuickSdkConfig>();
            _productCode = data.ProductCode;
            _productKey = data.ProductKey;
            _initOnLaunch = data.InitOnLaunch;
            
            _quickSdkListener = new QuickSdkListener();
            _quickSdkListener.OnExit += Application.Quit;
            _quickSdkListener.OnSwitchUser += OnSwitchUserSuccess;
        }

        public IEnumerator StartPlugin()
        {
            if (IsRunning)
                yield break;

            _initFailed = false;
            
            QuickSdkApi.SetQuickSdkListener(_quickSdkListener);

            if (_initOnLaunch)
            {
                InitSdk(delegate(bool success)
                {
                    if (success)
                    {
                        IsRunning = true;
                        _initFailed = false;
                    }
                    else
                    {
                        StartCoroutine(OnPluginInitError());
                    }
                });
                
                yield return new WaitUntil(() => IsRunning || _initFailed);
                Debug.Log("QuickSdk init result:::: " + IsRunning);
            }
            else
            {
                IsRunning = true;
            }
        }

        public void DisposePlugin(bool isAppQuit = false)
        {
        }

        private void InitSdk(Action<bool> callback)
        {
            if (_inited)
            {
                callback?.Invoke(true);
                return;
            }
            
            QuickSdkApi.InitSdk(_productCode, _productKey, delegate
            {
                OnSdkInitSuccess();
                callback?.Invoke(true);

            }, delegate(int code, string message)
            {
                Logger.Error($"Failed init QuickSDK:: code: {code}, message:{message}");
                callback?.Invoke(false);
            });
            
        }

        private void OnSdkInitSuccess()
        {
            _inited = true;

            if (_unreportedEvents != null)
            {
                foreach (var data in _unreportedEvents)
                {
                    QuickSdkApi.UploadEvent(data.eventName, data.eventData);
                }
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
            InitSdk(delegate(bool success)
            {
                if (success)
                {
                    QuickSdkApi.Login(delegate(string userInfo)
                    {
                        _userLoginData = userInfo;
                        callback?.Invoke(new Result
                        {
                            PluginName = Name,
                            Success = true,
                            Code = PluginConstants.SuccessCode,
                            Data = userInfo
                        });
                    }, delegate(int code, string message)
                    {
                        callback?.Invoke(new Result
                        {
                            PluginName = Name,
                            Success = false,
                            Code = code,
                            Error = message
                        });
                    });
                }
                else
                {
                    callback?.Invoke(new Result
                    {
                        PluginName = Name,
                        Success = false,
                        Code = PluginConstants.FailDefaultCode,
                        Error = "Init fail"
                    });
                }
            });
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
            
            QuickSdkApi.Logout(delegate
            {
                _userLoginData = null;
                callback?.Invoke(new Result
                {
                    PluginName = Name,
                    Success = true,
                    Code = PluginConstants.SuccessCode,
                });
                
                SendNotification(PluginConstants.NOTIFY_LOGIN_STATUS_CHANGED);
            }, delegate(int code, string message)
            {
                callback?.Invoke(new Result
                {
                    PluginName = Name,
                    Success = false,
                    Code = code,
                    Error = message
                });
            });
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
            InitSdk(delegate(bool success)
            {
                if (success)
                {
                    QuickSdkApi.Pay(jsonData, delegate(string result)
                    {
                        callback?.Invoke(new Result
                        {
                            PluginName = Name,
                            Success = true,
                            Code = PluginConstants.SuccessCode,
                            Data = result
                        });
                    }, delegate(int code, string message)
                    {
                        callback?.Invoke(new Result
                        {
                            PluginName = Name,
                            Success = false,
                            Code = code,
                            Error = message
                        });
                    });
                }
                else
                {
                    callback?.Invoke(new Result
                    {
                        PluginName = Name,
                        Success = false,
                        Code = PluginConstants.FailDefaultCode,
                        Error = "Init fail"
                    });
                }
            });
        }

        public void SetUserInfo(bool isNewUser, string userId, Dictionary<string, object> pairs = null)
        {
            if (pairs == null)
                return;

            if (!IsLoggedIn)
                return;

            var jsonData = Json.Serialize(pairs);
            Logger.Debug("SetUserInfo::: " + jsonData);
            QuickSdkApi.SetGameRoleInfo(isNewUser, jsonData);
        }

        public void ClearUserInfo()
        {
        }

        public void FlushUserInfo()
        {
        }

        public void ExitApplication()
        {
            QuickSdkApi.Exit();
        }

        public void VerifyRealName(Action<Result> callback = null)
        {
            if (!IsLoggedIn)
            {
                callback?.Invoke(new Result
                {
                    PluginName = Name,
                    Code = PluginConstants.FailDefaultCode,
                    Success = false,
                    Error = "No login"
                });
                return;
            }
            
            QuickSdkApi.VerifyRealName(delegate(string result)
            {
                Logger.Debug("QuickSdk VerifyRealName success " + result);
                callback?.Invoke(new Result
                {
                    PluginName = Name,
                    Code = PluginConstants.SuccessCode,
                    Success = true,
                    Data = result
                });
            }, delegate(int code, string message)
            {
                Logger.Debug("QuickSdk VerifyRealName fail " + message);
                callback?.Invoke(new Result
                {
                    PluginName = Name,
                    Code = code,
                    Success = false,
                    Error = message
                });
            });
        }
        
        public void CustomEvent(string customEventName, Dictionary<string, object> eventData = null)
        {
            string jsonData = "{}";
            if (eventData != null)
            {
                jsonData = Json.Serialize(eventData);
            }

            Logger.Debug($"CustomEvent::: event=>{customEventName}, params=>{jsonData}");
            if (_inited)
            {
                QuickSdkApi.UploadEvent(customEventName, jsonData);
            }
            else
            {
                if (_unreportedEvents == null)
                    _unreportedEvents = new List<EventData>();
                _unreportedEvents.Add(new EventData
                {
                    eventName = customEventName,
                    eventData = jsonData,
                });
            }
        }

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
            _initFailed = true;
            _managerInstance.Restart();
            yield break;
        }

        private IEnumerator OnPluginInitErrorCancel()
        {
            Application.Quit(1);
            yield break;
        }
        
    }
}
#endif