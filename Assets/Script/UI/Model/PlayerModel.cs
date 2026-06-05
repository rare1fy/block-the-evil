using Google.Protobuf.WellKnownTypes;
using System;
using Unity.VisualScripting;
using UnityEngine;
using static Cinemachine.CinemachineTriggerAction.ActionSettings;

public class PlayerModel
{
    private const string PlayerData = "PlayerData";

    private const string PlayerMaxAdsLevel = "MaxAdsLevel";

    private const string PlayerShareTime = "PlayerShareTime";
    private const string PlayerMainWinAdsCD = "PlayerMainWinAdsCD";
    private const string PlayerMainWinAdsCount = "PlayerMainWinAdsCount";

    /// <summary>
    /// 最高完成关卡
    /// </summary>
    private int leavl = 0;
    public int Level
    {
        get { return leavl; }
        private set { }
    }
    /// <summary>
    /// 插屏广告关卡
    /// </summary>
    public int adsLv = 0;

    #region 体力
    /// <summary>
    /// 体力
    /// </summary>
    public int Stamina;
    /// <summary>
    /// 上次体力刷新时间
    /// </summary>
    public long LastStaminaTime;
    /// <summary>
    /// 第一次进入游戏时间
    /// </summary>
    public long OpenTime;
    public int MaxStamina;
    #endregion

    #region 分享时间
    /// <summary>
    /// 上一次每日分享时间
    /// </summary>
    public long shareTime;
    #endregion

    #region 主界面广告
    /// <summary>
    /// 上一次观看主界面广告时间
    /// </summary>
    public long mainWinAdsTime;
    /// <summary>
    /// 今日可观看次数
    /// </summary>
    public int mainWinAdsCount;
    /// <summary>
    /// 每日可观看上限
    /// </summary>
    public int mainWinAdsMaxCount;
    /// <summary>
    /// 广告CD时长
    /// </summary>
    public int AdsCD;

    #endregion

    public PlayerModel()
    {
        AnalysisPrefData();
        AnalysisPrefAdsLv();
        MaxStamina = Config.GetConfig<Config_GdConstant>().GetConfigById(22).Num;//当日体力
        mainWinAdsMaxCount = Config.GetConfig<Config_GdConstant>().GetConfigById(35).Num;
        AdsCD = Config.GetConfig<Config_GdConstant>().GetConfigById(36).Num;
        InitMainWinAD();
        InitShare();
    }

    public void ChangeLeavl(int leavl)
    {
        this.leavl = leavl;
        SetPrefData();
        EventDispatchCenter.Instance.Dispatch(SDEvents.CHANGE_LEAVL);
    }

    private void SetPrefData()
    {
        string strData = "";
        strData += $"{leavl};";
        PlayerPrefs.SetString(PlayerData, strData);
    }

    public void SetAdsLv(int lv)
    {
        adsLv = lv;
        PlayerPrefs.SetInt(PlayerMaxAdsLevel, adsLv);
    }

    private void AnalysisPrefData()
    {
        if (PlayerPrefs.HasKey(PlayerData))
        {
            var strDatas = PlayerPrefs.GetString(PlayerData).Split(";");
            this.leavl = int.Parse(strDatas[0]);
        }
        else
        {
            this.leavl = 0;
            SetPrefData();
        }
    }

    private void AnalysisPrefAdsLv()
    {
        if (PlayerPrefs.HasKey(PlayerMaxAdsLevel))
        {
            adsLv = PlayerPrefs.GetInt(PlayerMaxAdsLevel);
        }
        else
        {
            adsLv = 0;
            SetAdsLv(0);
        }
    }

    public void InitStamina(PlayerData playerData)
    {
        Stamina = playerData.Stamina;
        LastStaminaTime = playerData.LastStaminaTime;
        OpenTime = playerData.OpenTime;
        if (LastStaminaTime == 0) 
        {
            OpenTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            SetStamina();
        }
        else
        {
            var CurrTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (Util.IsCrossDayInEast8(LastStaminaTime, CurrTime))
            {
                SetStamina();
            }
        }
    }

    public void SetStamina()
    {
        LastStaminaTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Stamina = MaxStamina;
        PlayerDataManager.instance.StaminaSave();
    }

    public void InitShare()
    {
        if (PlayerPrefs.HasKey(PlayerShareTime))
        {
            shareTime =  long.Parse(PlayerPrefs.GetString(PlayerShareTime));
        }
        else
        {
            shareTime = 0;
            PlayerPrefs.SetString(PlayerShareTime, shareTime.ToString());
        }
    }

    public void SetShare()
    {
        shareTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        PlayerPrefs.SetString(PlayerShareTime, shareTime.ToString());
    }

    public void InitMainWinAD()
    {
        if (PlayerPrefs.HasKey(PlayerMainWinAdsCD))
        {
            mainWinAdsTime = long.Parse(PlayerPrefs.GetString(PlayerMainWinAdsCD));
            mainWinAdsCount = PlayerPrefs.GetInt(PlayerMainWinAdsCount);
            var unixTimestamp = DateTimeOffset.UtcNow;
            if (Util.IsCrossDayInEast8(mainWinAdsTime, unixTimestamp.ToUnixTimeSeconds()))
            {
                RefreshMainWinAD();
            }
        }
        else
        {
            RefreshMainWinAD();
        }
    }

    public void SetMainWinAD()
    {
        mainWinAdsTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        mainWinAdsCount--;
        PlayerPrefs.SetString(PlayerMainWinAdsCD, mainWinAdsTime.ToString());
        PlayerPrefs.SetInt(PlayerMainWinAdsCount, mainWinAdsCount);
    }

    public void RefreshMainWinAD()
    {
        mainWinAdsCount = mainWinAdsMaxCount;
        mainWinAdsTime = 0;
        PlayerPrefs.SetString(PlayerMainWinAdsCD, mainWinAdsTime.ToString());
        PlayerPrefs.SetInt(PlayerMainWinAdsCount, mainWinAdsCount);

    }

}
