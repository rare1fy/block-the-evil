using Framework;
using Pb;
using System;
using System.Resources;
using System.Xml;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIBuildRewordItem : UIItemBase
{
    [BindNode] private Image _Img_Icon;//图标
    [BindNode(true)] private GameObject _Img_Icon_Loop;//可领取
    //[BindNode(true)] private GameObject _Obj_Lock;//未解锁
    //[BindNode(true)] private GameObject _Obj_Awarded;//已领取
    [BindNode(true)] private GameObject _Btn_Item;//可领取
    [BindNode] private CustomText _Txt_Num;//数量
    [BindNode(nodeName: "_Img_Icon")] private UIGray _Gray;


    int chapterId;

    protected override void InitItem()
    {
        AddListener(ui_listener_type.onClick, "_Btn_Item", OnClickBG);

    }

    public void SetUI(int chapterId)
    {
        this.chapterId = chapterId;
        var cfg = Config.GetConfig<Config_ChapterBase>().GetConfigById(chapterId);
        var Reward = cfg.Reward.Split("#");
        var id = int.Parse(Reward[0]);
        var itemBase = Config.GetConfig<Config_ItemBase>().GetConfigById(id);
        //ResourceManagerNew.instance.LoadSpriteAsset(itemBase.Img, _Img_Icon);
        _Txt_Num.text = $"{itemBase.ItemName}X{Reward[1]}";

        var ctrl = GameManager.Instance.ChapterControl;

        var unlock = ctrl.GetChapterMaxBuild(chapterId) == ctrl.GetChapterBuildCount(chapterId);
        var isGet = ctrl.Model.ChapterRewords.Exists(p => p == chapterId);

        _Btn_Item.gameObject.SetActiveEx(unlock && !isGet);
        _Img_Icon_Loop.SetActiveEx(unlock && !isGet);
        _Img_Icon.gameObject.SetActiveEx(isGet || !unlock);
        _Gray.DoGray(isGet || !unlock);
    }

    private void OnClickBG(GameObject _, PointerEventData __)
    {
        GameManager.Instance.ChapterControl.SetChapterRewords(chapterId);
        SetUI(chapterId);
    }
}
