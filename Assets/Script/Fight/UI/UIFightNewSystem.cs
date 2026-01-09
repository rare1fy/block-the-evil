using System.Collections;
using System.Collections.Generic;
using Framework;
using Pb;
using UnityEngine;
using UnityEngine.UI;

public class UIFightNewSystem : UIBase
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
        int lv = (int)param;
        MechanismGuild cfg = Config.GetConfig<Config_MechanismGuild>().GetConfigById(lv);
        base.OnOpen(param);
        ResourceManagerNew.instance.LoadSpriteAsset(cfg.Img, _Img_Block);
        _Txt_Name.text = cfg.Name;
        _Txt_Desc.text = cfg.Desc;
        AudioManagerNew.Instance.PlayAudio("fight_newblock");
    }
}
