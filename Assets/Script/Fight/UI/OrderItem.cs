using DG.Tweening;
using Framework;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OrderItem : UIItemBase
{
    [BindNode(true)] private GameObject _Obj_Npc;
    [BindNode] private Image _Img_Cost;
    [BindNode] private RawImage _Img_Npc;
    [BindNode] private Image _Img_Item;
    [BindNode] private CustomText _Txt_Cost;
    [BindNode] private CustomText _Txt_Count;
    [BindNode(true)] private GameObject _Btn_Cost;
    [BindNode(true)] private GameObject _Btn_AD;
    [BindNode] private UIBubble _Obj_Bubble;
    [BindNode] private ArtNumberTool _Obj_AddNum;

    private Animator _animator;
    /// <summary>
    /// 催促间隔时间
    /// </summary>
    private float _interval;
    /// <summary>
    /// 计时
    /// </summary>
    private float _nextTime;
    protected override void InitItem()
    {
        _animator = GetComponent<Animator>();
        var lvModel = GameManager.Instance.CurFightControl.LevelController.Model;
        AddListener(ui_listener_type.onClick, "_Btn_Cost", OnClickCost);
        AddListener(ui_listener_type.onClick, "_Btn_AD", OnClickAD);
        _interval = lvModel.IsEndLess || lvModel.LevelId > 10 ? Config.GetConfig<Config_GdConstant>().GetConfigById(31).Num : Config.GetConfig<Config_GdConstant>().GetConfigById(7).Num;
        _nextTime = _interval;
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_MUSIC_SELECT, ChangePlayAnim);
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_MUSIC_SELECT, ChangePlayAnim);
    }

    /// <summary>
    /// 订单数据
    /// </summary>
    public FightOrderData OrderData;

    public void RefreshItem(int index)
    {
        _Obj_Bubble.gameObject.SetActive(false);
        _Obj_AddNum.gameObject.SetActive(false);
        var isOver = GameManager.Instance.CurFightControl.LevelController.IsLevelTargetFinish();
        if (isOver)
        {
            return;
        }
        var orderData = GameManager.Instance.CurFightControl.LevelController.Model.FightOrders.Find(x => x.Index == index);
        if (orderData != null && OrderData != orderData)
        {
            _animator.Play("OrderItem_Show");
            AudioManagerNew.Instance.PlayAudio("fight_newcustomer");
            OrderData = orderData;
            _nextTime = _interval;
        }
        if (orderData == null)
        {
            _nextTime = -1;
        }
        
        _Btn_Cost.SetActiveEx(false);
        _Btn_AD.SetActiveEx(false);
        _Obj_Npc.SetActiveEx(orderData != null);
        _Img_Npc.gameObject.SetActiveEx(orderData != null);
        
        if (orderData == null)
        {
            _Txt_Cost.text = string.Empty;
        }
        else
        {
            var npc = Config.GetConfig<Config_NpcBase>().GetConfigById(orderData.NpcId).Img2;
            ResourceManagerNew.instance.LoadTextureAsset(npc, _Img_Npc);
            var item = Config.GetConfig<Config_OrderBlock>().GetConfigById(orderData.ItemId);
            ResourceManagerNew.instance.LoadSpriteAsset(item.Img, _Img_Item);
            _Txt_Count.text = orderData.NeedBlockCount.ToString();
        }
    }
    
    /// <summary>
    /// 飞行动画
    /// </summary>
    public void FlyAnim(FightOrderData orderData)
    {
        _nextTime = _interval;
        if (orderData == OrderData)
        {
            AudioManagerNew.Instance.PlayAudio("notice_order_progressadd.ogg", 1f, true);
            _Txt_Count.text = orderData.NeedBlockCount.ToString();
        }
    }

    public void OrderNpcAnim()
    {
        var sequence = DOTween.Sequence();
        // 添加到序列：放大
        sequence.Append(_Img_Item.transform.DOScale(1.3f, 0.1f).SetEase(Ease.InOutQuad));
        // 添加到序列：缩小回原始尺寸
        sequence.Append(_Img_Item.transform.DOScale(1, 0.1f).SetEase(Ease.InOutQuad));
        sequence.onComplete = () =>
        {
            sequence.Kill();
        };
    }

    public void PlayOrderFinishAnim()
    {
        ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_C_TBC_Good01", fx =>
        {
            PlatformManager.Instance.ShortVibration(2);
            var fxObj = Instantiate(fx, transform);
            fxObj.transform.localPosition = Vector3.zero;
            fxObj.transform.localScale = Vector3.one * 1.5f;
        });
        
        _Obj_Bubble.SetOrderComplete();
        AddNumPopup();
        DOVirtual.DelayedCall(0.6f, () =>
        {
            _animator.Play("OrderItem_Hide");

            DOVirtual.DelayedCall(0.26f, () =>
            {
                RefreshItem(OrderData.Index);
            });
        });
    }
    
    public RectTransform GetOrderNeedItemTransform(BlockData data)
    {
        return _Img_Item.GetComponent<RectTransform>(); 
    }

    private void OnClickCost(GameObject o, PointerEventData e)
    {
        if(GameManager.Instance.CurFightControl.LevelController.Model.OrderList.Count <= 0)
            return;
        var cost = GameManager.Instance.CurFightControl.LevelController.Model.LevelBaseData.PayorderNeed;

        if (GameManager.Instance.CurFightControl.Model.Money < cost)
        {
            UIManager.Instance.ShowSecondConfirm("钞票不足，是否观看广告解锁", CallAds, isAd: true);
            return;
        }
        
        GameManager.Instance.CurFightControl.Model.AddMoney(-cost);
        GameManager.Instance.CurFightControl.LevelController.AddOrderCount(1);
    }
    
    private void CallAds()
    {
        if (GameManager.Instance.CurFightControl.LevelController.Model.OrderList.Count <= 0)
            return;
        
        PlatformManager.Instance.ShowRewardedVideoAd(6, (isOk) =>
        {
            GameManager.Instance.LogManager.Log_AD(6, isOk);
            if (isOk)
                GameManager.Instance.CurFightControl.LevelController.AddOrderCount(1);
            else
                UIManager.Instance.ShowPromptWindow("观看时间不足，无法获取奖励");
        });
    }

    private void OnClickAD(GameObject o, PointerEventData e)
    {
        if(GameManager.Instance.CurFightControl.LevelController.Model.OrderList.Count <= 0)
            return;
        PlatformManager.Instance.ShowRewardedVideoAd(6,(isOk) =>
        {
            GameManager.Instance.LogManager.Log_AD(6, isOk);
            if (isOk)
                GameManager.Instance.CurFightControl.LevelController.AddOrderCount(2);
            else
                UIManager.Instance.ShowPromptWindow("观看时间不足，无法获取奖励");
        });
    }

    public void PlayLoopAnim()
    {
        ChangePlayAnim(null);
    }
    private void ChangePlayAnim(object obj)
    {
        int id = GameManager.Instance.MusicControl.PickMusicId;
        var cfg = Config.GetConfig<Config_MusicPlayer>().GetConfigById(id == 0 ? 1 : id);
        _animator.Play(cfg.BeatsAni);
        //Debug.LogError($"动画名称 {cfg.BeatsAni}");
    }

    private void AddNumPopup()
    {
        var lvCtrl = GameManager.Instance.CurFightControl.LevelController;
        if(lvCtrl.Model.LevelBaseData.Modle == 2 || lvCtrl.Model.IsEndLess)
        {
            var score = Config.GetConfig<Config_GdConstant>().GetConfigById(28).Num;
            _Obj_AddNum.gameObject.SetActiveEx(true);
            _Obj_AddNum.DisplayNumber(score);
        }
    }


    private void FixedUpdate()
    {
        if (OrderData == null || OrderData.NeedBlockCount <= 0)
            return;
        
        if (UIManager.Instance.GetTopUI() as UIFightMain)
        {
            if (GameManager.Instance.CurFightControl.ChatController != null
            && !GameManager.Instance.CurFightControl.ChatController.Model.bInChat)
            {
                _nextTime -= Time.fixedDeltaTime;
                if (_nextTime <= 0)
                {
                    _nextTime = _interval;
                    var npcCfg = Config.GetConfig<Config_NpcBase>().GetConfigById(OrderData.NpcId);
                    if(npcCfg.Vioce2 != null && npcCfg.Vioce2 != "null")
                        AudioManagerNew.Instance.PlayAudio(npcCfg.Vioce2);
                    //播放动画
                    _animator.Play("OrderItem_Notice");
                    _Obj_Bubble.SetOrderNotice();
                }
            }
        }
    }
}
