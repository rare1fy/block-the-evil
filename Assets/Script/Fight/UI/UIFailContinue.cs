using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIFailContinue : UIBase
{
    [BindNode] private CustomText _Txt_Revive;
    [BindNode] private Image _Img_AD;
    [BindNode] private CustomText _Txt_EndTime;

    public float awaidTime = 10f;
    float remainingTime;
    uint timeId;
    bool ADing = false;
    public override void InitOnce()
    {
        base.InitOnce();
        AddListener(ui_listener_type.onClick, "_Btn_Continue", OnClickContinue);
        AddListener(ui_listener_type.onClick, "_Btn_fangqi", OnClickAbandon);
    }

    public override void OnOpen(object param = null)
    {
        base.OnOpen(param);
        AudioManagerNew.Instance.SetPassFilter(true);
        GameManager.Instance.CurFightControl.IsGameEnd = true;
        
        var curRevive = GameManager.Instance.CurFightControl.LevelController.Model.ReviveTimes;
        var maxRevive = GameManager.Instance.CurFightControl.LevelController.Model.GetMaxReviveTimes();
        AudioManagerNew.Instance.PlayAudio("fight_fail_continue.ogg");
        _Txt_Revive.text = $"本关剩余:{maxRevive - curRevive}/{maxRevive}次";

        var freeAdIndex = Config.GetConfig<Config_GdConstant>().GetConfigById(38).Num;
        var curLvId = GameManager.Instance.CurFightControl.LevelController.Model.LevelBaseData.Id;
        if (curLvId <= freeAdIndex)
        {
            _Img_AD.gameObject.SetActive(false);
        }
        remainingTime = awaidTime;
        SetEndTime();
    }

    private void SetEndTime()
    {
        timeId = TimerManager.instance.AddTimer(1f, () =>
        {
            if (ADing) return;
            remainingTime--;
            if (remainingTime >= 0)
            {
                _Txt_EndTime.text = $"{remainingTime}";
            }
            else
            {
                OnClickAbandon(null,null);
            }
        }, true, false);
    }

    private void OnClickContinue(GameObject _, PointerEventData __)
    {
        var lvMode = GameManager.Instance.CurFightControl.LevelController.Model;
        var curRevive = lvMode.ReviveTimes;
        var maxRevive = lvMode.GetMaxReviveTimes();
        
        if (curRevive >= maxRevive)
        {
            UIManager.Instance.ShowPromptWindow("已无复活次数");
            return;
        }
        
        var freeAdIndex = Config.GetConfig<Config_GdConstant>().GetConfigById(38).Num;
        var curLvId = lvMode.LevelBaseData.Id;
        if (curLvId <= freeAdIndex)
        {
            UIManager.Instance.HideUI(this);
            GameManager.Instance.MusicControl.PlayPickMainBGM();
            GameManager.Instance.CurFightControl.GameContinue();
        }
        else
        {
            ADing = true;
            PlatformManager.Instance.ShowRewardedVideoAd(1, (isOk) =>
            {
                GameManager.Instance.LogManager.Log_AD(1, isOk);
                if (isOk)
                {
                    UIManager.Instance.HideUI(this);
                    GameManager.Instance.MusicControl.PlayPickMainBGM();
                    GameManager.Instance.CurFightControl.GameContinue();
                }
                else
                {
                    ADing = false;
                    UIManager.Instance.ShowPromptWindow("观看时间不足，无法获取奖励");
                }
            });
        }
    }

    private void OnClickAbandon(GameObject _, PointerEventData __)
    {
        UIManager.Instance.HideUI(this);
        EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_FIGHT_END);
    }

    public override void OnClose()
    {
        if (timeId != 0)
        {
            TimerManager.instance.RemoveTimer(timeId);
            timeId = 0;
        }
        base.OnClose();
        AudioManagerNew.Instance.SetPassFilter(false);
    }
}
