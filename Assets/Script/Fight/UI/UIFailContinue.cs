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
        
        var levelModel = GameManager.Instance.CurFightControl.LevelController.Model;
        AudioManagerNew.Instance.PlayAudio("fight_fail_continue.ogg");
        if (levelModel.IsEndLess)
        {
            var maxRevive = levelModel.GetMaxReviveTimes();
            _Txt_Revive.text = $"本关剩余:{maxRevive - levelModel.ReviveTimes}/{maxRevive}次";
            _Img_AD.gameObject.SetActiveEx(levelModel.ReviveTimes < maxRevive);
        }
        else if (levelModel.CanUseFreeAdRevive())
        {
            _Txt_Revive.text = "观看广告复活";
            _Img_AD.gameObject.SetActiveEx(true);
        }
        else
        {
            _Txt_Revive.text = $"铜板复活:{levelModel.GetNextCopperReviveCost()}";
            _Img_AD.gameObject.SetActiveEx(false);
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

        if (lvMode.IsEndLess)
        {
            TryEndlessRevive(lvMode);
            return;
        }

        if (lvMode.CanUseFreeAdRevive())
        {
            TryFreeAdRevive(lvMode);
        }
        else
        {
            TryCopperRevive(lvMode);
        }
    }

    private void TryEndlessRevive(FightLevelModel lvMode)
    {
        var maxRevive = lvMode.GetMaxReviveTimes();
        if (lvMode.ReviveTimes >= maxRevive)
        {
            UIManager.Instance.ShowPromptWindow("已无复活次数");
            return;
        }

        TryFreeAdRevive(lvMode);
    }

    private void TryFreeAdRevive(FightLevelModel lvMode)
    {
        ADing = true;
        PlatformManager.Instance.ShowRewardedVideoAd(1, (isOk) =>
        {
            GameManager.Instance.LogManager.Log_AD(1, isOk);
            if (isOk)
            {
                if (!lvMode.IsEndLess)
                    lvMode.MarkFreeAdReviveUsed();

                ContinueFight();
            }
            else
            {
                ADing = false;
                UIManager.Instance.ShowPromptWindow("观看时间不足，无法获取奖励");
            }
        });
    }

    private void TryCopperRevive(FightLevelModel lvMode)
    {
        var cost = lvMode.GetNextCopperReviveCost();
        if (GameManager.Instance.CurFightControl.Model.Money < cost)
        {
            UIManager.Instance.ShowPromptWindow("铜板不足");
            return;
        }

        GameManager.Instance.CurFightControl.Model.AddMoney(-cost);
        lvMode.MarkCopperReviveUsed();
        ContinueFight();
    }

    private void ContinueFight()
    {
        UIManager.Instance.HideUI(this);
        GameManager.Instance.MusicControl.PlayPickMainBGM();
        GameManager.Instance.CurFightControl.GameContinue();
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
