using Framework;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using Pb;
using System;

public partial class UIFightMain : UIBase
{
    [BindNode(true)] private GameObject _Obj_Wait;
    [BindNode(true)] private GameObject _Obj_TargetNum;
    [BindNode(true)] private GameObject _Obj_TargetOrder;
    [BindNode(true)] private GameObject _Obj_TargetTime;
    [BindNode(true)] private GameObject _Obj_Target;
    [BindNode] private CustomText _Txt_TargetPeople;
    [BindNode] private CustomText _Txt_TargetNum;
    [BindNode] private CustomText _Txt_TargetTime;

    private void OnOpenWait()
    {
        var lv = GameManager.Instance.CurFightControl.LevelController.Model.LevelId;
        LevelBase cfg = Config.GetConfig<Config_LevelBase>().GetConfigById(lv);
        _Obj_TargetOrder.SetActiveEx(cfg.Modle == 1);
        _Obj_TargetNum.SetActiveEx(cfg.Modle == 2);
        _Obj_TargetTime.SetActiveEx(cfg.Modle == 3);
        switch (cfg.Modle)
        {
            case 1:
                _Txt_TargetPeople.text = cfg.Modletarget.ToString();
                break;
            case 2:
                _Txt_TargetNum.text = cfg.Modletarget.ToString();
                break;
            case 3:
                _Txt_TargetTime.text = $"{cfg.Modletarget}S";
                break;
            default:
                break;
        }
        _Obj_Wait.SetActive(false);
        _Obj_Target.SetActiveEx(false);
    }

    public void ShowWait(float time = 2f, Action action = null)
    {
        _Obj_Wait.SetActiveEx(true);
        DOVirtual.DelayedCall(time, () =>
        {
            action?.Invoke();
            _Obj_Wait.SetActiveEx(false);
        });
    }

    public void ShowOrHideTarget(bool isShow)
    {
        _Obj_Target.SetActiveEx(isShow);

        if (isShow)
        {
            foreach (var blockItem in _blockItemList)
            {
                blockItem.CloseNoticeEffect();
            }
        }
    }

    public void PlayTargetAudio()
    {
        DOVirtual.DelayedCall(0.55f, () =>
        {
            AudioManagerNew.Instance.PlayAudio("UI_Reward.ogg");
        });
    }
}