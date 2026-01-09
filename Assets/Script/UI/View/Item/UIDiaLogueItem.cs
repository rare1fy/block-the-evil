using Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDiaLogueItem : UIItemBase
{
    //[BindNode]
    //protected Image _Img_Frame;
    [BindNode]
    protected Image _Img_Head;

    protected override void InitItem()
    {
    }

    public virtual void SetUI(OutWolrdChatBase outWolrd)
    {
        Debug.LogError(outWolrd.GetHead());
        ResourceManagerNew.instance.LoadSpriteAsset(outWolrd.GetHead(), _Img_Head);
    }

}
