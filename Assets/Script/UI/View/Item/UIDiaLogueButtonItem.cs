using Framework;
using Pb;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDiaLogueButtonItem : UIDiaLogueItem
{
    [BindNode(nodeName: "_Btn_Reword")]
    private Image _Img_Button;
    [BindNode]
    private UIButtonExtension _Btn_Reword;
    [BindNode(nodeName: "_Btn_Reword")]
    private UIGray _Obj_Gray;

    private OutWolrdChatBase data;

    protected override void InitItem()
    {
        AddListener(ui_listener_type.onClick, "_Btn_Reword", OnClickReword);
        EventDispatchCenter.Instance.Registry("DIALOGUE_CLICK_BTN", OnDialogueClock);
    }

    private void OnDialogueClock(object obj)
    {
        if (data.sid == (int)obj)
        {
            _Btn_Reword.enabled = !data.CanGet();
            _Obj_Gray.DoGray(data.CanGet());
        }
    }

    public override void SetUI(OutWolrdChatBase outWolrd)
    {
        data = outWolrd;
        base.SetUI(outWolrd);
        _Btn_Reword.enabled = !outWolrd.CanGet();
        _Obj_Gray.DoGray(outWolrd.CanGet());
        ResourceManagerNew.instance.LoadSpriteAsset(outWolrd.outWord.Txt1, _Img_Button);
    }

    private void OnClickReword(GameObject _, PointerEventData __)
    {
        data.OnClick(1);
    }

    protected override void OnDestroy()
    {
        EventDispatchCenter.Instance.UnRegistry("DIALOGUE_CLICK_BTN", OnDialogueClock);
        base.OnDestroy();
    }
}
