using Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIMainBuildItem : UIItemBase
{
    [BindNode(true)] GameObject _Img_Old;
    [BindNode(true)] GameObject _Img_New;

    int buildId;
    protected override void InitItem()
    {
    }

    public void SetUI(int buildId)
    {
        this.buildId = buildId; 
        var chapterModel = GameManager.Instance.ChapterControl.Model;
        if (chapterModel.ChapterDic.TryGetValue(chapterModel.curChapter, out var chapterData))
        {
            var isNew = chapterData.buildedIds.Exists(p => p == buildId);
            _Img_Old.SetActiveEx(!isNew);
            _Img_New.SetActiveEx(isNew);
        }
        else
        {
            _Img_Old.SetActiveEx(true);
            _Img_New.SetActiveEx(false);
        }
    }

  

    /// <summary>
    /// 建造建筑
    /// </summary>
    /// <param name="buildId"></param>
    public void OnBuild(int buildId)
    {
        _Img_Old.SetActiveEx(false);
        _Img_New.SetActiveEx(true);
    }
}
