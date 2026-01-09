using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChapterBuildItem : UIItemBase
{
    [BindNode(true)]
    GameObject _Obj_Old;
    [BindNode(true)]
    GameObject _Obj_New;
    [BindNode(true)]
    GameObject _Obj_Eff;
    Animator animator;

    protected override void InitItem()
    {
        animator = GetComponent<Animator>();
        animator.enabled = false;
    }

    public void SetItem(int buildId)
    {
        _Obj_Eff.SetActiveEx(false);
        var ctrl = GameManager.Instance.ChapterControl;
        var chapterModel = GameManager.Instance.ChapterControl.Model;
        int chapter = chapterModel.curChapter;
        bool isBuild = ctrl.GetCompletedBuild().Exists(p => buildId == p);
        _Obj_Old.SetActiveEx(!isBuild);
        _Obj_New.SetActiveEx(isBuild);
    }

    public void OnBuilding()
    {
        //ÌØÐ§
        _Obj_Eff.SetActiveEx(true);
        AudioManagerNew.Instance.PlayAudio("scene_build.ogg");
        DOVirtual.DelayedCall(1f, () =>
        {
            _Obj_Eff.SetActiveEx(false);
            animator.enabled = true;
        });

        //_Obj_Old.SetActiveEx(false);
        //_Obj_New.SetActiveEx(true);
    }
}
