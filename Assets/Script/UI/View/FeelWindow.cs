using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG;
using DG.Tweening;
using System;
using UnityEngine.EventSystems;
using Framework;

public class FeelWindow : UIBase
{
    //[BindNode]
    //CustomText _Txt_Lv; 
    [BindNode]
    CustomText _Txt_Name;
    [BindNode]
    Image _Img_Lv1;
    [BindNode]
    Image _Img_Lv2;

    int addExp;
    int curExp;
    /// <summary>
    /// 结束回调（处理多个好感提升）
    /// </summary>
    Action CloseAction;

    public override void InitOnce()
    {
    }

    public override void OnOpen(object param = null)
    {
        base.OnOpen(param);
        var data = (Tuple<int, int, Action>)param;//lv,npcId,closeAction
        var cfg = Config.GetConfig<Config_GoodfeelLv>().GetConfigById(data.value1);
        var npcCfg = Config.GetConfig<Config_NpcBase>().GetConfigById(data.value2);
        CloseAction = data.value3;
        _Txt_Name.text = npcCfg.Npcname;
        if (data.value1 >= 10)
        {
            _Img_Lv1.gameObject.SetActiveEx(true);
            _Img_Lv2.gameObject.SetActiveEx(true);
            ResourceManagerNew.instance.LoadSpriteAsset($"UI_zdn_txt_{data.value1 / 10}", _Img_Lv1);
            ResourceManagerNew.instance.LoadSpriteAsset($"UI_zdn_txt_{data.value1 % 10}", _Img_Lv2);
        }
        else
        {
            _Img_Lv1.gameObject.SetActiveEx(false);
            _Img_Lv2.gameObject.SetActiveEx(true);
            ResourceManagerNew.instance.LoadSpriteAsset($"UI_zdn_txt_{data.value1 % 10}", _Img_Lv2);
        }

        DOVirtual.DelayedCall(2f, () =>
        {
            OnBackClick();
        });
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void OnBackClick(GameObject go = null, PointerEventData eventData = null)
    {
        base.OnBackClick(go, eventData);
        CloseAction?.Invoke();
    }

}
