using DG.Tweening;
using Framework;
using Pb;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIBuildItem : UIItemBase
{
    [BindNode] private Image _Img_Icon;//图标
    [BindNode] private CustomText _Txt_Name;//名称
    [BindNode] private CustomText _Txt_Condition;//解锁条件
    [BindNode] private CustomText _Txt_Consume;//消耗
    [BindNode()] private UIButtonExtension _Btn_Build;//锻造
    [BindNode(true)] private GameObject _Obj_Red;

    int chapterId;
    BuildBase cfg;
    UIBuildWindow buildWindow;
    Animator animator;
    protected override void InitItem()
    {
        EventDispatchCenter.Instance.Registry(SDEvents.BAG_UPDATA_ITEM, RefreshRed);
        AddListener(ui_listener_type.onClick, "_Btn_Build", OnClickBG);
        animator = GetComponent<Animator>();
    }

    protected override void OnDestroy()
    {
        EventDispatchCenter.Instance.UnRegistry(SDEvents.BAG_UPDATA_ITEM, RefreshRed);
        base.OnDestroy();
    }

    private void RefreshRed(object obj = null)
    {
        if (cfg == null) 
        {
            return;
        }
        var bagCtrl = GameManager.Instance.GameBagControl;
        var Needglod = Util.AnalysisItem(cfg.Needglod);
        var id = Needglod.x;
        var count = Needglod.y;
        _Obj_Red.SetActiveEx(bagCtrl.CheckItemIsEnough(id, count));
    }

    public void SetUI(BuildBase cfg, int chapterId, UIBuildWindow buildWindow)
    {
        _Btn_Build.enabled = true;
        animator.Play("_Obj_BuildItem1_Idle");
        this.cfg = cfg;
        this.chapterId = chapterId;
        this.buildWindow = buildWindow;
        var bagCtrl = GameManager.Instance.GameBagControl;
        var lastBuildId = GameManager.Instance.ChapterControl.Model.lastBuildId;
        var Needglod = Util.AnalysisItem(cfg.Needglod);
        var id = Needglod.x;
        var count = Needglod.y;
        var itemBase = Config.GetConfig<Config_ItemBase>().GetConfigById(id);

        ResourceManagerNew.instance.LoadSpriteAsset(cfg.Image, _Img_Icon);
        _Txt_Name.text = cfg.Name;
        _Txt_Consume.text = $"X{Needglod.y}";

        var isLeavlUnlock = GameManager.Instance.ChapterControl.CheckBuildLeavl(cfg.Id);
        var chatCheck = GameManager.Instance.DialogueControl.CheckDialogue(cfg.Unlockchatid);
        var isLast = lastBuildId > 0 && cfg.Id == lastBuildId;
        if (!isLeavlUnlock)
        {
            _Txt_Condition.text = $"通关{cfg.Unlocklevel}解锁";
        }
        else
        {
            _Txt_Condition.text = cfg.Tips;
        }
        _Txt_Condition.gameObject.SetActiveEx(!isLeavlUnlock || !chatCheck);
        _Btn_Build.gameObject.SetActiveEx(isLeavlUnlock && chatCheck && !isLast);
        _Obj_Red.SetActiveEx(bagCtrl.CheckItemIsEnough(id, count));
        if (lastBuildId > 0 && cfg.Id == lastBuildId)
        {
            GameManager.Instance.ChapterControl.Model.lastBuildId = -1;
            animator.enabled = false;
            animator.enabled = true;
            animator.Play("_Obj_BuildItem1_Ani",0,0);
            DOVirtual.DelayedCall(1f, () =>
            {
                buildWindow.RefreshWindow();
            });
        }
    }

    private void OnClickBG(GameObject obj, PointerEventData eventData)
    {
        _Btn_Build.enabled = false;
        var bagCtrl = GameManager.Instance.GameBagControl;
        var Needglod = Util.AnalysisItem(cfg.Needglod);
        var id = Needglod.x;
        var num = Needglod.y;
        if (bagCtrl.CheckItemIsEnough(id, num))
        {
            buildWindow.OnClickBuild(cfg.Id, Needglod, (obj.transform as RectTransform).position);
        }
        else
        {
            UIManager.Instance.ShowPromptWindow("资源不足");
        }
    }
}
