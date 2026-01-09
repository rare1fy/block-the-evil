using System;
using System.Collections.Generic;
using Framework;
using Pb;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TreasureBoxItem : UIItemBase
{
    [BindNode(true)] GameObject _Obj_Close;
    [BindNode(true)] GameObject _Obj_Open;
    [BindNode] Image _Img_Reward;
    [BindNode] CustomText _Txt_Reward;

    private LevelTreasurechest cfg;
    public bool bReceived = false;
    public int id;
    public int count;
    private Action call;

    protected override void InitItem()
    {
        AddListener(ui_listener_type.onClick, "_Obj_Close", OnOpenClick);
    }

    public void SetItem(Action call)
    {
        if (cfg == null)
        {
            cfg = Config.GetConfig<Config_LevelTreasurechest>().GetRandomItem();
            SetRewaed();
        }
         this.call = null;
        this.call = call;
        _Obj_Close.SetActiveEx(!bReceived);
        _Obj_Open.SetActiveEx(bReceived);
        _Img_Reward.gameObject.SetActiveEx(bReceived);
    }

    private void SetRewaed()
    {
        string[] nums = cfg.Num.Split("#");
        if (cfg.Type == 1)
        {
            var itemBase = Config.GetConfig<Config_ItemBase>().GetConfigById(int.Parse(nums[0]));
            ResourceManagerNew.instance.LoadSpriteAsset(itemBase.Img, _Img_Reward);
            _Txt_Reward.text = $"X{nums[1]}";
            id = int.Parse(nums[0]);
            count = int.Parse(nums[1]);
        }
        else
        {
            var itemBase = Config.GetConfig<Config_ItemBase>().GetConfigById(1);
            ResourceManagerNew.instance.LoadSpriteAsset(itemBase.Img, _Img_Reward);
            id = 1;
            count = int.Parse(nums[0]) + int.Parse(nums[1]) * GameManager.Instance.PlayerControl.PlayerModel.Level;
            _Txt_Reward.text = $"X{count}";
        }
    }

    public void SetOpen()
    {
        bReceived = true;
        _Obj_Close.SetActiveEx(!bReceived);
        _Img_Reward.gameObject.SetActiveEx(bReceived);
        _Obj_Open.SetActiveEx(bReceived);
    }

    private void OnOpenClick(GameObject @object, PointerEventData data)
    {
        SetOpen();
        call?.Invoke();
    }
}