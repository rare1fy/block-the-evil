using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class UIOrderPanel : UIItemBase
{
    [BindNode] private OrderItem _Obj_OrderItem1;
    [BindNode] private OrderItem _Obj_OrderItem2;
    [BindNode] private OrderItem _Obj_OrderItem3;
    
    [BindNode(true)] private GameObject _Obj_Score;
    [BindNode] private ArtNumberTool _Obj_CurScore;
    [BindNode] private ArtNumberTool _Obj_TargetScore;
    [BindNode] private Image _Img_ScoreProgress;
    [BindNode] private CustomText _Txt_ScoreTitle;
    [BindNode(true)] private GameObject _Obj_New;


    private List<OrderItem> _orderItemList = new List<OrderItem>();
    
    private CanvasGroup _item2CGroup;
    protected override void InitItem()
    {
        _orderItemList.Add(_Obj_OrderItem1);
        _item2CGroup = _Obj_OrderItem2.GetComponent<CanvasGroup>();
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_REFRESH_ORDER_ITEM, RefreshPanel);
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_UPDATE_SCORE, UpdateScore);
        //EventDispatchCenter.Instance.Registry(SDEvents.C2C_ORDER_FINISH_WITH_BLOCK, PlayFlyEffect);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _orderItemList.Clear();
        _orderItemList = null;
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_REFRESH_ORDER_ITEM, RefreshPanel);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_UPDATE_SCORE, UpdateScore);
        //EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_ORDER_FINISH_WITH_BLOCK, PlayFlyEffect);
    }

    private UIFightMain _uiFightMain;
    public void InitPanel(UIFightMain ui)
    {
        _uiFightMain = ui;
        var levelModel = GameManager.Instance.CurFightControl.LevelController.Model;
        var maxMonsterSlots = levelModel.GetMaxActiveMonsterSlots();
        _Obj_OrderItem1.gameObject.SetActiveEx(maxMonsterSlots >= 1);
        _Obj_OrderItem2.gameObject.SetActiveEx(maxMonsterSlots >= 2);
        _Obj_OrderItem3.gameObject.SetActiveEx(maxMonsterSlots >= 3);

        if (maxMonsterSlots >= 2 && !_orderItemList.Contains(_Obj_OrderItem2))
            _orderItemList.Add(_Obj_OrderItem2);

        if (maxMonsterSlots >= 3 && !_orderItemList.Contains(_Obj_OrderItem3))
            _orderItemList.Add(_Obj_OrderItem3);

        RefreshPanel();
        
        if (levelModel.LevelBaseData.Modle == 2 || levelModel.IsEndLess)
        {
            _Obj_Score.SetActiveEx(true);
            _item2CGroup.alpha = 0f;
            _Obj_CurScore.DisplayNumber(0);
            if (levelModel.IsEndLess)
            {
                _Txt_ScoreTitle.text = "历史最高";
                _Obj_TargetScore.DisplayNumber(GameManager.Instance.EndlessControl.HistoryHighScore);
                isTriggeredNewNum = GameManager.Instance.EndlessControl.HistoryHighScore == 0;
            }
            else
            {
                _Txt_ScoreTitle.text = "目标分数";
                _Obj_TargetScore.DisplayNumber(GameManager.Instance.CurFightControl.LevelController.Model.Target);
            }
        }
        else
        {
            _Obj_Score.SetActiveEx(false);
            _item2CGroup.alpha = 1f;
        }
    }

    public void RefreshPanel(object param = null)
    {
        for (var i = 0; i < _orderItemList.Count; i++)
        {
            var item = _orderItemList[i];
            item.RefreshItem(i);
        }
    }


    private bool isTriggeredNewNum = false;

    private int _lastScore = 0;
    private void UpdateScore(object param)
    {
        if (param is int score)
        {
            var levelModel = GameManager.Instance.CurFightControl.LevelController.Model;
            if (levelModel.LevelBaseData.Modle != 2 && !levelModel.IsEndLess) 
                return;
            
            var target = 0;
            if (levelModel.IsEndLess)
            {
                target = GameManager.Instance.EndlessControl.HistoryHighScore;
                ShowNewNum(score, target);
            }
            else
            {
                target = GameManager.Instance.CurFightControl.LevelController.Model.Target;
            }
            
            var before = _lastScore;
            _lastScore = score;
            //if (levelModel.IsEndLess || levelModel.LevelBaseData.Modle == 2)
            //{
            //    AudioManagerNew.Instance.PlayAudio("fight_add_Point.ogg");
            //}
            DOVirtual.Int(before, score, 0.7f, value =>
            {
                if (value > target)
                {
                    _Obj_TargetScore.DisplayNumber(value);
                    _Img_ScoreProgress.fillAmount = 1;
                }
                else
                {
                    var progress = value / (float)target;
                    _Img_ScoreProgress.fillAmount = progress;
                }
                
                _Obj_CurScore.DisplayNumber(value);
            });
        }
    }

    private void ShowNewNum(int score, int target)
    {
        if (score > target && !isTriggeredNewNum)
        {
            DOVirtual.DelayedCall(0.71f, () =>
            {
                isTriggeredNewNum = !isTriggeredNewNum;
                _Obj_New.SetActiveEx(true);
                AudioManagerNew.Instance.PlayAudio("fight_high_score.ogg");
                DOVirtual.DelayedCall(1.2f, () =>
                {
                    _Obj_New.SetActiveEx(false);
                });
            });

        }
    }
    
    /// <summary>
    /// 订单飞行特效
    /// </summary>
    /// <param name="param"></param>
    public void PlayFlyEffect(object param)
    {
        List<FightOrderData> orderList = new();
        if (param is Dictionary<FightOrderData, List<BlockData>> data)
        {
            var targetCount = -1;
            foreach (var orderData in data.Keys)
            {
                orderList.Add(new FightOrderData(orderData));
                var orderItem = _orderItemList[orderData.Index];
                var num = data[orderData].Count - 1;
                foreach (var item in data[orderData])
                {
                    targetCount++;
                    var end = orderItem.GetOrderNeedItemTransform(item);
                    var delayTime = targetCount * 0.05f;
                    DOVirtual.DelayedCall(delayTime, () =>
                    {
                        _uiFightMain.PlayBlockClearFlyAnim(item, end, () =>
                        {
                            num--;
                            orderItem.FlyAnim(orderData);

                            if (num <= 0)
                            {
                                orderItem.OrderNpcAnim();
                                PlayFinishEffect(orderList);
                            }
                        });
                    });
                }
            }
            
            var fightController = GameManager.Instance.CurFightControl;
            var finishedOrders = fightController.LevelController.CheckFightOrderAndGetFinished();
            if (orderList.Count <= 0 && finishedOrders.Count > 0)
            {
                PlayFinishEffect(finishedOrders);
            }
        }
    }
    
    public void PlayFinishEffect(List<FightOrderData> dataList)
    {
        foreach (var data in dataList)
        {
            var orderItem = _orderItemList[data.Index];
            if (orderItem.OrderData != null && data.Index == orderItem.OrderData.Index)
            {
                var orderData = data;
                if (orderData.CheckNeedListFinish()) //订单完成
                {
                    var npcCfg = Config.GetConfig<Config_NpcBase>().GetConfigById(data.NpcId);
                    if(npcCfg.Voice1 != null && npcCfg.Voice1 != "null")
                    {
                        var audioStr = npcCfg.Voice1.Split(";");
                        var audioClipName = audioStr[UnityEngine.Random.Range(0, audioStr.Length)];
                        AudioManagerNew.Instance.PlayAudio(audioClipName);
                    }
                    orderItem.PlayOrderFinishAnim();
                }
            }
        }
        _uiFightMain.RefreshOrderNum();
    }
}
