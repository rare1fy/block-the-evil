using DG.Tweening;
using Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class MainGM : UIItemBase
{
    protected override void InitItem()
    {
        AddListener(ui_listener_type.onClick, "_Btn_Leavl", OnLeavlClick);
        AddListener(ui_listener_type.onClick, "_Btn_Leavl2", OnLeavlClick1);
        AddListener(ui_listener_type.onClick, "_Btn_Leavl3", OnLeavlClick10);
        AddListener(ui_listener_type.onClick, "_Btn_Leavl4", OnLeavlClick11);

        AddListener(ui_listener_type.onClick, "_Btn_Order", OnOrderClick);
        AddListener(ui_listener_type.onClick, "_Btn_Feel", OnFeelClick);
        AddListener(ui_listener_type.onClick, "_Btn_Gold", OnGoldClick);
    }

    private void OnFeelClick(GameObject _, PointerEventData __)
    {
        List<int> ids = new();
        for (int i = 0; i < 10; i++)
        {
            ids.Add(1);
        }
        GameManager.Instance.StageControl.AddPreAddition(ids);
        GameManager.Instance.StageControl.JoinStage();
    }

    private void OnOrderClick(GameObject _, PointerEventData __)
    {
        var ctrl = GameManager.Instance.NpcControl;
        var datas = ctrl.GetAllNpc();
        foreach (var data in datas)
        {
            data.AddNpcOrder(2);
            data.SetPrefData();
        }
    }

    private void OnLeavlClick(GameObject _, PointerEventData __)
    {
        var lv = GameManager.Instance.PlayerControl.PlayerModel.Level + 1;
        if (Config.GetConfig<Config_LevelBase>().m_LevelBaseDic.Count < lv)
        {
            lv = Config.GetConfig<Config_LevelBase>().m_LevelBaseDic.Count;
        }
        GameManager.Instance.PlayerControl.PlayerModel.ChangeLeavl(lv);
        //GameManager.Instance.GameBagControl.UpdateItems(2, 1);
    }

    private void OnLeavlClick1(GameObject _, PointerEventData __)
    {
        var lv = GameManager.Instance.PlayerControl.PlayerModel.Level - 1;
        if (lv < 0)
        {
            lv = 0;
        }
        GameManager.Instance.PlayerControl.PlayerModel.ChangeLeavl(lv);
        //GameManager.Instance.GameBagControl.UpdateItems(2, 1);
    }

    private void OnLeavlClick10(GameObject _, PointerEventData __)
    {
        var lv = GameManager.Instance.PlayerControl.PlayerModel.Level + 10;
        if (Config.GetConfig<Config_LevelBase>().m_LevelBaseDic.Count < lv)
        {
            lv = Config.GetConfig<Config_LevelBase>().m_LevelBaseDic.Count;
        }
        GameManager.Instance.PlayerControl.PlayerModel.ChangeLeavl(lv);
        //GameManager.Instance.GameBagControl.UpdateItems(2, 1);
    }

    private void OnLeavlClick11(GameObject _, PointerEventData __)
    {
        var lv = GameManager.Instance.PlayerControl.PlayerModel.Level - 10;
        if (lv < 0)
        {
            lv = 0;
        }
        GameManager.Instance.PlayerControl.PlayerModel.ChangeLeavl(lv);
    }

    private void OnGoldClick(GameObject _, PointerEventData __)
    {
        GameManager.Instance.GameBagControl.UpdateItems(GameBagModel.GOLD, 100);
        var itemCfg = Config.GetConfig<Config_ItemBase>().GetConfigById(1);
        UIManager.Instance.ShowPromptWindow($"获得{itemCfg.ItemName} * {100}");
    }


}
