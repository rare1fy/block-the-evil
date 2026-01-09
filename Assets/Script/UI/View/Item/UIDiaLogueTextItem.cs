using Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDiaLogueTextItem : UIDiaLogueItem
{
    [BindNode]
    private CustomText _Txt_Content;

    protected override void InitItem()
    {
    }

    public override void SetUI(OutWolrdChatBase outWolrd)
    {
        base.SetUI(outWolrd);
        if (outWolrd.IsTextCoutent(out var txt))
        {
            _Txt_Content.text = txt;
            _Txt_Content.gameObject.SetActiveEx(true);
        }
        else
        {
            _Txt_Content.gameObject.SetActiveEx(false);
        }
    }
}
