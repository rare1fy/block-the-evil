
using DG.Tweening;
using Google.Protobuf.WellKnownTypes;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public partial class MainWindow : UIBase
{
    [BindNode(true)] GameObject _Obj_ShareRed;
    [BindNode(true)] GameObject _Obj_AdsRed;
    [BindNode] CustomText _Txt_AddMoney;
    [BindNode] RectTransform _Obj_FlyGold;
    [BindNode] RectTransform _Obj_Tools;
    [BindNode] RectTransform _Obj_ResGroup;
    private bool sharing = false;

    private Tween refreshTween;

    public void InitOnceSDK()
    {
        AddListener(ui_listener_type.onClick, "_Btn_Share", OnClickShare);
        AddListener(ui_listener_type.onClick, "_Btn_Ads", OnClickAds);
        EventDispatchCenter.Instance.Registry(SDEvents.ACROSS_THE_DAY, RefreshRed);
        EventDispatchCenter.Instance.Registry(SDEvents.WX_ON_SHOW, ShareOver);
        EventDispatchCenter.Instance.Registry(SDEvents.MAIN_ADS_CALL, MainAdsCall);
    }

    public void OnOpenSDK(object param = null)
    {
        RefreshRed();
    }

    protected void OnDestroySDK()
    {
        EventDispatchCenter.Instance.UnRegistry(SDEvents.ACROSS_THE_DAY, RefreshRed);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.WX_ON_SHOW, ShareOver);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.MAIN_ADS_CALL, MainAdsCall);
    }

    private void ShareOver(object o = null)
    {
        var ctrl = GameManager.Instance.PlayerControl;

        if (sharing)
        {
            sharing = !sharing;
            if (ctrl.CheckShare())
            {
                ctrl.PlayerModel.SetShare();
                var num = Config.GetConfig<Config_GdConstant>().GetConfigById(37).Num;
                RefreshRed();
                var change = GetChangeMoney(num);
                FlyMoney(change.value1, change.value2);
            }
        }
    }

    private void MainAdsCall(object o =null)
    {
        var ctrl = GameManager.Instance.PlayerControl;
        var num = Config.GetConfig<Config_GdConstant>().GetConfigById(34).Num;
        ctrl.PlayerModel.SetMainWinAD();
        var change = GetChangeMoney(num);
        RefreshRed();
        FlyMoney(change.value1, change.value2);
    }

    private void RefreshRed(object o = null)
    {
        if (refreshTween != null)
        {
            refreshTween.Kill();
        }
        var ctrl = GameManager.Instance.PlayerControl;
        _Obj_AdsRed.SetActiveEx(ctrl.CheckAdsCount() && !ctrl.CheckAdsCd());
        _Obj_ShareRed.SetActiveEx(ctrl.CheckShare());
        if (ctrl.GetAdsCd() > 0)
        {
            refreshTween = DOVirtual.DelayedCall(ctrl.GetAdsCd() +0.1f, () =>
            {
                RefreshRed();
            });
        }
    }

    private void FlyMoney( int last, int cur)
    {
        Sequence scaleSequence = DOTween.Sequence();

        var fly = Instantiate(_Obj_FlyGold, _Obj_Tools);
        fly.localPosition = Vector3.zero;
        fly.gameObject.SetActiveEx(true);
        var endPos = _Obj_Tools.transform.InverseTransformPoint(_Img_Gold.transform.position);
        AudioManagerNew.Instance.PlayAudio("UI_Bo.ogg");
        scaleSequence.Append(fly.DOScale(1.2f, 0.3f).SetEase(Ease.InOutQuad));
        scaleSequence.Join(fly.DOMove(fly.position + new Vector3(0f, -1f, 0f), 0.3f));
        scaleSequence.AppendInterval(0.5f);
        scaleSequence.Append(fly.DOScale(0.2f, 0.5f).SetEase(Ease.InOutQuad));
        scaleSequence.Join(fly.DOLocalJump(endPos, 1.3f, 1, 0.5f).SetEase(Ease.OutQuad));
        scaleSequence.Play();
        scaleSequence.OnComplete(() =>
        {
            scaleSequence.Kill();
            Destroy(fly.gameObject);
            var add = Instantiate(_Txt_AddMoney, _Obj_ResGroup);
            add.text = $"+{cur - last}";
            add.gameObject.SetActiveEx(true);
            add.GetComponent<Animator>().Play("Add");
            AudioManagerNew.Instance.PlayAudio("UI_party_ad_reward.ogg");
            DOVirtual.Int(last, cur, 1f, v =>
            {
                _Txt_GoldNum.text = Util.FormatNumber(v);
            }).OnComplete(() =>
            {
                Destroy(add);
            });

        });
    }

    private void OnClickAds(GameObject @object, PointerEventData data)
    {
        UIManager.Instance.ShowUI("UIAdsWindow");
    }

    private Tuple<int, int> GetChangeMoney(int change)
    {
        var ctrl = GameManager.Instance.GameBagControl;
        var last = ctrl.GetItemNumberById(GameBagModel.GOLD);
        ctrl.UpdateItems(GameBagModel.GOLD, change);
        var cur = ctrl.GetItemNumberById(GameBagModel.GOLD);
        return new Tuple<int, int>(last, cur);
    }


    private void OnClickShare(GameObject @object, PointerEventData data)
    {
        var dic = Config.GetConfig<Config_ShareWechatImage>().m_ShareWechatImageDic;
        var randomValue = dic.GetRandomValue();
        sharing = true;
        PlatformManager.Instance.ShareMessage(randomValue.ShareText, randomValue.ShareImgUrl, ShareOver);
    }

}
