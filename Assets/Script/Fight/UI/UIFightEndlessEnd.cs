using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Framework;
using Pb;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIFightEndlessEnd : UIBase
{
    [BindNode(true)] private GameObject _Obj_New;
    [BindNode(true)] private GameObject _Obj_LoseQP;
    [BindNode(true)] private GameObject _Obj_Progress;
    [BindNode(true)] private GameObject _Obj_Mask;

    [BindNode] private CustomText _Txt_Score;
    [BindNode] private CustomText _Txt_ProgressScore;
    [BindNode] private Image _Img_lose;
    [BindNode] private Image _Img_win;
    [BindNode] private Image _Img_Npc;
    [BindNode] private Image _Img_huodedi;

    [BindNode(true)] private GameObject _Obj_Rank;
    [BindNode] private CustomText _Txt_Rank;
    [BindNode(true)] private GameObject _Obj_Rank2;
    [BindNode] private CustomText _Txt_Rank2;
    [BindNode(true)] private GameObject _Obj_Preview;
    [BindNode] private Image _Img_NextNpc;
    [BindNode] private Image _Img_NextNpcOutLine;
    [BindNode] private CustomText _Txt_Time;

    [BindNode(true)] GameObject _Obj_ShareRed;//首次分享红点
    [BindNode] CustomText _Txt_AddMoney;
    [BindNode] RectTransform _Obj_FlyGold;
    [BindNode] RectTransform _Obj_Tools;
    [BindNode] RectTransform _Obj_ResGroup;
    [BindNode] RectTransform _Obj_Gold;
    [BindNode] CustomText _Txt_GoldNum;
    private bool sharing = false;

    public override void InitOnce()
    {
        base.InitOnce();
        AddListener(ui_listener_type.onClick, "_Btn_Back", OnClickBack);
        AddListener(ui_listener_type.onClick, "_Btn_Restart", OnClickRestart);
        AddListener(ui_listener_type.onClick, "_Btn_Share", OnClickShare);
        EventDispatchCenter.Instance.Registry(SDEvents.WX_ON_SHOW, ShareEndCall);
    }

    protected override void OnDestroy()
    {
        EventDispatchCenter.Instance.UnRegistry(SDEvents.WX_ON_SHOW, ShareEndCall);
    }

    public override void OnOpen(object param = null)
    {
        base.OnOpen(param);
        RefreshRed();
        GameManager.Instance.CurFightControl.FightEnd(true,true);
        GameManager.Instance.CurFightControl.IsGameEnd = true;
        InitWindow();
    }

    private void InitWindow()
    {
        AudioManagerNew.Instance.PlayAudio("fight_partywin");
        var fightCtrl = GameManager.Instance.CurFightControl;
        var endlessCtrl  = GameManager.Instance.EndlessControl;
        var endlessId = fightCtrl.LevelController.Model.LevelId;
        var endlessCfg =  Config.GetConfig<Config_LevelWujingModle>().GetConfigById(endlessId);
        var levelScore = fightCtrl.LevelController.Model.LevelScore;
        var owned = GameManager.Instance.StageControl.CheckNpcOwned(endlessCfg.SpNpc);
        var npcCfg = Config.GetConfig<Config_NpcBase>().GetConfigById(endlessCfg.SpNpc);
        ResourceManagerNew.instance.LoadSpriteAsset(npcCfg.Img4, _Img_Npc);

        var bFinish = owned || levelScore >= endlessCfg.Npc; //已经完成 或者 分数达到
        var huoDeImgStr = bFinish? "UI_zjm_tz_huodedi2" : "UI_zjm_tz_huodedi1";
        ResourceManagerNew.instance.LoadSpriteAsset(huoDeImgStr, _Img_huodedi);
        _Img_huodedi.gameObject.SetActiveEx(false);
        _Obj_New.SetActiveEx(false);
        _Obj_Mask.SetActiveEx(true);
        _Obj_LoseQP.SetActiveEx(!bFinish);
        _Img_lose.gameObject.SetActive(!bFinish);
        _Img_win.gameObject.SetActive(bFinish);
        endlessCtrl.EndlessFinish(endlessCfg);
        SetEndlessRank(owned);
        
        if (owned)
        {
            _Obj_Progress.SetActiveEx(false);
            _Obj_Preview.SetActiveEx(true);
            SetNextNpc();
            DOVirtual.Int(0, levelScore, 1.3f, (value) =>
            {
                _Txt_Score.text = value.ToString();
            }).SetEase(Ease.OutQuad).onComplete = () =>
            {
                _Obj_Mask.SetActiveEx(false);
            };
        }
        else
        {
            _Obj_Progress.SetActiveEx(true);
            _Obj_Preview.SetActiveEx(false);
            var targetImg = bFinish ? _Img_win : _Img_lose;
            DOVirtual.Int(0, levelScore, 1.3f, (value) =>
            {
                _Txt_Score.text = value.ToString();
                _Txt_ProgressScore.text = $"{value}/{endlessCfg.Npc}";
            
                targetImg.fillAmount = value / (float)endlessCfg.Npc;
            
                if (value >= endlessCtrl.HistoryHighScore)
                {
                    _Obj_New.SetActiveEx(true);
                }
            }).SetEase(Ease.OutQuad).onComplete = () =>
            {
                if (levelScore >= endlessCfg.Npc)
                {
                    ShowNewStar(endlessCfg);
                }    
                _Obj_Mask.SetActiveEx(false);
                _Img_huodedi.gameObject.SetActiveEx(true);
            };
        }
    }

    #region 每日首次分享相关

    private void ShareEndCall(object o = null)
    {
        var ctrl = GameManager.Instance.PlayerControl;

        if (sharing)
        {
            sharing = !sharing;
            if (ctrl.CheckShare())
            {
                ctrl.PlayerModel.SetShare();
                var num = Config.GetConfig<Config_GdConstant>().GetConfigById(37).Num;
                RefreshRed();
                var change = GetChangeMoney(num);
                FlyMoney(change.value1, change.value2);
            }
        }
    }

    private Tuple<int, int> GetChangeMoney(int change)
    {
        var ctrl = GameManager.Instance.GameBagControl;
        var last = ctrl.GetItemNumberById(GameBagModel.GOLD);
        ctrl.UpdateItems(GameBagModel.GOLD, change);
        var cur = ctrl.GetItemNumberById(GameBagModel.GOLD);
        return new Tuple<int, int>(last, cur);
    }

    private void RefreshRed()
    {
        var ctrl = GameManager.Instance.PlayerControl;
        _Obj_ShareRed.SetActiveEx(ctrl.CheckShare());

    }

    private void FlyMoney(int last, int cur)
    {
        var bagCtrl = GameManager.Instance.GameBagControl;
        var fly = Instantiate(_Obj_FlyGold, _Obj_Tools);
        fly.localPosition = Vector3.zero;
        fly.gameObject.SetActiveEx(false);
        _Obj_Gold.anchoredPosition.Set(_Obj_Gold.anchoredPosition.x, 0);
        var endPos = _Obj_Tools.transform.InverseTransformPoint(_Obj_ResGroup.transform.position);
        _Obj_Gold.anchoredPosition.Set(_Obj_Gold.anchoredPosition.x, 300);

        _Txt_GoldNum.text = Util.FormatNumber(bagCtrl.GetItemNumberById(GameBagModel.GOLD));
        Sequence scaleSequence = DOTween.Sequence();
        scaleSequence.Append(_Obj_Gold.DOLocalMoveY(0, 0.5f).OnComplete(() =>
        {
            fly.gameObject.SetActiveEx(true);
            AudioManagerNew.Instance.PlayAudio("UI_Bo.ogg");
        }));
        scaleSequence.Append(fly.DOScale(1.2f, 0.3f).SetEase(Ease.InOutQuad));
        scaleSequence.Join(fly.DOMove(fly.position + new Vector3(0f, -1f, 0f), 0.3f));
        scaleSequence.AppendInterval(0.5f);
        scaleSequence.Append(fly.DOScale(0.2f, 0.5f).SetEase(Ease.InOutQuad));
        scaleSequence.Join(fly.DOLocalJump(endPos, 1.3f, 1, 0.5f).SetEase(Ease.OutQuad));
        scaleSequence.Play();
        scaleSequence.OnComplete(() =>
        {
            scaleSequence.Kill();
            Destroy(fly.gameObject);
            var add = Instantiate(_Txt_AddMoney, _Obj_ResGroup);
            add.text = $"+{cur - last}";
            add.gameObject.SetActiveEx(true);
            add.GetComponent<Animator>().Play("Add");
            AudioManagerNew.Instance.PlayAudio("UI_party_ad_reward.ogg");
            DOVirtual.Int(last, cur, 1f, v =>
            {
                _Txt_GoldNum.text = Util.FormatNumber(v);
            }).OnComplete(() =>
            {
                DOVirtual.DelayedCall(0.3f, () =>
                {
                    _Obj_Gold.DOLocalMoveY(300, 0.5f);
                });
                Destroy(add);
            });

        });
    }

    #endregion

    private void OnClickBack(GameObject o, PointerEventData e)
    {
        GameManager.Instance.CurFightControl.ClearData();
        UIManager.CutToScene(() =>
        {
            UIManager.Instance.CloseAll(true);
            UIManager.Instance.ShowUI("MainWindow");
        });
    }
    
    private void OnClickRestart(GameObject o, PointerEventData e)
    {
        GameManager.Instance.CurFightControl.ClearData();
        UIManager.Instance.CloseAll(true);
        GameManager.Instance.FightStart(0, 2);
    }

    private void ShowNewStar(LevelWujingModle curCfg)
    {
        GameManager.Instance.StageControl.AddPreAddition(new List<int>(){curCfg.SpNpc});
        GameManager.Instance.EndlessControl.ChangeLastFinishEndlessTime();
        UIManager.Instance.ShowUI("UIFightNewStar", null, curCfg.SpNpc);
    }
    private void SetEndlessRank(bool owned)
    {
        var levelModel = GameManager.Instance.CurFightControl.LevelController.Model;
        var endlessCtrl = GameManager.Instance.EndlessControl;
        _Obj_Rank.SetActiveEx(!owned || endlessCtrl.IsEndNpc());
        _Obj_Rank2.SetActiveEx(owned && !endlessCtrl.IsEndNpc());
        if (levelModel.IsEndLess)
        {
            var desc = Config.GetConfig<Config_RankWujing>().GetRankeString(levelModel.LevelScore);
            _Txt_Rank.text = desc;
            _Txt_Rank2.text = desc;
        }
    }

    private void SetNextNpc()
    {
        var endlessCtrl = GameManager.Instance.EndlessControl;
        if (endlessCtrl.IsEndNpc())
        {
            _Obj_Preview.SetActive(false);
            return;
        }

        _Txt_Time.text =  $"神秘大咖{endlessCtrl.GetNextDayTime()}后造访!";
        var endlessCfg = endlessCtrl.GetNextCfg();
        var npcCfg = Config.GetConfig<Config_NpcBase>().GetConfigById(endlessCfg.SpNpc);
        ResourceManagerNew.instance.LoadSpriteAsset(npcCfg.Img4, _Img_NextNpc);
        ResourceManagerNew.instance.LoadSpriteAsset(npcCfg.Img4, _Img_NextNpcOutLine);
    }
    
    
    private void OnClickShare(GameObject _, PointerEventData __)
    {
        sharing = true;
        PlatformManager.Instance.ShareMessage("我这分数天王老子来了也超不过我", "", ShareEndCall);
    }
}
