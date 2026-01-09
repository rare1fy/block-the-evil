using System;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class UICommonTips : UIBase
{
    private CustomText Text_error;

    private Tuple<string, Action> _param;

    public override void InitOnce()
    {
        Text_error = GetNodeByName<CustomText>("_Txt_Error");
        AddListener(ui_listener_type.onClick,"_Btn_Mask", OnBackClick);
        AddListener(ui_listener_type.onClick, "_Btn_Close", OnBackClick);

        AddListener(ui_listener_type.onClick, "_Btn_go", OnBackClick);
    }

    public override void OnOpen(object param = null)
    {
        base.OnOpen(param);
        _param = (Tuple<string, Action>)param;
        Text_error.text = _param.value1;
    }

    protected override void OnBackClick(GameObject go = null, PointerEventData eventData = null)
    {
        if (!ReferenceEquals(_param.value2, null))
        {
            _param.value2();
        }
        base.OnBackClick(go, eventData);
    }
}


