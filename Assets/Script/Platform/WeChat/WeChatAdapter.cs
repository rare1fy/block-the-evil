#if WEIXINMINIGAME
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Newtonsoft.Json;
using UnityEngine;
using WeChatWASM;

public class WeChatAdapter : IPlatformAdapter
{
    public WeChatPlatformSDK WxSDK;
    public async Task Initialize()
    {
        WxSDK = new WeChatPlatformSDK();
        var sdkResult = await WxSDK.InitSDK();
        WxSDK.ReportGameStart();
        ChuXinWeChatSDK.Instance.InitSDK
        ("fkyqgwx",
            "6AFW6X35rVva",
            "yuFyY5OjI2fMMZrWqU4U",
            false,
            YooAssetManager.AppVersion,
            "com.wangfeng.fkyqg.wx",
            b =>
            {
                Debug.Log($"初始化完成:{b}");
                LoginNoServer();
            }
        );
    }
    
    public void RegisterEvents()
    {
        // WxSDK.CreateRewardedVideoAd();
        // WxSDK.CreateInterstitialAd();
        WxJsManager.Instance.RegisterWeChatCallbacks();
    }
    
    public void LoadData(Action<PlayerData> onComplete)
    {
        WxJsManager.Instance.LoadDataFromJson("PlayerData", (jsonData) =>
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                onComplete?.Invoke(new PlayerData());
            }
            else
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<PlayerData>(jsonData);
                    onComplete?.Invoke(obj ?? new PlayerData());
                }
                catch (Exception e)
                {
                    Debug.LogError("解析存档失败: " + e.Message);
                    onComplete?.Invoke(new PlayerData());
                }
            }
        });
    }

    public void SaveData(PlayerData data)
    {
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        WxJsManager.Instance.SaveDataToJson("PlayerData", json);
    }

    public void ShareMessage(string title, string imageUrl, Action<object> onFinish)
    {
        WxSDK.ShareMessage(title, imageUrl);
    }

    public void ShortVibration(int level)
    {
        WxJsManager.Instance.TriggerShortVibration(level);
    }

    public void ShowRewardedVideoAd(int adType, Action<bool> onCloseResponse, Action<string> onFailed = null)
    {
        var adCfg = Config.GetConfig<Config_AdDesc>().GetConfigById(adType);
        ReportAdEvent(1, false, false, 1, adCfg.Ad);
        ChuXinWeChatSDK.Instance.ShowRewardAdSDK(isFinish =>
        {
            var action = isFinish ? 3 : 2;
            ReportAdEvent(1, false, isFinish, action, adCfg.Ad);
            onCloseResponse?.Invoke(isFinish);
        }, adType.ToString(), adType);
        
        //WxSDK.ShowRewardedVideoAd(onCloseResponse, onFailed);
    }

    public void ShowInterstitialAd(Action onCloseResponse, int adType, Action<string> onFailed = null)
    {
        WxSDK.ShowInterstitialAd(onCloseResponse, onFailed);
    }

    private LoginCheckData LoginCheckData;
    private void LoginNoServer()
    {
        ChuXinWeChatSDK.Instance.LoginSDKNoServer((b, data) =>
        {
            LoginCheckData = data;
            Debug.Log($"无服务器登录成功  uid={LoginCheckData.uid}");
            ChuXinWeChatSDK.Instance.SetUserInfoSDK(data.uid, data.session, data.open_id, data.user_name);
        });
    }
    
    private RequestBaseData CreateBaseData()
    {
        RequestBaseData data = new RequestBaseData
        {
            log_id = "__UUID__",
            game = LoginCheckData.game,
            platform = LoginCheckData.platform,
            platform_id = 0,
            server_id = 0,
            server_name = "",
            timestamp = "__TS__",
            account = LoginCheckData.uid,
            role_id = LoginCheckData.uid,
            role_name = "",
            grade = 0,
            ip = "__IP__",
            version = YooAssetManager.AppVersion
        };
        return data;
    }
    

    private void ReportEvent(string funcName, int checkId, string checkName, Dictionary<string, object> extraFields)
    {
        var data = CreateBaseData();
        data.func = funcName;
        data.check_id = checkId;   
        data.check_name = checkName;

        var result = new Dictionary<string, object>();
        foreach (var f in typeof(RequestBaseData).GetFields())
            result[f.Name] = f.GetValue(data);

        if (extraFields != null)
        {
            foreach (var kv in extraFields)
                result[kv.Key] = kv.Value;
        }

        var json = JsonConvert.SerializeObject(result, Formatting.None);
        WebRequestManager.Instance.PostJSON("https://iaa-log.chuxinhd.com/api/v1/log/log?env=prod", json,
            s =>
            {
                Debug.Log("上报成功:"+ s);
            },
            f =>
            {
                Debug.LogError("上报失败" + f);
                Debug.LogError("自定义上报内容:"+ json);
            },headers: GetFixedHeaders());
        //ChuXinWeChatSDK.Instance.ReportCustomEvent(funcName, funcName, json);
    }
    
    private Dictionary<string, string> GetFixedHeaders()
    {
        return new Dictionary<string, string>
        {
            {"Accept", "application/json"},
            {"Content-Type", "application/json"}
        };
    }
    
    
    //上报自定义事件
    public void ReportCustomEvent(string funcType, Dictionary<string, object> fields, int checkId = 0, string checkName = "")
    {
        ReportEvent(funcType, checkId, checkName, fields);
    }
    
    /// <summary>
    /// 上报广告事件
    /// </summary>
    /// <param name="adType">广告类型(1.激励 2.banner 3.插屏 4.开屏)</param>
    /// <param name="isSkip">是否是跳过</param>
    /// <param name="isFinish">是否完播</param>
    /// <param name="action">类型：1开始 2中途退出 3完播 4曝光</param>
    /// <param name="explain">广告打点位置</param>
    /// <param name="checkId">关卡id(没有填0)</param>
    /// <param name="checkName">关卡名称</param>
    public void ReportAdEvent(int adType, bool isSkip, bool isFinish, int action, string explain)
    {
        var fightCtrl = GameManager.Instance.CurFightControl;
        var checkId = 0;
        var checkName = "";
        if (fightCtrl.fightStart)
        {
            checkId = fightCtrl.LevelController.Model.LevelId;
            checkName = fightCtrl.LevelController.Model.IsEndLess ? "无尽" : "普通";
        }
        
        var fields = new Dictionary<string, object>
        {
            { "ad_type", adType },
            { "is_skip", isSkip },
            { "is_finish", isFinish },
            { "action", action },
            { "point_from", explain }
        };
        
        ReportEvent("watchAd", checkId, checkName, fields);
    }
}

#endif
