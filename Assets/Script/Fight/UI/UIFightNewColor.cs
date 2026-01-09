using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;
using UnityEngine.UI;

public class UIFightNewColor : UIBase
{
    [BindNode] private Image _Img_Block;
    [BindNode] private CustomText _Txt_Name;
    [BindNode] private CustomText _Txt_Desc;


    public override void InitOnce()
    {
        base.InitOnce();
        AddListener(ui_listener_type.onClick, "_Btn_Sure", OnBackClick);
    }
    
    public override void OnOpen(object param = null)
    {
        base.OnOpen(param);

        if (param is int colorType)
        {
            var cfg = Config.GetConfig<Config_BlockColor>().GetConfigById(colorType);
            var colorStr=  Config.GetConfig<Config_BlockColor>().GetColorImg(colorType,true);
            _Txt_Desc.text = cfg.Desc;
            _Txt_Name.text = cfg.Name;
            ResourceManagerNew.instance.LoadSpriteAsset(colorStr, _Img_Block);
            AudioManagerNew.Instance.PlayAudio("fight_newblock");
        }
    }
}
