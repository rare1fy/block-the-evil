using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Pb;
using UnityEngine;
using UnityEngine.Rendering;

public class UIFightStart : UIBase  
{
    [BindNode] private CustomText _Txt_Level;
    [BindNode(true)] private GameObject _Obj_Order;
    [BindNode(true)] private GameObject _Obj_Boss;
    [BindNode(true)] private GameObject _Obj_Time;
    [BindNode(true)] private GameObject _Obj_Num;
    [BindNode] private CustomText _Txt_People;
    [BindNode] private CustomText _Txt_Time;
    [BindNode] private CustomText _Txt_Num;

    public override void InitOnce()
    {
        base.InitOnce();
    }

    private int _targetLv;
    public override void OnOpen(object param = null)
    {
        base.OnOpen(param);
        _targetLv = (int)param;
        UIManager.Instance.HideUI("UILoading");
        //GameManager.Instance.PreloadFightStart(_targetLv);
        
        _Txt_Level.text = $"第{_targetLv}天";
        AudioManagerNew.Instance.PlayAudio("fight_partyson");
        LevelBase cfg = Config.GetConfig<Config_LevelBase>().GetConfigById(_targetLv);
        _Obj_Num.SetActiveEx(cfg.Modle == 2);
        _Obj_Order.SetActiveEx(cfg.Modle == 1);
        _Obj_Time.SetActiveEx(cfg.Modle == 3);
        _Obj_Boss.SetActiveEx(cfg.Boss == 1);
        if (cfg.Boss == 1)
        {
            DOVirtual.DelayedCall(0.5f, () =>
            {
                AudioManagerNew.Instance.PlayAudio("UI_hard_rise.ogg");
            });
        }
        switch(cfg.Modle)
        {
            case 1:
                _Txt_People.text = cfg.Modletarget.ToString();
                return;
            case 2:
                _Txt_Num.text = cfg.Modletarget.ToString();
                return;
            case 3:
                _Txt_Time.text = $"{cfg.Modletarget}S";
                return;
            default:
                return;
        }
    }

    public void StartFight()
    {
        UIManager.CutToScene(() =>
            {
                // UIManager.Instance.ShowUI("UIFightMain");
                // GameManager.Instance.CurFightControl.fightStart = true;
                GameManager.Instance.FightStart(_targetLv, 1);
            }
            , () =>
            {
                UIManager.Instance.HideUI(this);
            });
    }
    
}
