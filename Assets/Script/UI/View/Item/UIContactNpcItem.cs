using Framework;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIContactNpcItem : UIItemBase
{
    [BindNode] private RawImage _Img_Head;
    [BindNode] private CustomText _Txt_Name;

    [BindNode(true)] private GameObject _Obj_Unknown;
    [BindNode] private CustomText _Txt_Condition;

    [BindNode(true)] private GameObject _Btn_Unlock;

    [BindNode(true)] private GameObject _Obj_Unlock;
    [BindNode] private Image _Img_GoodFeeling;
    [BindNode] private CustomText _Txt_GoodFeeling;
    [BindNode] private Image _Img_Lv1;
    [BindNode] private Image _Img_Lv2;


    [BindNode(true)] private GameObject _Obj_Quick;
    [BindNode] private CustomText _Txt_Condition1;

    NpcData data;

    protected override void InitItem()
    {
        AddListener(ui_listener_type.onClick, "_Btn_Unlock", OnClickUnLock);
        AddListener(ui_listener_type.onClick, "_Btn_Quick", OnClickUnLock);
        EventDispatchCenter.Instance.Registry(SDEvents.CHANGE_NPC_STATE, OnChangeItem);
        EventDispatchCenter.Instance.Registry(SDEvents.CHAGE_NPC_FEEL, OnChangeFeel);
        EventDispatchCenter.Instance.Registry(SDEvents.CHANGE_ORDER, OnChangeItem);

    }



    protected override void OnDestroy()
    {
        EventDispatchCenter.Instance.UnRegistry(SDEvents.CHANGE_NPC_STATE, OnChangeItem);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.CHAGE_NPC_FEEL, OnChangeFeel);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.CHANGE_ORDER, OnChangeItem);

        base.OnDestroy();
    }
    private void OnChangeItem(object obj)
    {
        var npcid = (int)obj;
        if (data == null || npcid != data.Id) return;
        if (GameManager.Instance.NpcControl.Model.NpcDic.TryGetValue(data.Id, out var newData))
        {
            SetUI(newData);
        }
    }

    private void OnChangeFeel(object obj)
    {
        var tmp = (Tuple<int, int>)obj;
        if (data == null || tmp.value1 != data.Id) return;
        if (GameManager.Instance.NpcControl.Model.NpcDic.TryGetValue(data.Id, out var newData))
        {
            SetUI(newData);
        }
    }

    public void SetUI(NpcData data)
    {
        this.data = data;
        var cfg = Config.GetConfig<Config_NpcBase>().GetConfigById(data.Id);
        var maxExp = GameManager.Instance.NpcControl.GetMaxExp(data.FeelLv, out bool isMax);
        var playerModel = GameManager.Instance.PlayerControl.PlayerModel;
        //头像
        ResourceManagerNew.instance.LoadTextureAsset(cfg.Img3, _Img_Head);
        _Txt_Name.text = cfg.Npcname;

        //_Obj_Unknown.SetActiveEx(playerModel.Level < cfg.Unlocklevelneed);
        //_Btn_Unlock.gameObject.SetActive(data.NpcState == NpcState.Openable);
        //_Obj_Unlock.SetActiveEx(data.NpcState == NpcState.UnLock);
        //_Obj_Quick.SetActiveEx(playerModel.Level >= cfg.Unlocklevelneed && data.Order < cfg.Unlockorderneed && data.NpcState != NpcState.UnLock);
        //if (playerModel.Level < cfg.Unlocklevelneed)
        //{
        //    _Txt_Condition.text = $"通过关卡{cfg.Unlocklevelneed}";
        //}
        if (data.NpcState == NpcState.UnLock)
        {
            var Lv = data.FeelLv;
            if (Lv >= 10)
            {
                _Img_Lv2.gameObject.SetActiveEx(true);
                _Img_Lv1.gameObject.SetActiveEx(true);
                ResourceManagerNew.instance.LoadSpriteAsset($"UI_zdn_txt_{Lv / 10}", _Img_Lv2);
                ResourceManagerNew.instance.LoadSpriteAsset($"UI_zdn_txt_{Lv % 10}", _Img_Lv1);
            }
            else
            {
                _Img_Lv2.gameObject.SetActiveEx(false);
                _Img_Lv1.gameObject.SetActiveEx(true);
                ResourceManagerNew.instance.LoadSpriteAsset($"UI_zdn_txt_{Lv % 10}", _Img_Lv1);
            }
            _Txt_GoodFeeling.text = $"{data.FeelExp}/{maxExp}";
            _Img_GoodFeeling.fillAmount = data.FeelExp / (float)maxExp;
        }
        //if (playerModel.Level >= cfg.Unlocklevelneed && data.NpcState != NpcState.UnLock)
        //{
        //    _Txt_Condition1.text = $"订单量{data.Order}/{cfg.Unlockorderneed}";
        //}
    }

    private void OnClickUnLock(GameObject _, PointerEventData __)
    {
        var NpcControl = GameManager.Instance.NpcControl;
        NpcControl.UnLockingNpc(data.Id);
    }
}
