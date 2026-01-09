using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;
using UnityEngine.UI;

public class UIFightNewStar : UIBase 
{ 
    [BindNode] private CustomText _Txt_Name;
    [BindNode] private CustomText _Txt_Desc;
    [BindNode] private RawImage _Img_UnlockNcp;

    public override void InitOnce()
    {
        base.InitOnce();
        AddListener(ui_listener_type.onClick, "_Btn_Close", OnBackClick);
    }


    public override void OnOpen(object param = null)
    {
        base.OnOpen(param);

        if (param is int npcId)
        {
            var npcCfg = Config.GetConfig<Config_NpcBase>().GetConfigById(npcId);
            ResourceManagerNew.instance.LoadTextureAsset(npcCfg.Img2, _Img_UnlockNcp);
            _Txt_Desc.text = npcCfg.Word;
            _Txt_Name.text = npcCfg.Npcname;
            AudioManagerNew.Instance.PlayAudio("UI_End_UnlockSpNPC.ogg");
            AudioManagerNew.Instance.PlayAudio(npcCfg.Voice1);
        }
    }
}