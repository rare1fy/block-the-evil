using Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDiaLogueImageItem : UIDiaLogueItem
{
    [BindNode]
    private RawImage _Img_Content;

    protected override void InitItem()
    {
    }

    public override void SetUI(OutWolrdChatBase outWolrd)
    {
        base.SetUI(outWolrd);
        if (outWolrd.IsImgCoutent(out var imgName))
        {
            Debug.LogError(imgName);
            ResourceManagerNew.instance.LoadTextureAsset(imgName, _Img_Content);
            _Img_Content.gameObject.SetActiveEx(true);
        }
        else
        {
            _Img_Content.gameObject.SetActiveEx(false);
        }
    }
}
