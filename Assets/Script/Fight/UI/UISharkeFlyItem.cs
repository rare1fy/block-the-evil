using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using Framework;
using UnityEngine;
using UnityEngine.UI;

public class UISharkeFlyItem : UIBase
{
    public int idx;

    public void SetItem()
    {
        var imgs = GetComponentsInChildren<Image>();
        var data = GameManager.Instance.CurFightControl.Model.GetPuzzleData(idx);
        this.gameObject.SetActiveEx(!data.bUsed);
        foreach (var img in imgs)
        {
             var colorStr = Config.GetConfig<Config_BlockColor>().GetColorImg(data.ColorType);
            ResourceManagerNew.instance.LoadSpriteAsset(colorStr, img);
        }
    }
}
