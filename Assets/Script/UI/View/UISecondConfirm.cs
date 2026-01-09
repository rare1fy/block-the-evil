using System;
using UnityEngine.EventSystems;
using UnityEngine;

public class UISecondConfirm : UIBase
{
    private CustomText Text_error;
    [BindNode(true)] GameObject _Obj_AD;
    private Tuple<string, Action, Action, bool> _param;

    public override void InitOnce()
    {
        Text_error = GetNodeByName<CustomText>("_Txt_Error");
        AddListener(ui_listener_type.onClick,"_Btn_Mask", OnCancelClick);
        AddListener(ui_listener_type.onClick, "_Btn_Close", OnCancelClick);
        AddListener(ui_listener_type.onClick, "_Btn_go", OnGoClick);
        AddListener(ui_listener_type.onClick, "_Btn_cancel", OnCancelClick);
    }

    public override void OnOpen(object param = null)
    {
        base.OnOpen(param);
        _param = (Tuple<string, Action, Action, bool>)param;
        _Obj_AD.SetActiveEx(_param.value4);
        Text_error.text = _param.value1;
    }

    private void OnGoClick(GameObject go = null, PointerEventData eventData = null)
    {
        if (!ReferenceEquals(_param.value3, null))
        {
            _param.value3();
        }
        OnBackClick();
    }


    private void OnCancelClick(GameObject go = null, PointerEventData eventData = null)
    {
        if (!ReferenceEquals(_param.value2, null))
        {
            _param.value2();
        }
        OnBackClick();
    }

    protected override void OnBackClick(GameObject go = null, PointerEventData eventData = null)
    {

        base.OnBackClick(go, eventData);
    }
}


