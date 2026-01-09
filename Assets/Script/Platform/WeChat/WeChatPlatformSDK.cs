using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

#if WEIXINMINIGAME && UNITY_WEBGL
    using WeChatWASM;

public class PlatformShareMessage
{
    public string title;
    public string imageUrl;
}

public class WeChatPlatformSDK
{
    
#if WEIXINMINIGAME && UNITY_WEBGL
    private Dictionary<int, WXGameClubButton> _gameClubButtonDict = new();

    public UniTask<int> InitSDK()
    {
        var source = new UniTaskCompletionSource<int>();
        WXBase.InitSDK(code =>
        {
            source.TrySetResult(code);
        });
        return source.Task;
    }

    public void InitFont()
    {
        WXBase.GetWXFont("", (font) =>
        {
            if (font != null)
            {
                TMP_FontAsset fontAsset =
                    TMP_FontAsset.CreateFontAsset(font, 32, 8, GlyphRenderMode.SDFAA, 1024, 1024);
                fontAsset.isMultiAtlasTexturesEnabled = true;
                TMP_Settings.fallbackFontAssets.Add(fontAsset);
            }
        });
    }
    
    /// <summary>
    /// 开启上报
    /// </summary>
    public void ReportGameStart()
    {
        WXBase.ReportGameStart();
    }
    
    /// <summary>
    /// 数据上报
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="data"></param>
    public void SetEvent(string eventId, Dictionary<string, string> data)
    {
        WX.ReportEvent(eventId, data);
    }    
    
    
    public void SetKeepScreenOn()
    {
        WX.SetKeepScreenOn(new SetKeepScreenOnOption
        {
            keepScreenOn = true
        });
    }

    public Dictionary<string, string> GetLaunchOptionsSync()
    {
        var launchOption = WX.GetLaunchOptionsSync();
        return launchOption.query;
    }

    public void ShareMessage(string title, string imageUrl)
    {
        ShareAppMessageOption option = new ShareAppMessageOption();
        option.title = title;
        option.imageUrl = imageUrl;
        WX.ShareAppMessage(option);
    }

    public void OnShareMessage(Action<PlatformShareMessage> action)
    {
        WXShareAppMessageParam defaultParam = new WXShareAppMessageParam();
        defaultParam.title = "title";
        defaultParam.imageUrl = "imageUrl";
        WXBase.OnShareAppMessage(defaultParam, callback =>
        {
            PlatformShareMessage shareMessage = new PlatformShareMessage();
            action.Invoke(shareMessage);
            Debug.Log(shareMessage.imageUrl);
            WXShareAppMessageParam param = new WXShareAppMessageParam();
            param.title = shareMessage.title;
            param.imageUrl = shareMessage.imageUrl;
            callback(param);
        });
    }

    public void SetClipboardData(string data, Action<bool> action)
    {
        WX.SetClipboardData(new SetClipboardDataOption()
        {
            data = data,
            success = result => { action?.Invoke(true); },
            fail = result => { action?.Invoke(false); },
        });
    }

    public void VibrateShort()
    {
        VibrateShortOption option = new VibrateShortOption();
        WX.VibrateShort(option);
    }

    #region 广告
    
    //激励广告
    private WXRewardedVideoAd _rewardedVideoAd;
    private Action<bool> _rewardedVideoAdOnCloseResponse;
    public void CreateRewardedVideoAd(Action<string> onError = null)
    {
        _rewardedVideoAd = WXBase.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam()
        {
            adUnitId = "adunit-fda72d3759eb9dbd",
        });
        
        _rewardedVideoAd.OnClose(res =>
        {
            _rewardedVideoAd.Load();
            _rewardedVideoAdOnCloseResponse?.Invoke(res.isEnded);
        });
        
        _rewardedVideoAd.Load();
    }

    public void ShowRewardedVideoAd(Action<bool> onCloseResponse, Action<string> onSuccess = null, Action<string> onFailed = null)
    {
        _rewardedVideoAdOnCloseResponse = onCloseResponse;
        _rewardedVideoAd.Show(res =>
        {
            onSuccess?.Invoke(res.errMsg);
        }, res =>
        {
            onFailed?.Invoke(res.errMsg);
            _rewardedVideoAd.Load();
        });
    }

    //插屏广告
    private WXInterstitialAd _interstitialAd; 
    private Action _interstitialAdOnCloseResponse;
    public void CreateInterstitialAd(Action<string> onError = null)
    {
        _interstitialAd = WXBase.CreateInterstitialAd(new WXCreateInterstitialAdParam()
        {
            adUnitId = "adunit-93958a1474641f84",
        });
        
        _interstitialAd.OnClose(() =>
        {
            _interstitialAd.Load();
            _interstitialAdOnCloseResponse?.Invoke();
        });
        _interstitialAd.Load();
    }
    
    public void ShowInterstitialAd(Action onCloseResponse, Action<string> onFailed = null)
    {
        _interstitialAdOnCloseResponse = onCloseResponse;
        _interstitialAd.Show(res =>
        {
            Debug.LogError("插屏广告成功回调 " + res.errCode);
        }, res =>
        {
            onFailed?.Invoke(res.errMsg);
            _interstitialAd.Load();
        });
    }
    
    #endregion

    public void RestartApplication()
    {
        WX.RestartMiniProgram(null);
    }

    public void TriggerGC()
    {
        WX.TriggerGC();
    }

#endif
}

#endif

