using DG.Tweening;
using Framework;
using Google.Protobuf.WellKnownTypes;
using Pb;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class UIFightEnd : UIBase
{
    [BindNode(true)] GameObject _Obj_Treasure;
    [BindNode] TreasureBoxItem _Obj_BoxItem1;
    [BindNode] TreasureBoxItem _Obj_BoxItem2;
    [BindNode] TreasureBoxItem _Obj_BoxItem3;
    [BindNode(true)] GameObject _Btn_Get;
    [BindNode(true)] GameObject _Btn_ADS;
    [BindNode(true)] GameObject _Obj_Tps;
    [BindNode(true)] GameObject _Obj_Btns;
    [BindNode(true)] GameObject _Obj_AddNum;

    List<TreasureBoxItem> treasureBoxItems;
    private void InitTreasure()
    {
        AddListener(ui_listener_type.onClick, "_Btn_Get", OnClickGet);
        AddListener(ui_listener_type.onClick, "_Btn_ADS", OnClickBoxAds);
        treasureBoxItems = new() { _Obj_BoxItem1, _Obj_BoxItem2, _Obj_BoxItem3 };
    }

    private void OpenTreasure()
    {
        _Obj_AddNum.SetActiveEx(false);
        _Obj_Btns.SetActiveEx(false);
        _Btn_ADS.SetActiveEx(false);
        _Btn_Get.SetActiveEx(false);
        _Obj_Tps.SetActiveEx(true);
        foreach (var item in treasureBoxItems)
        {
            item.SetItem(ClickItemCall);
        }
    }

    private void ClickItemCall()
    {
        _Obj_Tps.SetActiveEx(false);
        _Btn_ADS.SetActiveEx(true);
        _Btn_Get.SetActiveEx(true);
    }

    private void OnClickBoxAds(GameObject @object, PointerEventData data)
    {
        PlatformManager.Instance.ShowRewardedVideoAd(3, (isOk) =>
        {
            if (isOk)
            {
                foreach (var item in treasureBoxItems)
                {
                    item.SetOpen();
                }
            }
            else
            {
                UIManager.Instance.ShowPromptWindow("观看时间不足，无法获取奖励");
            }
        });
    }

    private void OnClickGet(GameObject @object, PointerEventData data)
    {
        _Obj_AddNum.SetActiveEx(true);
        _Obj_Btns.SetActiveEx(true);
        _Obj_Treasure.SetActiveEx(false);
        GetReword();
        OpenPuzzle();
        //关闭抽奖界面
        //打开人数界面
        //播放动画
    }

    private void GetReword()
    {
        Dictionary<int ,ItemConfig> items = new();
        foreach (var treasureBox in treasureBoxItems)
        {
            if (treasureBox.bReceived)
            {
                if (items.TryGetValue(treasureBox.id, out ItemConfig item))
                {
                    item.Number += treasureBox.count;
                }
                else
                {
                    item = new ItemConfig() { Id = treasureBox.id, Number = treasureBox.count };
                    items.Add(treasureBox.id, item);
                }
            }
        }

        foreach (var item in items.Values)
        {
            if (item.Id == 1)
            {
                var change = GetChangeMoney((int)item.Number);
                FlyMoney(change.value1, change.value2);
            }
            GameManager.Instance.GameBagControl.UpdateItems(item.Id, item.Number);
        }
    }
}


