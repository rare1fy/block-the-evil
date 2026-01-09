using DG.Tweening;
using Framework;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using RectTransform = UnityEngine.RectTransform;

public partial class UIFightMain
{
    [BindNode(true)] private GameObject _Obj_Finish;
    [BindNode] private RectTransform _Obj_EndStar;
    [BindNode] private Image _Obj_EndlessTips;
    [BindNode] private Image _Img_EProgress;
    [BindNode] private UIBubble _Obj_bubble;
    [BindNode(true)] private GameObject _Obj_Rank;
    [BindNode] private CustomText _Txt_Rank;


    private void InitMask()
    {
        AddListener(ui_listener_type.onClick, "_Img_EProgress", OnClickProgress);
    }

    private void OnClickProgress(GameObject @object, PointerEventData data)
    {
        var levelModel = GameManager.Instance.CurFightControl.LevelController.Model;
        var target = levelModel.Target;
        var num = target - _curScore;
        _Obj_bubble.SetDesc($"再收集{num}分，\n我就加入你的派对！");
    }

    private void CreateAllMask()
    {
        var masksData = _fightController.Model.MasksData;
        foreach (var maskData in masksData)
        {
            CreateMask(maskData);
        }
    }

    private void CreateMask(MaskData maskData)
    {
        ResourceManagerNew.instance.LoadAssetAsync<GameObject>("MaskItem", obj =>
        {
            var maskObj = Instantiate(obj, _Obj_Pool);
            var maskItem = maskObj.GetComponent<MaskItem>();
            maskItem.InitMaskItem(maskData);
        });
    }
    
    private void CheckChat()
    {
        ShowWait(0.8f, () =>
        {
            if (_fightController.LevelController.Model.IsEndLess)  //无尽模式只触发第一个npc的对话
            {
                var firstOrder = _fightController.LevelController.Model.FightOrders[0];
                _fightController.LevelController.CheckOpenChatEndless(firstOrder);
            }
            else
            {
                bool hasChat = false;
                foreach (var fightOrderData in _fightController.LevelController.Model.FightOrders)
                {
                    hasChat = hasChat || _fightController.LevelController.CheckOpenChat(fightOrderData);
                }
                OpenNewSystem(hasChat);
            }
        });
    }

    private void OpenNewSystem(bool hasChat)
    {
        var lvBase = _fightController.LevelController.Model.LevelBaseData;
        if (lvBase.Mechanism != 0 && !hasChat)
        { 
            UIManager.Instance.ShowUI("UIFightNewSystem", param: lvBase.Mechanism);
        }
    }
    
    private void InitEndlessTips()
    {
        var levelModel = GameManager.Instance.CurFightControl.LevelController.Model;
        SetEndlessRank();
        if (!levelModel.IsEndLess)
        {
            _Obj_EndlessTips.gameObject.SetActiveEx(false);
            return;
        }
        
        var endlessCfg =  Config.GetConfig<Config_LevelWujingModle>().GetConfigById(levelModel.LevelId);
        var npcCfg = Config.GetConfig<Config_NpcBase>().GetConfigById(endlessCfg.SpNpc);
        var owned = GameManager.Instance.StageControl.CheckNpcOwned(endlessCfg.SpNpc);
        _Obj_EndlessTips.gameObject.SetActiveEx(!owned);
        if(owned)
            return;
        
        _curScore = 0;
        _Img_EProgress.fillAmount = 0;
        _Obj_Finish.gameObject.SetActiveEx(false);
        ResourceManagerNew.instance.LoadSpriteAsset(npcCfg.Img1, _Obj_EndlessTips);
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_UPDATE_SCORE, ShowEndlessTips); //如果是无尽再注册
    }
    
    private int _curScore = 0;
    private void ShowEndlessTips(object param)
    {
        if (param is int newScore)
        {
            if (_curScore > newScore)
                return;
            var levelModel = GameManager.Instance.CurFightControl.LevelController.Model;
            var target = levelModel.Target;
            var before = _curScore;
            _curScore = newScore;
            _Obj_EndStar.gameObject.SetActiveEx(true);
            
            DOVirtual.Int(before, newScore, 0.8f, value =>
            {
                var progress = value / (float)target;
                progress = progress > 1f ? 1f : progress;
                _Img_EProgress.fillAmount = progress;
                var angle = progress * 360;
                _Obj_EndStar.rotation = Quaternion.Euler(0, 0, angle - 270);
            }). onComplete = () =>
            {
                if (newScore >= target)
                {
                    _Img_EProgress.gameObject.SetActiveEx(false);
                    _Obj_EndStar.gameObject.SetActiveEx(false);
                    _Obj_Finish.gameObject.SetActiveEx(true);
                }
            };
        }
    }
    
    private void SetEndlessRank(object o = null)
    {
        var levelModel = GameManager.Instance.CurFightControl.LevelController.Model;
        _Obj_Rank.SetActiveEx(levelModel.IsEndLess);
        if (levelModel.IsEndLess)
        {
            var desc = Config.GetConfig<Config_RankWujing>().GetRankeString(levelModel.LevelScore);
            _Txt_Rank.text = desc;
        }
    }

}