using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ComboReword : UIItemBase
{
    [BindNode(true)] private GameObject _Obj_Reward;
    [BindNode] private ArtNumberTool _Obj_RewardNum;
    [BindNode] private ArtNumberTool _Obj_Score;

    public float wait = 1f;
    protected override void InitItem()
    {
    }

    public void ShowComboReword(int id)
    {
        gameObject.SetActive(true);
        var cfg = Config.GetConfig<Config_FighteffectBase>().GetConfigById(id);
        var lvCtrl = GameManager.Instance.CurFightControl.LevelController;
        _Obj_RewardNum.DisplayNumber(cfg.Reward);
        _Obj_Score.DisplayNumber(cfg.Score);
        bool isScore = lvCtrl.Model.IsEndLess || lvCtrl.Model.LevelBaseData.Modle == 2;
        _Obj_Reward.SetActiveEx(!isScore);
        _Obj_Score.gameObject.SetActiveEx(isScore);
        Remover();
    }

    private void Remover()
    {
        DOVirtual.DelayedCall(wait, () =>
        {
            gameObject.SetActive(false);
        });
    }
}
