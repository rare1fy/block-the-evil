using System;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Framework;

public class UIAdsWindow : UIBase
{
    [BindNode] Image _Img_Num1;
    [BindNode] Image _Img_Num2;
    [BindNode] Image _Img_Num3;
    [BindNode] Image _Img_Num4;
    [BindNode] UIGray _Btn_go;
    [BindNode] CustomText _Txt_Num;

    uint time = 0;

    public override void InitOnce()
    {
        AddListener(ui_listener_type.onClick,"_Btn_Mask", OnCancelClick);
        AddListener(ui_listener_type.onClick, "_Btn_Close", OnCancelClick);
        AddListener(ui_listener_type.onClick, "_Btn_go", OnGoClick);
    }

    public override void OnOpen(object param = null)
    {
        base.OnOpen(param);
        StartTime();
        var ctrl = GameManager.Instance.PlayerControl;
        _Txt_Num.text = $"今日剩余:{ctrl.PlayerModel.mainWinAdsCount}次" ;
    }

    public override void OnClose()
    {
        base.OnClose();
        if (time != 0)
        {
            TimerManager.instance.RemoveTimer(time);
            time = 0;
        }
    }

    public void StartTime()
    {
        time = TimerManager.instance.AddTimer(1, SetTime, true, false, -1);
    }

    public void SetTime()
    {
        var ctrl = GameManager.Instance.PlayerControl;
        var cd = ctrl.GetAdsCd();
        string name = $"UI_zdn_xjz_tiaozi_";
        if (cd <= 0)
        {
            ResourceManagerNew.instance.LoadSpriteAsset(name + 0, _Img_Num1);
            ResourceManagerNew.instance.LoadSpriteAsset(name + 0, _Img_Num2);
            ResourceManagerNew.instance.LoadSpriteAsset(name + 0, _Img_Num3);
            ResourceManagerNew.instance.LoadSpriteAsset(name + 0, _Img_Num4);
            _Btn_go.DoGray(false || !ctrl.CheckAdsCount());
        }
        else
        {
            var time = Util.ConvertSecondsToTimeFormatSimple(ctrl.GetAdsCd());
            var m = time.Minutes;
            var s = time.Seconds;
            ResourceManagerNew.instance.LoadSpriteAsset(name + m / 10, _Img_Num1);
            ResourceManagerNew.instance.LoadSpriteAsset(name + m % 10, _Img_Num2);
            ResourceManagerNew.instance.LoadSpriteAsset(name + s / 10, _Img_Num3);
            ResourceManagerNew.instance.LoadSpriteAsset(name + s % 10, _Img_Num4);
            _Btn_go.DoGray(true);
        }
    }

    private void OnGoClick(GameObject go = null, PointerEventData eventData = null)
    {
        var ctrl = GameManager.Instance.PlayerControl;

        if (!ctrl.CheckAdsCount())
        {
            UIManager.Instance.ShowPromptWindow("今天没有赞助商啦，明天来吧！");
            return;
        }

        if (ctrl.CheckAdsCd())
        {
            ctrl.GetAdsCd();
            UIManager.Instance.ShowPromptWindow($"别急，新的赞助商正在路上");
            return;
        }

        PlatformManager.Instance.ShowRewardedVideoAd(9, isOk =>
        {
            GameManager.Instance.LogManager.Log_AD(9, isOk);
            if (isOk)
            {
                EventDispatchCenter.Instance.Dispatch(SDEvents.MAIN_ADS_CALL);
            }
            else
            {
                UIManager.Instance.ShowPromptWindow("观看时间不足，无法获取奖励");
            }
        });
        OnBackClick();
    }


    private void OnCancelClick(GameObject go = null, PointerEventData eventData = null)
    {

        OnBackClick();
    }

    protected override void OnBackClick(GameObject go = null, PointerEventData eventData = null)
    {

        base.OnBackClick(go, eventData);
    }
}


