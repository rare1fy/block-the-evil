using DG.Tweening;
using Framework;
using Pb;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class UIFightEnd : UIBase
{
    [BindNode(true)] private GameObject _Obj_WinUI;//胜利
    [BindNode(true)] private GameObject _Obj_FailUI;//失败
    [BindNode(true)] private GameObject _Obj_FailEnd;//失败结束界面
    [BindNode(true)] private GameObject _Obj_Puzzle;//拼图
    [BindNode(true)] private GameObject _Obj_Unlock;//解锁特殊npc
    [BindNode(true)] private GameObject _Obj_AD; //重开广告
    [BindNode(true)] GameObject _Obj_ShareRed;//首次分享红点
    [BindNode] CustomText _Txt_AddMoney;
    [BindNode] RectTransform _Obj_FlyGold;
    [BindNode] RectTransform _Obj_Tools;
    [BindNode] RectTransform _Obj_ResGroup;
    [BindNode] RectTransform _Obj_Gold;
    [BindNode] CustomText _Txt_GoldNum;

    #region 拼图

    [BindNode] private CustomText _Txt_Add;
    [BindNode] private CustomText _Txt_Schedule;
    [BindNode] private Image _Img_Schedule;
    [BindNode] private RawImage _Img_Npc;
    [BindNode(true)] private GameObject _Obj_suipian_1;
    [BindNode(true)] private GameObject _Obj_suipian_2;
    [BindNode(true)] private GameObject _Obj_suipian_3;
    [BindNode(true)] private GameObject _Obj_suipian_4;
    [BindNode(true)] private GameObject _Obj_suipian_5;
    private List<GameObject> PuzzleImgs;
    #endregion
    [BindNode] private CustomText _Txt_Name;
    [BindNode] private CustomText _Txt_Desc;
    [BindNode] private RawImage _Img_UnlockNcp;
    [BindNode] private CustomText _Txt_Level;
    [BindNode] private CustomText _Txt_RestartStamina;

    private Animator _animator;
    private bool sharing = false;
    public override void InitOnce()
    {
        base.InitOnce();
        _animator = GetComponent<Animator>();
        AddListener(ui_listener_type.onClick, "_Btn_Level_Win", OnClickWin);
        AddListener(ui_listener_type.onClick, "_Btn_Level_Next", OnClickNext);
        AddListener(ui_listener_type.onClick, "_Btn_Share", OnClickShare);
        AddListener(ui_listener_type.onClick, "_Btn_Level_Lose", OnClickLose);
        AddListener(ui_listener_type.onClick, "_Btn_Restart", OnClickRestart);
        InitTreasure();
        EventDispatchCenter.Instance.Registry(SDEvents.ACROSS_THE_DAY, OnAcrossDay);
        EventDispatchCenter.Instance.Registry(SDEvents.WX_ON_SHOW, ShareEndCall);

        PuzzleImgs = new List<GameObject>() { _Obj_suipian_1, _Obj_suipian_2, _Obj_suipian_3, _Obj_suipian_4, _Obj_suipian_5 };
    }

    protected override void OnDestroy()
    {
        EventDispatchCenter.Instance.UnRegistry(SDEvents.WX_ON_SHOW, ShareEndCall);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.ACROSS_THE_DAY, OnAcrossDay);
    }

    private void OnAcrossDay(object obj)
    {
        var playerModel = GameManager.Instance.PlayerControl.PlayerModel;
        _Txt_RestartStamina.text = $"{playerModel.Stamina}/{playerModel.MaxStamina}";
        _Obj_AD.SetActiveEx(!playerModel.HasEnoughStaminaForFight() && !playerModel.CanClaimDailyStamina());
    }

    public override void OnOpen(object param = null)
    {
        base.OnOpen(param);
        AudioManagerNew.Instance.FadeStopMusic();
        GameManager.Instance.CurFightControl.IsGameEnd = true;
        if (param is bool isWin)
        {
            RefreshRed();
            var fightCtrl = GameManager.Instance.CurFightControl;
            _Obj_WinUI.SetActiveEx(isWin);
            _Obj_FailUI.SetActiveEx(!isWin);
            var audioName = isWin ? "fight_partywin" : "fight_partylose";
            AudioManagerNew.Instance.PlayAudio(audioName);
            _animator.enabled = false;
            if (isWin)
            {
                fightCtrl.FightEnd(true);
                _Obj_Puzzle.SetActiveEx(true);
                _Obj_Unlock.SetActiveEx(false);
                OpenTreasure();
            }
            else
            {
                fightCtrl.FightEnd(false);
                var playerModel = GameManager.Instance.PlayerControl.PlayerModel;
                _Txt_RestartStamina.text = $"{playerModel.Stamina}/{playerModel.MaxStamina}";
                _Obj_AD.SetActiveEx(!playerModel.HasEnoughStaminaForFight() && !playerModel.CanClaimDailyStamina());
            }
        }
    }

    private void OpenPuzzle()
    {
        var level = GameManager.Instance.PlayerControl.PlayerModel.Level;
        var npcId = Config.GetConfig<Config_NpcUnlock>().GetNpcIdByLevel(level);
        var npcCfg = Config.GetConfig<Config_NpcBase>().GetConfigById(npcId);
        var curPiece = (level) % 5 == 0 ? 5 : (level) % 5;

        ResourceManagerNew.instance.LoadTextureAsset(npcCfg.Img2, _Img_Npc);
        ResourceManagerNew.instance.LoadTextureAsset(npcCfg.Img2, _Img_UnlockNcp);
        _Txt_Desc.text = npcCfg.Word;
        _Txt_Name.text = npcCfg.Npcname;
        _Txt_Level.text = $"第{level + 1}天"; 
        for (int i = 0; i < PuzzleImgs.Count; i++)
        {
            if (i < curPiece - 1)
            {
                PuzzleImgs[i].SetActiveEx(false);
            }
            else if(i > curPiece - 1)
            {
                PuzzleImgs[i].GetOrAddComponent<Animator>().enabled = false;
            }
            else
            {
                var img = PuzzleImgs[i];
                PuzzleImgs[i].SetActiveEx(true);
                PuzzleImgs[i].GetOrAddComponent<Animator>().enabled = true;
                DOVirtual.DelayedCall(1.5f, () =>
                {
                    img.SetActiveEx(false);
                    SetPuzzleSchedule(curPiece, npcCfg, true);
                });
            }
        }
        SetPuzzleSchedule(curPiece - 1, npcCfg);
        var npcIds = Config.GetConfig<Config_NpcBase>().GetRandomNormalNpcIds();
        _Txt_Add.text = $"派对新面孔：<size=48><color=#3EFF2D>+{npcIds.Count}</color></size>";
        if (curPiece == 5)
        {
            npcIds.Add(npcId);
        }
        GameManager.Instance.StageControl.AddPreAddition(npcIds);
        _Obj_Unlock.SetActiveEx(false);
    }

    private void SetPuzzleSchedule( int piece,NpcBase npcCfg, bool bPlayAnim = false)
    {
        if (bPlayAnim)
        {
            _Img_Schedule.DOFillAmount(piece / 5.0f, 0.5f).OnComplete(() =>
            {
                _Txt_Schedule.text = $"大咖解锁：<color=#f6bd4b>{piece * 100 / 5}%</color>";
                if(piece == 5)
                {
                    _animator.enabled = true;
                    _animator.Play("WinUI");
                    AudioManagerNew.Instance.PlayAudio("UI_End_UnlockSpNPC.ogg");
                    AudioManagerNew.Instance.PlayAudio(npcCfg.Voice1);
                    //_Obj_Puzzle.SetActiveEx(false);
                    _Obj_Unlock.SetActiveEx(true);
                }
            });
        }
        else
        {
            _Img_Schedule.fillAmount = piece/5.0f;
            _Txt_Schedule.text = $"大咖解锁：<color=#f6bd4b>{piece * 100 / 5}%</color>";
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


    private void OnClickRestart(GameObject _, PointerEventData __)
    {

        GameManager.Instance.PlayerControl.StartFight(() =>
        {
            GameManager.Instance.CurFightControl.ClearData();
        });
    }

    private void OnClickLose(GameObject _, PointerEventData __)
    {
        GameManager.Instance.CurFightControl.ClearData();
        UIManager.CutToScene(() =>
        {
            UIManager.Instance.CloseAll(true);
            UIManager.Instance.ShowUI("MainWindow");
        });
    }

    private void OnClickShare(GameObject _, PointerEventData __)
    {
        var dic = Config.GetConfig<Config_ShareWechatImage>().m_ShareWechatImageDic;
        var randomValue = dic.GetRandomValue();
        sharing = true;
        PlatformManager.Instance.ShareMessage(randomValue.ShareText, randomValue.ShareImgUrl, ShareEndCall);
    }

    private void OnClickNext(GameObject _, PointerEventData __)
    {

        GameManager.Instance.PlayerControl.StartFight(() =>
        {
            GameManager.Instance.CurFightControl.ClearData();
        });
    }

    private void OnClickWin(GameObject _, PointerEventData __)
    {
        GameManager.Instance.CurFightControl.ClearData();
        UIManager.CutToScene(() =>
        {
            UIManager.Instance.CloseAll(true);
            UIManager.Instance.ShowUI("MainWindow");
        });

    }
}


