using System;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public class PlayerControl : BaseControl
{
    private PlayerModel _playerModel;
    public PlayerModel PlayerModel {  
        
        get { 
            if (_playerModel == null)
                _playerModel = new PlayerModel();
            return _playerModel; 
        }
        set { _playerModel = value; }
    }

    protected override void OnInitControl()
    {
        LaunchManager.Instance.RegisterSystemWaitForInit();
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_LOAD_PLAYERDATA_FINISH, InitPlayerData);
    }
    
    protected override void OnCloseControl()
    {
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_LOAD_PLAYERDATA_FINISH, InitPlayerData);
    }
    
    private void InitPlayerData(object param)
    {
        if (param is PlayerData playerData)
        {
            PlayerModel.InitStamina(playerData);
            StartTime();
            
            LaunchManager.Instance.MaskSystemReady();
        }
    }
    

    #region 体力和跨天

    /// <summary>
    /// 跨天倒计时
    /// </summary>
    public void StartTime()
    {
        var await = Util.GetTimeUntilNextCrossDay(null).TotalSeconds;
        TimerManager.instance.AddTimer(1, () =>
        {
            if(await <= 0)
            {
                PlayerModel.SetStamina();
                PlayerModel.RefreshMainWinAD();
                EventDispatchCenter.Instance.Dispatch(SDEvents.ACROSS_THE_DAY);
                await = Util.GetTimeUntilNextCrossDay(null).TotalSeconds;
            }
            await--;
        }, false, false, -1);
    }

    public void AddStamina(int num)
    {
        if (PlayerModel.Stamina == PlayerModel.MaxStamina && num > 0)
            return;
        PlayerModel.Stamina += num;
        PlayerDataManager.instance.StaminaSave();
    }

    #endregion

    #region 主界面分享和广告

    /// <summary>
    /// 广告按钮是否CD中
    /// </summary>
    /// <returns></returns>
    public bool CheckAdsCd()
    {
        return PlayerModel.mainWinAdsTime + PlayerModel.AdsCD > DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public long GetAdsCd()
    {
        return PlayerModel.mainWinAdsTime + PlayerModel.AdsCD - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    /// <summary>
    /// 今日可看广告次数
    /// </summary>
    /// <returns></returns>
    public bool CheckAdsCount()
    {
        return PlayerModel.mainWinAdsCount > 0;
    }

    /// <summary>
    /// 是否今日首次分享
    /// </summary>
    /// <returns></returns>
    public bool CheckShare()
    {
        return Util.IsCrossDayInEast8(PlayerModel.shareTime, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }

    #endregion

    public void EndFight(int leavl, List<NpcInfo> npcInfoList, List<ItemConfig> itemConfigs,bool isEndless)
    {
        GameManager.Instance.GameBagControl.UpdateItems(itemConfigs);
        GameManager.Instance.NpcControl.ChangeNpcDatas(npcInfoList);
        if (!isEndless)
        {
            AddStamina(1);
        }

        if (!GameManager.Instance.CurFightControl.LevelController.Model.IsEndLess)
            PlayerModel.ChangeLeavl(leavl);
    }

    public void StartFightHasTips(Action action = null)
    {
        int level = PlayerModel.Level;
        var maxLevel = Config.GetConfig<Config_LevelBase>().m_LevelBaseDic.Count;
        if (level >= maxLevel)
        {
            UIManager.Instance.ShowUI("UIPassConfirm", param: null);
        }
        else
        {
            if (PlayerModel.Stamina <= 0)
            {
                UIManager.Instance.ShowSecondConfirm("体力不足，是否观看广告重新开始!!!", () =>
                {
                    PlatformManager.Instance.ShowRewardedVideoAd(2, (isOk) =>
                    {
                        GameManager.Instance.LogManager.Log_AD(2, isOk);
                        if (isOk)
                        {
                            action?.Invoke();
                            UIManager.Instance.CloseAll(true);
                            UIManager.Instance.ShowUI("UIFightStart", null, level + 1);
                            AudioManagerNew.Instance.FadeStopMusic();
                            GameManager.Instance.MusicControl.PlayPickMainBGM();
                        }
                        else
                        {
                            UIManager.Instance.ShowPromptWindow("观看时间不足，无法获取奖励");
                        }
                    });
                    
                });
            }
            else
            {
                UIManager.Instance.ShowSecondConfirm("是否消耗体力重新开始", () =>
                {
                    action?.Invoke();
                    UIManager.Instance.CloseAll(true);
                    UIManager.Instance.ShowUI("UIFightStart", null, level + 1);
                    AudioManagerNew.Instance.FadeStopMusic();
                    GameManager.Instance.MusicControl.PlayPickMainBGM();
                    AddStamina(-1);
                });
            }
        }
    }

    public void StartFight(Action action = null)
    {
        int level = PlayerModel.Level;
        var maxLevel = Config.GetConfig<Config_LevelBase>().m_LevelBaseDic.Count;
        if (level >= maxLevel) 
        {
            UIManager.Instance.ShowUI("UIPassConfirm",param :null);
        }
        else
        {
            if(PlayerModel.Stamina <= 0)
            {
                PlatformManager.Instance.ShowRewardedVideoAd(2, (isOk) =>
                {
                    GameManager.Instance.LogManager.Log_AD(2, isOk);
                    if (isOk)
                    {
                        action?.Invoke();
                        UIManager.Instance.CloseAll(true);
                        UIManager.Instance.ShowUI("UIFightStart", null, level + 1);
                        AudioManagerNew.Instance.FadeStopMusic();
                        GameManager.Instance.MusicControl.PlayPickMainBGM();
                    }
                    else
                    {
                        UIManager.Instance.ShowPromptWindow("观看时间不足，无法获取奖励");
                    }
                });
                return;
            }

            if (level >= 10 && level % 5 == 0 && level > PlayerModel.adsLv)
            {
                PlayerModel.SetAdsLv(level);
                UIManager.Instance.ShowUI("ScreenAdsWindow", null,new Tuple<Action> (() =>
                {
                    action?.Invoke();
                    UIManager.Instance.CloseAll(true);
                    UIManager.Instance.ShowUI("UIFightStart", null, level + 1);
                    AudioManagerNew.Instance.FadeStopMusic();
                    GameManager.Instance.MusicControl.PlayPickMainBGM();
                    AddStamina(-1);
                }));
                return;
            }
            action?.Invoke();
            UIManager.Instance.CloseAll(true);
            UIManager.Instance.ShowUI("UIFightStart", null, level + 1);
            AudioManagerNew.Instance.FadeStopMusic();
            GameManager.Instance.MusicControl.PlayPickMainBGM();
            AddStamina(-1);
        }
    }
}

