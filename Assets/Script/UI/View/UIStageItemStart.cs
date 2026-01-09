using DG.Tweening;
using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

public class UIStageItemStart : UIItemBase
{
    [BindNode(true)] GameObject _Obj_AddNum;
    [BindNode] Image _Img_AddNum1;
    [BindNode] Image _Img_AddNum2;
    [BindNode] Image _Img_AddNum3;
    [BindNode] Image _Img_AddNum4;
    [BindNode] Image _Img_Num1;
    [BindNode] Image _Img_Num2;
    [BindNode] Image _Img_Num3;
    [BindNode] Image _Img_Num4;

    private List<Image> numsImg = new List<Image>();
    private List<Image> addNumsImg = new List<Image>();

    private UIStage ui;

    private Animator animator;

    protected override void InitItem()
    {
        addNumsImg = new List<Image>() { _Img_AddNum1, _Img_AddNum2, _Img_AddNum3, _Img_AddNum4 };
        numsImg = new List<Image>() { _Img_Num1, _Img_Num2, _Img_Num3, _Img_Num4 };
        animator = GetComponent<Animator>();
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_MUSIC_SELECT, ChangeAnim);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_MUSIC_SELECT, ChangeAnim);
    }

    private void ChangeAnim(object obj = null)
    {
        var musicId = GameManager.Instance.MusicControl.MainMusicId;
        musicId = musicId == 0 ? 1 : musicId;
        var musicCfg = Config.GetConfig<Config_MusicPlayer>().GetConfigById(musicId);
        animator.Play(musicCfg.BeatsAniMainDj);
    }

    public void SetUI(UIStage ui,int count)
    {
        _Obj_AddNum.SetActiveEx(false);
        SetNumUI(GameManager.Instance.StageControl.joined.Count);
        ShowTips(count);
        ChangeAnim();
    }

    public void SetNumUI(int num)
    {
        int idx = 0;
        while(num > 0)
        {
            var tmp = num % 10;
            num = num / 10;
            ResourceManagerNew.instance.LoadSpriteAsset($"UI_zjm_sj_{tmp}", numsImg[idx]);
            idx++;
        }
        for (int i = idx; i < 4; i++)
        {
            ResourceManagerNew.instance.LoadSpriteAsset($"UI_zjm_sj_{0}", numsImg[i]);
        }
    }

    public void SetAddNumUI(int num)
    {
        int idx = 0;
        while (num > 0)
        {
            var tmp = num % 10;
            num = num / 10;
            ResourceManagerNew.instance.LoadSpriteAsset($"UI_zjm_sj_{tmp}", addNumsImg[idx]);
            idx++;
        }
        for (int i = idx; i < 4; i++)
        {
            ResourceManagerNew.instance.LoadSpriteAsset($"UI_zjm_sj_{0}", numsImg[i]);
        }
    }

    public void ShowTips(int num)
    {
        if(num > 0)
        {
            _Obj_AddNum.SetActiveEx(true);
            SetAddNumUI(num);
            DOVirtual.DelayedCall(1.5f, () =>
            {
                _Obj_AddNum.SetActiveEx(false);
                SetNumUI(GameManager.Instance.StageControl.joined.Count);
            });
        }
        
    }
}
