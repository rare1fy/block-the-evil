using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIChapterItem : UIItemBase
{
    [BindNode] private Image _Img_Icon;//图标
    [BindNode(nodeName: "_Img_Icon")] private UIGray _UIGray;
    [BindNode(true,nodeName: "_Btn_Item")] private GameObject _Obj_Item;
    [BindNode(true)] private GameObject _Img_Icon_Loop;


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

        var ctrl = GameManager.Instance.ChapterControl;

        var unlock = ctrl.GetChapterMaxBuild(chapterId) == ctrl.GetChapterBuildCount(chapterId);
        var isGet = ctrl.Model.ChapterRewords.Exists(p => p == chapterId);
        _Img_Icon_Loop.SetActiveEx(unlock && !isGet);
        _Obj_Item.SetActiveEx(unlock && !isGet);
        _UIGray.DoGray(isGet || !unlock);
        _Img_Icon.gameObject.SetActiveEx(isGet || !unlock);
    }

    private void OnClickBG(GameObject _, PointerEventData __)
    {
        GameManager.Instance.ChapterControl.SetChapterRewords(chapterId);
        SetUI(chapterId);
    }
}
