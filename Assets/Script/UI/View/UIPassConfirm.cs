using System;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class UIPassConfirm : UIBase
{
    private CustomText Text_error;

    private Action _param;

    public override void InitOnce()
    {
        AddListener(ui_listener_type.onClick,"_Btn_Mask", OnBackClick);
        AddListener(ui_listener_type.onClick, "_Btn_Close", OnBackClick);
        AddListener(ui_listener_type.onClick, "_Btn_go", OnBackClick);
    }

    public override void OnOpen(object param = null)
    {
        base.OnOpen(param);
        _param = (Action)param;
    }

    protected override void OnBackClick(GameObject go = null, PointerEventData eventData = null)
    {
        _param?.Invoke();
        base.OnBackClick(go, eventData);
    }
}


