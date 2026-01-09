using Framework;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIChatItem : UIItemBase
{
    [BindNode] 
    private RawImage _Img_Head;

    [BindNode] 
    private CustomText _Txt_Count;

    [BindNode] 
    private CustomText _Txt_Name;
    [BindNode]
    private Image _Img_FeelLv1;
    [BindNode]
    private Image _Img_FeelLv2;
    [BindNode(true)]
    private GameObject _Obj_RedPoint;
    [BindNode(true)]
    private GameObject _Obj_Omit;

    /// <summary>
    /// 聊天数据
    /// </summary>
    private DialogueData data;

    protected override void InitItem()
    {
        AddListener(ui_listener_type.onClick, "_Btn_Bg", OnClickBG);
        EventDispatchCenter.Instance.Registry(SDEvents.DIALOGUE_RED_REFRESH, RefreshChatRed);
        EventDispatchCenter.Instance.Registry(SDEvents.DIALOGUE_NEXT, RefreshChat);

    }



    protected override void OnDestroy()
    {
        EventDispatchCenter.Instance.UnRegistry(SDEvents.DIALOGUE_RED_REFRESH, RefreshChatRed);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.DIALOGUE_NEXT, RefreshChat);
        base.OnDestroy();
    }

    private void RefreshChatRed(object obj)
    {
        if (data != null)
        {
            _Obj_RedPoint.SetActiveEx(data.isNew);
        }
    }

    private void RefreshChat(object obj)
    {
        if (data == null || data.npcId != (int)obj)
        {
            return;
        }
        string des = "";
        if (data.CurEventChatBase != null)
        {
            des = data.CurEventChatBase.Parameter;
            var DialogueType = data.CurEventChatBase.outWord.DialogueType;
            if (DialogueType == 9 || DialogueType == 4)
            {
                des = "[语音通话]";
            }
            else if (DialogueType == 3)
            {
                des = "[恭喜发财]";
            }
            else if (DialogueType == 10)
            {
                des = "[图片]";
            }
            else if (DialogueType == 5)
            {
                des = "[礼物]";
            }
        }
        _Txt_Count.text = des;
        _Obj_Omit.SetActiveEx(_Txt_Count.preferredWidth > _Txt_Count.rectTransform.sizeDelta.x);
    }

    public void SetUI(DialogueData dialogueData)
    {
        data = dialogueData;
        var npcData = GameManager.Instance.NpcControl.GetNpcData(data.npcId);
        var npcCfg = Config.GetConfig<Config_NpcBase>().GetConfigById(data.npcId);
        if (ReferenceEquals(npcCfg,null)) 
        {
            return;
        }
        ResourceManagerNew.instance.LoadTextureAsset(npcCfg.Img1, _Img_Head);
        _Txt_Name.text = npcCfg.Npcname;
        SetLevel(npcData.FeelLv);
        _Obj_RedPoint.SetActiveEx(data.isNew);

        string des = "";
        if (data.CurEventChatBase != null)
        {
            des = data.CurEventChatBase.Parameter;
            var DialogueType = data.CurEventChatBase.outWord.DialogueType;
            if (DialogueType == 9 || DialogueType == 4)
            {
                des = "[语音通话]";
            }
            else if (DialogueType == 3)
            {
                des = "[恭喜发财]";
            }else if(DialogueType == 10)
            {
                des = "[图片]";
            }else if(DialogueType == 5)
            {
                des = "[礼物]";
            }
        }
        _Txt_Count.text = des;
        _Obj_Omit.SetActiveEx(_Txt_Count.preferredWidth > _Txt_Count.rectTransform.sizeDelta.x);
    }

    private void SetLevel(int Lv)
    {
        if (Lv >= 10)
        {
            _Img_FeelLv2.gameObject.SetActiveEx(true);
            _Img_FeelLv1.gameObject.SetActiveEx(true);
            ResourceManagerNew.instance.LoadSpriteAsset($"UI_zdn_txt_{Lv / 10}", _Img_FeelLv2);
            ResourceManagerNew.instance.LoadSpriteAsset($"UI_zdn_txt_{Lv % 10}", _Img_FeelLv1);
        }
        else
        {
            _Img_FeelLv2.gameObject.SetActiveEx(false);
            _Img_FeelLv1.gameObject.SetActiveEx(true);
            ResourceManagerNew.instance.LoadSpriteAsset($"UI_zdn_txt_{Lv % 10}", _Img_FeelLv1);
        }
    }

    private void OnClickBG(GameObject go, PointerEventData click)
    {
        //打开对话界面
        UIManager.Instance.ShowUI("UIDialogueWindow", param: data);
    }
}
