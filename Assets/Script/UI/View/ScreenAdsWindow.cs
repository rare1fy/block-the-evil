using DG.Tweening;
using Framework;
using System;
using System.Diagnostics.SymbolStore;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScreenAdsWindow : UIBase
{
    [BindNode] CustomText _Txt_Num;
    [BindNode(true)] GameObject _Btn_Mask;
    [BindNode(true)] GameObject _Btn_Ads;
    [BindNode(true)] GameObject _Obj_Reword;
    [BindNode(true)] GameObject _Obj_Perview;
    [BindNode(true)] GameObject _Obj_Mask;

    Action action;
    Animator animator;

    public override void InitOnce()
    {
        AddListener(ui_listener_type.onClick, "_Btn_Mask", OnClickMask);
        AddListener(ui_listener_type.onClick, "_Btn_Close", OnClickMask);
        AddListener(ui_listener_type.onClick, "_Btn_Ads", OnClickADs);
        animator = this.GetComponent<Animator>();
    }

    bool hiding = false;

    private void OnClickMask(GameObject o = null, PointerEventData d = null)
    {
        if (hiding)
            return;
        hiding = !hiding;
        animator.enabled = false;
        animator.enabled = true;
        animator.Play("hide");
        DOVirtual.DelayedCall(0.6f, () =>
        {
            action?.Invoke();
            base.OnBackClick();
        });
    }

    private void OnClickADs(GameObject @object, PointerEventData data)
    {
        var level = GameManager.Instance.PlayerControl.PlayerModel.Level;
        GameManager.Instance.PlayerControl.PlayerModel.SetAdsLv(level + 1);
        PlatformManager.Instance.ShowRewardedVideoAd(8, AdsCall);
    }

    private void AdsCall(bool isOk)
    {
        GameManager.Instance.LogManager.Log_AD(8, isOk);
        if (isOk)
        {
            animator.Play("show");
            AudioManagerNew.Instance.PlayAudio("UI_party_ad_reward.ogg");
            _Btn_Mask.SetActiveEx(true);
            _Btn_Ads.SetActiveEx(false);
            _Obj_Reword.SetActiveEx(true);
            _Obj_Perview.SetActiveEx(false);
            var ctrl = GameManager.Instance.PlayerControl;
            var num = Config.GetConfig<Config_GdConstant>().GetConfigById(33).Num;
            GameManager.Instance.GameBagControl.UpdateItems(GameBagModel.GOLD, ctrl.PlayerModel.Level * num);
        }
        else
        {
            OnClickMask();
        }

    }


    public override void OnOpen(object param = null)
    {
        animator.enabled = false;
        animator.enabled = true;
        animator.Play("show");
        _Obj_Mask.SetActiveEx(true);
        hiding = false;
        DOVirtual.DelayedCall(1f, () =>
        {
            _Obj_Mask.SetActiveEx(false);
        });
        AudioManagerNew.Instance.PlayAudio("UI_party_ad.ogg");
        if (param is Tuple<Action> tuple)
        {
            action = tuple.value1;
        }
        _Btn_Mask.SetActiveEx(false);
        _Btn_Ads.SetActiveEx(true);
        _Obj_Reword.SetActiveEx(false);
        _Obj_Perview.SetActiveEx(true);
        var ctrl = GameManager.Instance.PlayerControl;
        var num = Config.GetConfig<Config_GdConstant>().GetConfigById(33).Num;
        _Txt_Num.text = $"x{ctrl.PlayerModel.Level * num}";
    }
}
