using System;
using System.Collections.Generic;
using Framework;
using Pb;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum TreasureRewardType
{
    Gold = 1,
    Stamina = 2
}

public class TreasureBoxItem : UIItemBase
{
    private struct TreasureRewardOption
    {
        public TreasureRewardType Type;
        public int BaseCount;
        public int LevelCount;
        public int Weight;
    }

    private static readonly TreasureRewardOption[] RewardOptions =
    {
        new TreasureRewardOption { Type = TreasureRewardType.Gold, BaseCount = 80, LevelCount = 30, Weight = 35 },
        new TreasureRewardOption { Type = TreasureRewardType.Gold, BaseCount = 140, LevelCount = 45, Weight = 25 },
        new TreasureRewardOption { Type = TreasureRewardType.Gold, BaseCount = 240, LevelCount = 60, Weight = 15 },
        new TreasureRewardOption { Type = TreasureRewardType.Stamina, BaseCount = 5, LevelCount = 0, Weight = 20 },
        new TreasureRewardOption { Type = TreasureRewardType.Stamina, BaseCount = 10, LevelCount = 0, Weight = 5 }
    };

    private const string StaminaIcon = "UI_ty_icon_Motion";

    [BindNode(true)] GameObject _Obj_Close;
    [BindNode(true)] GameObject _Obj_Open;
    [BindNode] Image _Img_Reward;
    [BindNode] CustomText _Txt_Reward;

    public bool bReceived = false;
    public TreasureRewardType rewardType;
    public int count;
    private Action call;

    protected override void InitItem()
    {
        AddListener(ui_listener_type.onClick, "_Obj_Close", OnOpenClick);
    }

    public void SetItem(Action call)
    {
        bReceived = false;
        CreateReward();
        this.call = null;
        this.call = call;
        _Obj_Close.SetActiveEx(!bReceived);
        _Obj_Open.SetActiveEx(bReceived);
        _Img_Reward.gameObject.SetActiveEx(bReceived);
    }

    private void CreateReward()
    {
        var reward = RollReward();
        rewardType = reward.Type;
        count = reward.BaseCount + reward.LevelCount * GameManager.Instance.PlayerControl.PlayerModel.Level;
        if (rewardType == TreasureRewardType.Gold)
            GameManager.Instance.GameBagControl.SetImgIcon(GameBagModel.GOLD, _Img_Reward);
        else
            ResourceManagerNew.instance.LoadSpriteAsset(StaminaIcon, _Img_Reward);

        _Txt_Reward.text = rewardType == TreasureRewardType.Stamina ? $"体力X{count}" : $"X{count}";
    }

    private TreasureRewardOption RollReward()
    {
        var totalWeight = 0;
        foreach (var option in RewardOptions)
            totalWeight += option.Weight;

        var randomValue = UnityEngine.Random.Range(0, totalWeight);
        var currentWeight = 0;
        foreach (var option in RewardOptions)
        {
            currentWeight += option.Weight;
            if (randomValue < currentWeight)
                return option;
        }

        return RewardOptions[0];
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
