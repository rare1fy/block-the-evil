using DG.Tweening;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildWindow : UIBase
{
    [BindNode]
    private UIBuildItem _Obj_BuildItem1;
    [BindNode]
    private UIBuildItem _Obj_BuildItem2;
    [BindNode]
    private UIBuildItem _Obj_BuildItem3;
    [BindNode]
    private UIBuildItem _Obj_BuildItem4;

    private UIBuildItem[] buildItems;

    [BindNode]
    private UIBuildRewordItem _Obj_Item;

    [BindNode]
    private Image _Img_Schedule;
    [BindNode]
    private CustomText _Txt_Schedule;
    [BindNode]
    private CustomText _Txt_Name;

    [BindNode]
    private Image _Img_Gold;
    [BindNode]
    private CustomText _Txt_GoldNum;
    [BindNode]
    private Image _Img_Star;
    [BindNode]
    private CustomText _Txt_StarNum;
    [BindNode]
    private Image _Img_FlyStar;
    [BindNode(true)]
    private GameObject _Obj_Fly;
    /// <summary>
    /// 章节id
    /// </summary>
    int ChapterId;

    //[BindNode]
    //private RectTransform _Obj_Content;

    public override void InitOnce()
    {
        AddListener(ui_listener_type.onClick, "_Btn_Close", base.OnBackClick);
        EventDispatchCenter.Instance.Registry(SDEvents.BUILD_REWORD, SetReword);

        buildItems = new[] { _Obj_BuildItem1, _Obj_BuildItem2, _Obj_BuildItem3, _Obj_BuildItem4 };
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventDispatchCenter.Instance.UnRegistry(SDEvents.BUILD_REWORD, SetReword);
    }

    public override void OnOpen(object param = null)
    {
        SetRes();
        _Obj_Fly.SetActiveEx(false);
        ChapterId = GameManager.Instance.ChapterControl.Model.curChapter;
        var ChapterControl = GameManager.Instance.ChapterControl;
        _Img_Schedule.fillAmount = ChapterControl.GetChapterBuildCount(ChapterId) / (float)ChapterControl.GetChapterMaxBuild(ChapterId);
        _Txt_Schedule.text = $"{ChapterControl.GetChapterBuildCount(ChapterId)} / {(float)ChapterControl.GetChapterMaxBuild(ChapterId)}";
        RefreshWindow();
    }

    public void RefreshWindow()
    {
        SetBuild();
        SetReword();
        _Txt_Name.text = $"章节{ChapterId}";
    }

    private void SetBuild()
    {
        var ChapterControl = GameManager.Instance.ChapterControl;
        var builds = ChapterControl.GetShowBuild(ChapterId);
        //建筑
        for (int i = 0; i < buildItems.Count(); i++)
        {
            bool isShow = builds.Count() > i;
            buildItems[i].gameObject.SetActiveEx(isShow);
            if (isShow)
            {
                buildItems[i].SetUI(builds[i], ChapterId, this);
            }
        }
        _Img_Schedule.DOFillAmount(ChapterControl.GetChapterBuildCount(ChapterId) / (float)ChapterControl.GetChapterMaxBuild(ChapterId), 1f)
            .OnComplete(() => 
            {
                _Txt_Schedule.text = $"{ChapterControl.GetChapterBuildCount(ChapterId)} / {(float)ChapterControl.GetChapterMaxBuild(ChapterId)}";
            });
    }

    private void SetRes()
    {
        var bagCtrl = GameManager.Instance.GameBagControl;
        bagCtrl.SetImgIcon(GameBagModel.GOLD, _Img_Gold);
        _Txt_GoldNum.text = bagCtrl.GetItemNumberById(GameBagModel.GOLD).ToString();
        bagCtrl.SetImgIcon(GameBagModel.Star, _Img_Star);
        bagCtrl.SetImgIcon(GameBagModel.Star, _Img_FlyStar);
        _Txt_StarNum.text = bagCtrl.GetItemNumberById(GameBagModel.Star).ToString();
    }

    private void SetReword(object obj = null)
    {
        var ctr = GameManager.Instance.ChapterControl;
        _Obj_Item.SetUI(ctr.Model.curChapter);
    }

    public void OnClickBuild(int cfgId, Vector2Int Needglod,Vector3 endPos)
    {
        var bagCtrl = GameManager.Instance.GameBagControl;
        var chapterCtrl = GameManager.Instance.ChapterControl;
        FlyStar(endPos, () =>
        {
            chapterCtrl.OnBuild(chapterCtrl.Model.curChapter, cfgId);
            bagCtrl.UpdateItems(Needglod.x, -Needglod.y);
            UIManager.Instance.HideUI(this);
        });
    }

    private void FlyStar(Vector2 endPos,Action EndCall)
    {
        var startPos = (_Obj_Fly.transform as RectTransform).InverseTransformVector(_Img_Star.rectTransform.position);
        var targetPos = (_Obj_Fly.transform as RectTransform).InverseTransformVector(endPos);
        _Img_FlyStar.rectTransform.localPosition = startPos;
        _Obj_Fly.SetActiveEx(true);
        _Img_FlyStar.rectTransform.DOLocalMove(targetPos, 0.5f)
            .OnComplete(() =>
            {
                EndCall?.Invoke();
                SetRes();
            });
    }
}
