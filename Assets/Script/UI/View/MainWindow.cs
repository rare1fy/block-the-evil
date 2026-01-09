using DG.Tweening;
using Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class MainWindow : UIBase
{
    [BindNode(true)]
    private GameObject _Obj_Content; 
    [BindNode(true)]
    private GameObject _Obj_UIScaneDrag;
    [BindNode]
    private CustomText _Txt_Name;
    [BindNode]
    private Image _Img_Schedule;
    [BindNode]
    private CustomText _Txt_Schedule;
    [BindNode(true)] GameObject _Obj_FX_MusicalNotes;
    //[BindNode]
    //private UIChapterItem _Obj_Item;
    //[BindNode(true)]
    //private GameObject _Obj_ChatRed;
    [BindNode]
    private Image _Img_Gold;
    [BindNode]
    private CustomText _Txt_GoldNum;
    [BindNode]
    private Image _Img_Star;
    [BindNode]
    private CustomText _Txt_StarNum;
    //[BindNode(true)]
    //private GameObject _Obj_BuildRed;
    //[BindNode(true)]
    //private GameObject _Obj_ContactsRed;

    //[BindNode]
    //private Animator _Btn_Phone_Loop;

    [BindNode] UIStage _Obj_UIStage;
    [BindNode] CustomText _Txt_Stamina;
    [BindNode(true)] private GameObject _Btn_Music;
    [BindNode] private Image _Img_Music;
    [BindNode(true)] private GameObject _Obj_RedPoint;
    [BindNode(true)] private GameObject _Img_BrokenMusic;
    [BindNode] private Image _Img_Boss;
    
    [BindNode] private CustomText _Txt_Version;
    [BindNode(nodeName: "_Btn_Endless")] private Animator endlessAnim;
    [BindNode(true)] private GameObject _Obj_Rank;
    [BindNode] private CustomText _Txt_Rank;
    [BindNode(true)] private GameObject _Obj_AD; //开始广告图标
    [BindNode(true)] private GameObject _Obj_StaminaTime; //开始广告图标
    [BindNode] private CustomText _Txt_StaminaTime;


    private Animator musicAnimator;

    public override void InitOnce()
    {
        AddListener(ui_listener_type.onClick, "_Btn_Start", OnClickStart);
        //AddListener(ui_listener_type.onClick, "_Btn_Phone_Loop", OnClickPhone);
        //AddListener(ui_listener_type.onClick, "_Btn_Contacts", OnClickContacts);
        //AddListener(ui_listener_type.onClick, "_Btn_Architecture", OnClickArchitecture);

        AddListener(ui_listener_type.onClick, "_Btn_Signup", OnEnptyClick);
        AddListener(ui_listener_type.onClick, "_Btn_FirstRecharge", OnEnptyClick);
        AddListener(ui_listener_type.onClick, "_Btn_Event", OnEnptyClick);
        AddListener(ui_listener_type.onClick, "_Btn_Endless", OnClickEndless);
        AddListener(ui_listener_type.onClick, "_Btn_Area", OnEnptyClick);
        AddListener(ui_listener_type.onClick, "_Btn_Book", OnEnptyClick);
        AddListener(ui_listener_type.onClick, "_Btn_Setting", OnSettingClick);
        AddListener(ui_listener_type.onClick, "_Btn_Music", OnMusicClick);
        AddListener(ui_listener_type.onClick, "_Btn_Desktop", OnClickDdesktop);

        EventDispatchCenter.Instance.Registry(SDEvents.BUILD_ARCHITECTURE, OnBuildShow);
        EventDispatchCenter.Instance.Registry(SDEvents.CHANGE_CHAPTER, OnChangeChapter);
        EventDispatchCenter.Instance.Registry(SDEvents.DIALOGUE_RED_REFRESH, RefreshChatRed);
        EventDispatchCenter.Instance.Registry(SDEvents.DIALOGUE_NEXT, RefreshBuildRed);
        EventDispatchCenter.Instance.Registry(SDEvents.CHANGE_LEAVL, LevelUp);
        EventDispatchCenter.Instance.Registry(SDEvents.BAG_UPDATA_ITEM, RefreshItem);
        EventDispatchCenter.Instance.Registry(SDEvents.CHANGE_NPC_STATE, RefreshNpcRed);
        EventDispatchCenter.Instance.Registry(SDEvents.BUILD_REWORD, OnChangeChapter);
        EventDispatchCenter.Instance.Registry(SDEvents.ACROSS_THE_DAY, OnAcrossDay);
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_MUSIC_SELECT, RefreshMusicImg);
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_MUSIC_PROGRESS_REFRESH, RefreshMusicProgress);

        musicAnimator = _Btn_Music.GetComponent<Animator>();
        InitOnceSDK();
    }

    public override void OnOpen(object param = null)
    {
        InitEndless();
        OnChangeChapter();
        RefreshChatRed();
        RefreshNpcRed();
        RefreshItem();
        SetEndlessRank();
        OnOpenSDK();
        StartStaminaTime();
        var playerModel = GameManager.Instance.PlayerControl.PlayerModel;
        _Txt_Stamina.text = $"今日剩余：{playerModel.Stamina}/{playerModel.MaxStamina}";
        _Obj_AD.SetActiveEx(playerModel.Stamina <= 0);
        _Obj_StaminaTime.SetActiveEx(playerModel.Stamina <= 0);
        GameManager.Instance.StageControl.JoinStage();
        RefreshMusicImg();
        RefreshMusicProgress();
        GameManager.Instance.MusicControl.PlayPickMainBGM(true);
        _Img_BrokenMusic.SetActiveEx(playerModel.Level < 1);
        _Btn_Music.SetActiveEx(playerModel.Level >= 1);
        _Txt_Version.text = YooAssetManager.PackageVersion;
    }

    public override void OnClose()
    {
        base.OnClose();
        TimerManager.instance.RemoveTimer(timer);
        timer = 0;
    }

    protected override void OnDestroy()
    {
        EventDispatchCenter.Instance.UnRegistry(SDEvents.BUILD_ARCHITECTURE, OnBuildShow);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.CHANGE_CHAPTER, OnChangeChapter);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.DIALOGUE_RED_REFRESH, RefreshChatRed);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.DIALOGUE_NEXT, RefreshBuildRed);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.CHANGE_LEAVL, LevelUp);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.BAG_UPDATA_ITEM, RefreshItem);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.CHANGE_NPC_STATE, RefreshNpcRed);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.BUILD_REWORD, OnChangeChapter);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.ACROSS_THE_DAY, OnAcrossDay);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_MUSIC_SELECT, RefreshMusicImg);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_MUSIC_PROGRESS_REFRESH, RefreshMusicProgress);
        OnDestroySDK();
        if (timer != 0)
            TimerManager.instance.RemoveTimer(timer);
        timer = 0;
    }

    private void LevelUp(object obj)
    {
        GameManager.Instance.ChapterControl.RefreshChapter();
        RefreshBuildRed();
        OnChangeChapter();
    }

    private void InitEndless()
    {
        var stageCtrl = GameManager.Instance.StageControl;
        var NpcCount = stageCtrl.joined.Count + stageCtrl.PreAddition.Count;
        var UnlockCount = Config.GetConfig<Config_GdConstant>().GetConfigById(32).Num;
        var endlessCtrl = GameManager.Instance.EndlessControl;
        var endlessCfg = Config.GetConfig<Config_LevelWujingModle>().GetConfigById(endlessCtrl.GetEndlessLevelId());
        var npcCfg = Config.GetConfig<Config_NpcBase>().GetConfigById(endlessCfg.SpNpc);
        if (!string.IsNullOrEmpty(npcCfg.Img4))
        {
            ResourceManagerNew.instance.LoadSpriteAsset(npcCfg.Img4, _Img_Boss);
        }
        endlessAnim.GetComponent<UIGray>().DoGray(NpcCount < UnlockCount);
        endlessAnim.enabled = NpcCount >= UnlockCount;
        var checkNpc = GameManager.Instance.StageControl.CheckNpcOwned(endlessCfg.SpNpc);
        var animName = checkNpc ? "MainWindow_Endless_Idel" : "MainWindow_Endless_AC";
        endlessAnim.Play(animName);
    }

    /// <summary>
    /// 通讯录（可解锁）红点
    /// </summary>
    /// <param name="obj"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void RefreshNpcRed(object obj = null)
    {
        //var bUnlockable = GameManager.Instance.NpcControl.CheckHasNpcUnlockable();
        //_Obj_ContactsRed.SetActiveEx(false);
    }

    /// <summary>
    /// 刷新建筑的红点
    /// </summary>
    private void RefreshBuildRed(object param = null)
    {
        var canBuild = GameManager.Instance.ChapterControl.CheckChapterCanBuilding();
        //if (_Obj_BuildRed!= null)
        //{
        //    _Obj_BuildRed.SetActiveEx(canBuild);
        //}
    }

    private void RefreshChatRed(object obj = null)
    {
        bool checkRed = GameManager.Instance.DialogueControl.CheckDialogueRed();
        //_Obj_ChatRed.SetActiveEx(false);
        //_Btn_Phone_Loop.enabled = checkRed;
    }


    private void OnAcrossDay(object obj)
    {
        var playerModel = GameManager.Instance.PlayerControl.PlayerModel;
        _Txt_Stamina.text = $"今日剩余：{playerModel.Stamina}/{playerModel.MaxStamina}";
        _Obj_AD.SetActiveEx(playerModel.Stamina <= 0);
        _Obj_StaminaTime.SetActiveEx(playerModel.Stamina <= 0);
        InitEndless();
    }

    private void OnChangeChapter(object obj = null)
    {
        //var ChapterControl = GameManager.Instance.ChapterControl;
        //var PlayerControl = GameManager.Instance.PlayerControl;
        //var ChapterId = ChapterControl.Model.curChapter;
        //var cfg = Config.GetConfig<Config_ChapterBase>().GetConfigById(ChapterId);

        //_Txt_Name.text = $"章节{ChapterId}";
        //var fill = PlayerControl.PlayerModel.Level - cfg.Lvmin > 0 ? PlayerControl.PlayerModel.Level - cfg.Lvmin : 0;
        //fill = PlayerControl.PlayerModel.Level - cfg.Lvmin > cfg.Lvmax ? cfg.Lvmax : PlayerControl.PlayerModel.Level - cfg.Lvmin;
        //_Img_Schedule.fillAmount = ChapterControl.GetChapterBuildCount(ChapterId) / (float)ChapterControl.GetChapterMaxBuild(ChapterId);
        //_Txt_Schedule.text = $"{ChapterControl.GetChapterBuildCount(ChapterId)}/{ChapterControl.GetChapterMaxBuild(ChapterId)}";
        //_Obj_Item.SetUI(ChapterId);
    }

    /// <summary>
    /// 建造建筑隐藏主界面
    /// </summary>
    /// <param name="obj"></param>
    private void OnBuildShow(object obj)
    {
        //_Obj_Content.SetActive(false);
        //_Obj_UIScaneDrag.SetActiveEx(false);
        ////播放动画病创建图片
        //DOVirtual.DelayedCall(2f, () =>
        //{
        //    var ctrl = GameManager.Instance.ChapterControl;
        //    var ChapterId = ctrl.Model.curChapter;
        //    var buildCout = ctrl.GetChapterBuildCount(ChapterId);
        //    var maxBuild = ctrl.GetChapterMaxBuild(ChapterId);
        //    ctrl.Model.isBuilding = false;
        //    _Obj_Content.SetActive(true);
        //    _Obj_UIScaneDrag.SetActiveEx(true);
        //    _Img_Schedule.DOFillAmount(buildCout / (float)maxBuild, 0.3f)
        //    .OnComplete(() =>
        //    {
        //        _Txt_Schedule.text = $"{buildCout}/{maxBuild}";
        //        //_Obj_Item.SetUI(ChapterId);
        //        ctrl.RefreshChapter();
        //    });
        //});
    }


    private void RefreshItem(object obj = null)
    {
        var bagCtrl = GameManager.Instance.GameBagControl;
        if (obj == null)
        {
            bagCtrl.SetImgIcon(GameBagModel.GOLD, _Img_Gold);
            _Txt_GoldNum.text = Util.FormatNumber(bagCtrl.GetItemNumberById(GameBagModel.GOLD));
            // bagCtrl.SetImgIcon(GameBagModel.Star, _Img_Star);
            // _Txt_StarNum.text = bagCtrl.GetItemNumberById(GameBagModel.Star).ToString();
        }
        else
        {
            var itemBase = obj as ItemConfig;
            if (itemBase.Id == GameBagModel.GOLD)
            {
                //bagCtrl.SetImgIcon(GameBagModel.GOLD, _Img_Gold);
                //_Txt_GoldNum.text = Util.FormatNumber(bagCtrl.GetItemNumberById(GameBagModel.GOLD));
            }
            else
            {
                bagCtrl.SetImgIcon(GameBagModel.Star, _Img_Star);
                _Txt_StarNum.text = bagCtrl.GetItemNumberById(GameBagModel.Star).ToString();
            }
        }
        RefreshBuildRed();
    }

    private void RefreshMusicImg(object param = null)
    {
        var id = GameManager.Instance.MusicControl.MainMusicId;
        var list = GameManager.Instance.MusicControl.UnlockMusicIds;
        var lv = GameManager.Instance.PlayerControl.PlayerModel.Level;
        if (list.Count > 0 && id == 0 && lv > 0)
        {
            GameManager.Instance.MusicControl.MainMusicId = list[0];
            id = list[0];
        }
        if (id > 0)
        {
            _Btn_Music.SetActiveEx(true);
            _Img_BrokenMusic.SetActiveEx(false);
            var cfg = Config.GetConfig<Config_MusicPlayer>().GetConfigById(id);
            ResourceManagerNew.instance.LoadSpriteAsset(cfg.Img, _Img_Music);
        }
    }

    private void RefreshMusicProgress(object param = null)
    {
        var hasUnLockable = GameManager.Instance.MusicControl.HasUnlockable();
        if (hasUnLockable)
        {
            musicAnimator.Play("_Btn_Music_Loop_Loop");
        }
        else
        {
            musicAnimator.Play("_Btn_Music_Loop");
        }
        _Obj_RedPoint.SetActiveEx(hasUnLockable);
    }

    private void SetEndlessRank(object o = null)
    {
        var ctrl = GameManager.Instance.EndlessControl;
        var stageCtrl = GameManager.Instance.StageControl;
        var NpcCount = stageCtrl.joined.Count + stageCtrl.PreAddition.Count;
        var UnlockCount = Config.GetConfig<Config_GdConstant>().GetConfigById(32).Num;
        _Obj_Rank.SetActiveEx(NpcCount >= UnlockCount);
        var desc = Config.GetConfig<Config_RankWujing>().GetRankeString(ctrl.HistoryHighScore,24);
        _Txt_Rank.text = desc;
    }

    private uint timer = 0;

    private void StartStaminaTime()
    {
        if (timer != 0)
            TimerManager.instance.RemoveTimer(timer);
        timer = TimerManager.instance.AddTimer(1f, () =>
        {
            var playerModel = GameManager.Instance.PlayerControl.PlayerModel;
            if (playerModel.Stamina > 0)
            {
                return;
            }
            _Obj_StaminaTime.SetActiveEx(playerModel.Stamina <= 0);
            var timeSpan = Util.GetTimeUntilNextCrossDay(null);
            if (timeSpan.TotalSeconds <= 0)
            {
                _Txt_StaminaTime.text = $"00时00分后恢复体力";
            }
            if (timeSpan.Hours > 0)
            {
                _Txt_StaminaTime.text = $"{timeSpan.Hours:D2}时{timeSpan.Minutes:D2}分后恢复体力";
            }
            else 
            {
                _Txt_StaminaTime.text = $"{timeSpan.Minutes:D2}分{timeSpan.Seconds:D2}秒后恢复体力";
            }
        }, true, false, -1);
    }

    public void OnClickStart(GameObject go, PointerEventData eventData)
    {
        GameManager.Instance.PlayerControl.StartFight();
    }

    public void OnClickPhone(GameObject go, PointerEventData eventData)
    {
        //UIManager.Instance.ShowUI("UIChatWindow");
    }

    public void OnClickContacts(GameObject go, PointerEventData eventData)
    {
        //UIManager.Instance.ShowUI("UIContactsWindow");
    }

    public void OnClickArchitecture(GameObject go, PointerEventData eventData)
    {
        //传入章节id
        var ctrl = GameManager.Instance.ChapterControl;
        if (ctrl.CheckHasNotBuilt())
        {
            UIManager.Instance.ShowUI("UIBuildWindow");
        }
        else
        {
            UIManager.Instance.ShowPromptWindow("已完成本章节所有建筑");
        }
    }

    private void OnGoldClick(GameObject _, PointerEventData __)
    {
        GameManager.Instance.GameBagControl.UpdateItems(GameBagModel.GOLD, 100);
        var itemCfg = Config.GetConfig<Config_ItemBase>().GetConfigById(1);
        UIManager.Instance.ShowPromptWindow($"获得{itemCfg.ItemName} * {100}");
    }

    private void OnMusicClick(GameObject _, PointerEventData __)
    {
        var closeCB = new Action(() =>
        {
            _Obj_FX_MusicalNotes.SetActiveEx(true);
        });
        UIManager.Instance.ShowUI("UIMusicWindow", obj =>
        {
            _Obj_FX_MusicalNotes.SetActiveEx(false);
        }, new Tuple<Action, bool>(closeCB, true));
    }

    private void OnSettingClick(GameObject _, PointerEventData __)
    {
        UIManager.Instance.ShowUI("UISetPopup",param : false);
    }
    
    private void OnClickEndless(GameObject o, PointerEventData e)
    {
        var count = GameManager.Instance.StageControl.GetAllNpcCount(); 
        var max = Config.GetConfig<Config_GdConstant>().GetConfigById(32).Num;
        if (count < max)
        {
            UIManager.Instance.ShowPromptWindow($"再邀请{max - count}个派对宾客解锁！");
        }
        else
        {
            UIManager.Instance.CloseAll(true);
            AudioManagerNew.Instance.FadeStopMusic();
            UIManager.CutToScene(() =>
            {
                GameManager.Instance.FightStart(0, 2);
            });
        }
    }

    private void OnEnptyClick(GameObject @object, PointerEventData data)
    {
        UIManager.Instance.ShowPromptWindow("功能暂未开放");
    }

    private void OnClickDdesktop(GameObject @object, PointerEventData data)
    {
        UIManager.Instance.ShowUI("UIDdesktop");
    }
}
