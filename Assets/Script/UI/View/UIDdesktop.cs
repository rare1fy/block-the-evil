using System;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class UIDdesktop : UIBase
{

    public override void InitOnce()
    {
        AddListener(ui_listener_type.onClick,"_Btn_Mask", OnCancelClick);
        AddListener(ui_listener_type.onClick, "_Btn_Close", OnCancelClick);
        AddListener(ui_listener_type.onClick, "_Btn_go", OnGoClick);
    }

    public override void OnOpen(object param = null)
    {
        base.OnOpen(param);
    }

    private void OnGoClick(GameObject go = null, PointerEventData eventData = null)
    {

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


