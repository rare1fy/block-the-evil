using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG;
using DG.Tweening;
using System;
using Framework;
using Google.Protobuf.WellKnownTypes;

public class FeelView : UIItemBase
{
    [BindNode]
    Image _Img_Lv1;
    [BindNode]
    Image _Img_Lv2;
    [BindNode]
    Image _Img_GoodFeeling;
    [BindNode]
    RawImage _Img_Head;
    [BindNode]
    CustomText _Txt_GoodFeeling;
    Animator _anim;

    int addExp;
    int curExp;
    int Lv;
    /// <summary>
    /// 结束回调（处理多个好感提升）
    /// </summary>
    Action CloseAction;

    protected override void InitItem()
    {
        _anim = GetComponent<Animator>();
    }

    public void InitFeelView(Action closeAction)
    {
        CloseAction = closeAction;
    }

    /// <summary>
    /// 只设置参数
    /// </summary>
    /// <param name="lv"></param>
    /// <param name="exp"></param>
    public void SetInitialFeelView(int lv, int exp, int npcId)
    {
        var cfg = Config.GetConfig<Config_GoodfeelLv>().GetConfigById(lv);
        var npcCfg = Config.GetConfig<Config_NpcBase>().GetConfigById(npcId);
        Lv = lv;
        ResourceManagerNew.instance.LoadTextureAsset(npcCfg.Img4, _Img_Head);
        _Img_GoodFeeling.fillAmount = exp / (float)cfg.Exp;
        _Txt_GoodFeeling.text = $"{exp}/{cfg.Exp}";
        gameObject.SetActiveEx(false);
        SetLevel();
    }

    public void PlayViewDoTween(int addExp)
    {
        int curLv = Lv;
        var cfg = Config.GetConfig<Config_GoodfeelLv>().GetConfigById(curLv);
        var isMax = cfg.Max == 1;
        var allExp = addExp + curExp;
        var checkLvUp = allExp >= cfg.Exp;
        var lastExp = checkLvUp ? cfg.Exp : allExp;
        int startExp = curExp;
        DOTween.To(() => startExp, x =>
        {
            _Img_GoodFeeling.fillAmount = x / (float)cfg.Exp;
            _Txt_GoodFeeling.text = $"{x}/{cfg.Exp}";
        }, lastExp, 0.3f)
            .SetLoops(1)
            .OnComplete(() =>
            {
                if (checkLvUp)
                {
                    if (isMax)
                    {
                        _Img_GoodFeeling.fillAmount = 1;
                        _Txt_GoodFeeling.text = $"{cfg.Exp}/{cfg.Exp}";
                        DOVirtual.DelayedCall(1f, () =>
                        {
                            _anim.Play("Hide");
                        });
                        return;
                    }
                    Lv = curLv + 1;
                    SetLevel();
                    var lastCfg = Config.GetConfig<Config_GoodfeelLv>().GetConfigById(curLv + 1);
                    curExp = 0;
                    _Img_GoodFeeling.fillAmount = 0;
                    _Txt_GoodFeeling.text = $"0/{lastCfg.Exp}";
                    PlayViewDoTween(allExp - cfg.Exp);
                }
                else
                {
                    curExp = allExp;
                    _Txt_GoodFeeling.text = $"{curExp}/{cfg.Exp}";

                    DOVirtual.DelayedCall(1f, () =>
                    {
                        _anim.Play("Hide");
                    });
                }
            });
    }

    /// <summary>
    /// 打开界面
    /// </summary>
    /// <param name="addExp"></param>
    public void ShowView(int addExp)
    {
        AudioManagerNew.Instance.PlayAudio("fight_likerise");
        gameObject.SetActiveEx(true);
        this.addExp = addExp;
        //播放入场动画
        _anim.Play("Open");
       
    }

    public void OpenOver()
    {
        //设置数据
        PlayViewDoTween(addExp);
    }

    public void HideOver()
    {
        gameObject.SetActiveEx(false);
        CloseAction?.Invoke();
    }

    private void SetLevel()
    {
        if (Lv >= 10)
        {
            _Img_Lv2.gameObject.SetActiveEx(true);
            _Img_Lv1.gameObject.SetActiveEx(true);
            ResourceManagerNew.instance.LoadSpriteAsset($"UI_zdn_txt_{Lv / 10}", _Img_Lv2);
            ResourceManagerNew.instance.LoadSpriteAsset($"UI_zdn_txt_{Lv % 10}", _Img_Lv1);
        }
        else
        {
            _Img_Lv2.gameObject.SetActiveEx(false);
            _Img_Lv1.gameObject.SetActiveEx(true);
            ResourceManagerNew.instance.LoadSpriteAsset($"UI_zdn_txt_{Lv % 10}", _Img_Lv1);
        }
    }
}
