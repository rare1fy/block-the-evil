using Framework;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameBagControl : BaseControl
{
    private string GameBagSaveKey = "ORDER_PLAYER_SAVE_KEY";
    private GameBagModel model;
    public GameBagModel Model
    {
        get
        {
            if (ReferenceEquals(model, null))
            {
                model = new GameBagModel();
            }
            return model;
        }
        set
        {
            model = value;
        }
    }

    private Config_ItemBase itemBase;

    protected override void OnInitControl()
    {
        itemBase = Config.GetConfig<Config_ItemBase>();
        //初始化
        var item = PlayerPrefs.GetString(GameBagSaveKey);
        var itemArray = item.Split(';');
        for (int i = 0; i < itemArray.Length; i++)
        {
            var signItem = itemArray[i].Split('#');
            if (signItem.Length >= 2)
            {
                var id = int.Parse(signItem[0]);
                var number = int.Parse(signItem[1]);
                Model.ItemInfos.Add(id, number);
            }
        }
    }
    

    /// <summary>
    /// 设置物品显示
    /// </summary>
    /// <param name="itmeId">物品ID</param>
    /// <param name="icon">物品图标</param>
    public void SetImgIcon(int itmeId, Image icon)
    {
        var item = itemBase.GetConfigById(itmeId);
        if (item != null)
        {
            ResourceManagerNew.instance.LoadSpriteAsset(item.Img, icon);
        }
    }

    /// <summary>
    /// 设置物品背景
    /// </summary>
    /// <param name="itmeId">物品ID</param>
    /// <param name="icon">物品图标</param>
    public void SetImgBg(int itmeId, Image icon)
    {
        var item = itemBase.GetConfigById(itmeId);
        if (item != null)
        {
            //ResourceManagerNew.instance.LoadSpriteAsset(item.ImgBackground, icon);
        }
    }

    /// <summary>
    /// 物品名字
    /// </summary>
    /// <param name="itemId">物品ID</param>
    /// <param name="icon">物品图标</param>
    public string GetItemName(int itemId)
    {
        var itemName = string.Empty;
        var item = itemBase.GetConfigById(itemId);
        if (item != null)
        {
            itemName = item.ItemName;
        }
        return itemName;
    }

    /// <summary>
    /// 获取道具数量
    /// </summary>
    /// <param name="packItems"></param>
    public int GetItemNumberById(int ItmeID)
    {
        int itemNumber = 0;
        foreach (var item in Model.ItemInfos) 
        {
            if (item.Key == ItmeID)
            {
                itemNumber += (int)item.Value;
            }
        }
        return itemNumber;
    }

    /// <summary>
    /// 获取道具是否足够
    /// </summary>
    /// <param name="ItmeID"></param>
    /// <param name="number"></param>
    /// <returns></returns>
    public bool CheckItemIsEnough(int ItmeID,long number)
    {
        return GetItemNumberById(ItmeID) >= number;
    }

    /// <summary>
    /// 更新物品数量
    /// </summary>
    /// <param name="itemConfig"></param>
    public void UpdateItems(List<ItemConfig> itemConfig)
    {
        if (!ReferenceEquals(itemConfig,null) && itemConfig.Count > 0)
        {
            foreach (var item in itemConfig)
            {
                UpdateItems(item.Id, item.Number);
            }
        }
    }

    /// <summary>
    /// 更新物品数量
    /// </summary>
    /// <param name="itemID">物品ID</param>
    /// <param name="number">物品数量</param>
    public void UpdateItems(int itemID, long number)
    {
        if (number == 0)
            return;
        switch (itemID)
        {
            case GameBagModel.GOLD: 
            case GameBagModel.Star: 
                if (!Model.ItemInfos.TryAdd(itemID, number))
                {
                    Model.ItemInfos[itemID] += number;
                }
                UpdateItem();
                EventDispatchCenter.Instance.Dispatch(SDEvents.BAG_UPDATA_ITEM, new ItemConfig() { Id = itemID, Number = number });
                break;
            default:
                if (!Model.ItemInfos.TryAdd(itemID, number))
                {
                    Model.ItemInfos[itemID] += number;
                }
                UpdateItem();
                EventDispatchCenter.Instance.Dispatch(SDEvents.BAG_UPDATA_ITEM, new ItemConfig() { Id = itemID, Number = number });
                break;
        }
    }

    public void SetItemCount(int id, int number)
    {
        Model.ItemInfos[id] = number;
        UpdateItem();
    }

    /// <summary>
    /// 更新单个物体
    /// </summary>
    private void UpdateItem()
    {
        var itemString = string.Empty;
        int index = 0;
        var itemCount = Model.ItemInfos.Count;
        foreach (var item in Model.ItemInfos) 
        {
            itemString += item.Key + "#" + item.Value;
            index ++;
            if (index < itemCount)
            {
                itemString += ";";
            }
        }
        if (string.IsNullOrEmpty(itemString))
        {
            PlayerPrefs.DeleteKey(GameBagSaveKey);
        }
        else
        {
            PlayerPrefs.SetString(GameBagSaveKey, itemString);
        }
    }

    protected override void OnCloseControl()
    {
        Model = null;
    }
}
