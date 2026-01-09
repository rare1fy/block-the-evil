using Framework;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using System;
using System.Runtime.CompilerServices;

public partial class UIFightMain : UIBase
{
    [BindNode(true)] private GameObject _Obj_Spwan;
    [BindNode(true)] private GameObject _Obj_PropTools;
    [BindNode] private UIButtonExtension _Btn_RemoveItem;
    [BindNode(true)] private GameObject _Obj_RemoveColumn;
    [BindNode(true)] protected GameObject _Obj_Prop;
    [BindNode] protected Transform _Obj_Removes;
    [BindNode(true)] protected GameObject _Obj_RemoveItem;
    [BindNode] private Animator _Obj_Animator1;
    [BindNode] private Animator _Obj_Animator2;
    [BindNode] private Animator _Obj_Animator3;
    [BindNode(true)] private GameObject _img_Price1;
    [BindNode(true)] private GameObject _img_Price2;
    [BindNode(true)] private GameObject _img_Price3;
    [BindNode(true)] private GameObject _Txt_PropName1;
    [BindNode(true)] private GameObject _Txt_PropName2;
    [BindNode(true)] private GameObject _Txt_PropName3;
    [BindNode] private CustomText _Txt_Price1;
    [BindNode] private CustomText _Txt_Price2;
    [BindNode] private CustomText _Txt_Price3;
    [BindNode] private UIButtonExtension _Btn_Prop1;
    [BindNode] private UIButtonExtension _Btn_Prop2;
    [BindNode] private UIButtonExtension _Btn_Prop3;


    private void InitPartialOnce()
    {
        AddListener(ui_listener_type.onButtonPointerUp, "_Btn_Prop1", OnCliCkProp1);
        AddListener(ui_listener_type.onButtonPointerUp, "_Btn_Prop2", OnCliCkProp2);
        AddListener(ui_listener_type.onButtonPointerUp, "_Btn_Prop3", OnCliCkProp3);
        AddListener(ui_listener_type.onButtonPointerUp, "_Btn_Column1", OnClickRemoveColumn);
        AddListener(ui_listener_type.onButtonPointerUp, "_Btn_Column2", OnClickRemoveColumn);
        AddListener(ui_listener_type.onButtonPointerUp, "_Btn_Column3", OnClickRemoveColumn);
        AddListener(ui_listener_type.onButtonPointerUp, "_Btn_Column4", OnClickRemoveColumn);
        AddListener(ui_listener_type.onButtonPointerUp, "_Btn_RemoveItem", OnClickRemoveItem);
        AddListener(ui_listener_type.onClick, "_Btn_Back", OnClickPropTools);
    }
    
    public void OpenProp()
    {
        var consume = Config.GetConfig<Config_GdConstant>().GetConfigById(14).Num;
        _Txt_Price1.text = Util.FormatNumber(consume);
        consume = Config.GetConfig<Config_GdConstant>().GetConfigById(15).Num;
        _Txt_Price2.text = Util.FormatNumber(consume);
        consume = Config.GetConfig<Config_GdConstant>().GetConfigById(16).Num;
        _Txt_Price3.text = Util.FormatNumber(consume);
        _img_Price1.SetActiveEx(false);
        _img_Price2.SetActiveEx(false);
        _img_Price3.SetActiveEx(false);
    }

    public void RefreshPropItem(object param)
    {
        ItemConfig cfg = (ItemConfig)param;
        var bagCtrl = GameManager.Instance.GameBagControl;

        var num = bagCtrl.GetItemNumberById(GameBagModel.Prop1);
        _Txt_PropCount1.text = bagCtrl.GetItemNumberById(GameBagModel.Prop1).ToString();
        _img_Price1.SetActiveEx(num <= 0);
        _Txt_PropName1.SetActiveEx(num >0);

        num = bagCtrl.GetItemNumberById(GameBagModel.Prop2);
        _Txt_PropCount2.text = bagCtrl.GetItemNumberById(GameBagModel.Prop2).ToString();
        _img_Price2.SetActiveEx(num <= 0);
        _Txt_PropName2.SetActiveEx(num > 0);

        num = bagCtrl.GetItemNumberById(GameBagModel.Prop3);
        _Txt_PropCount3.text = bagCtrl.GetItemNumberById(GameBagModel.Prop3).ToString();
        _img_Price3.SetActiveEx(num <= 0);
        _Txt_PropName3.SetActiveEx(num > 0);
    }

    private void InitRemoves()
    {
        _Obj_RemoveColumn.SetActiveEx(true);
        _Obj_PropTools.SetActiveEx(true);
        _Btn_RemoveItem.gameObject.SetActiveEx(true);
        for (int i = 0; i < FightModel.GRID_HEIGHT; i++)
        {
            for (int j = 0; j < FightModel.GRID_WIDTH; j++)
            {
                var item = Instantiate(_Obj_RemoveItem, _Obj_Removes);
                item.SetActiveEx(true);
                item.GetComponent<BlockRay>().SetItem(new Vector2Int(i, j));
            }
        }
        _Obj_RemoveColumn.SetActiveEx(false);
        _Obj_PropTools.SetActiveEx(false);
        _Btn_RemoveItem.gameObject.SetActiveEx(false);
    }

    private void OnClickPropTools(GameObject o, PointerEventData e)
    {
        _Obj_RemoveColumn.SetActiveEx(false);
        _Obj_PropTools.SetActiveEx(false);
        _Btn_RemoveItem.gameObject.SetActiveEx(false);
        _Obj_Spwan.SetActiveEx(true);
    }

    private void OnClickRemoveItem(GameObject o, PointerEventData e)
    {
        var raycaster = mCanvas.GetComponent<GraphicRaycaster>();
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(e, results);
        foreach (RaycastResult result in results)
        {
            var script = result.gameObject.GetComponentInParent<BlockRay>();
            var block = script.data;
            if (script != null)
            {
                _Obj_PropTools.SetActiveEx(false);
                _Btn_RemoveItem.gameObject.SetActiveEx(false);
                if (!block.IsOccupied || block.ColorType == 0)
                {
                    UIManager.Instance.ShowPromptWindow("目标位置没有色块");
                    return;
                }
                var levelModel = _fightController.LevelController.Model;
                GameManager.Instance.LogManager.Log_GameUseProp(levelModel.LevelId, GameBagModel.Prop1, levelModel.IsEndLess);
                
                var bagCtrl = GameManager.Instance.GameBagControl;
                bagCtrl.UpdateItems(GameBagModel.Prop1, -1);
                ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_Tool_Chuizi", (obj) =>
                {
                    var pos = this.transform.InverseTransformPoint(script.transform.position);
                    var eff = Instantiate(obj, this.transform);
                    eff.transform.localPosition = pos;
                    AudioManagerNew.Instance.PlayAudio("fight_prop_hammer.ogg");
                    StartCoroutine(_fightController.UseRemoveItem(script.pos, this));
                    DOVirtual.DelayedCall(0.5f, () =>
                    {
                        Destroy(eff);
                    });
                });
                return;
            }
        }
        _Obj_PropTools.SetActiveEx(false);
        _Btn_RemoveItem.gameObject.SetActiveEx(false);
    }

    private void OnClickRemoveColumn(GameObject o, PointerEventData e)
    {
        int idx = 1;
        switch (o.name)
        {
            case "_Btn_Column1":
                idx = 1; break;
            case "_Btn_Column2":
                idx = 2; break;
            case "_Btn_Column3":
                idx = 3; break;
            default:
                idx = 4; break;
        }
        
        var levelModel = _fightController.LevelController.Model;
        GameManager.Instance.LogManager.Log_GameUseProp(levelModel.LevelId, GameBagModel.Prop2, levelModel.IsEndLess);
        
        _Obj_RemoveColumn.SetActiveEx(false);
        _Obj_PropTools.SetActiveEx(false);
        _Obj_Spwan.SetActiveEx(true);
        var bagCtrl = GameManager.Instance.GameBagControl;
        bagCtrl.UpdateItems(GameBagModel.Prop2, -1);
        ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_Tool_Xiangbing", (obj) =>
        {
            AudioManagerNew.Instance.PlayAudio("fight_prop_champagne.ogg");

            var pos = this.transform.InverseTransformPoint(o.transform.position);
            var eff = Instantiate(obj, this.transform);
            eff.GetComponentInParent<Transform>().localPosition = new Vector3(pos.x, 0, 0);
            StartCoroutine(_fightController.UseRemoveColumn(idx, this));
            DOVirtual.DelayedCall(0.6f, () =>
            {
                Destroy(eff);
            });
        });
    }

    private void PlaySharke()
    {
        var levelModel = _fightController.LevelController.Model;
        GameManager.Instance.LogManager.Log_GameUseProp(levelModel.LevelId, GameBagModel.Prop3, levelModel.IsEndLess);
    
        var bagCtrl = GameManager.Instance.GameBagControl;
        bagCtrl.UpdateItems(GameBagModel.Prop3, -1);
        ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_Tool_Tiaojiu", (obj) =>
        {
            AudioManagerNew.Instance.PlayAudio("fight_prop_shake.ogg");
            var o = Instantiate(obj, this.transform);
            _Obj_Prop.SetActiveEx(false);
            _Obj_Spwan.SetActiveEx(false);
            DOVirtual.DelayedCall(1.917f, () =>
            {
                Destroy(o);
                _Obj_Spwan.SetActiveEx(true);
                _Obj_Prop.SetActiveEx(true);
            });
        });
    }

    private void OnCliCkProp1(GameObject o, PointerEventData e)
    {
        var bagCtrl = GameManager.Instance.GameBagControl;
        var consume = Config.GetConfig<Config_GdConstant>().GetConfigById(14).Num;
        if (bagCtrl.GetItemNumberById(GameBagModel.Prop1) > 0)
        {
            _Obj_PropTools.SetActiveEx(true);
            _Btn_RemoveItem.gameObject.SetActiveEx(true);
        }
        else if (bagCtrl.GetItemNumberById(GameBagModel.GOLD) >= consume)
        {
            UIManager.Instance.ShowSecondConfirm($"是否消耗{consume}钞票购买道具", () =>
            {
                var _fightController = GameManager.Instance.CurFightControl;
                _fightController.Model.AddMoney(-consume);
                AddPropAnim1(Props[0], GameBagModel.Prop1, () =>
                {
                    AddProp(GameBagModel.Prop1, 1, 2);
                });
            });
        }
        else
        {
            UIManager.Instance.ShowSecondConfirm("是否观看广告获得道具", () =>
            {
                PlatformManager.Instance.ShowRewardedVideoAd(3, (isOk) =>
                {
                    GameManager.Instance.LogManager.Log_AD(3, isOk);
                    if (isOk)
                    {
                        AddPropAnim2(Props[0], GameBagModel.Prop1, () =>
                        {
                            var num = Config.GetConfig<Config_GdConstant>().GetConfigById(17).Num;
                            AddProp(GameBagModel.Prop1, num, 1);
                        });
                    }
                    else
                    {
                        UIManager.Instance.ShowPromptWindow("观看时间不足，无法获取奖励");
                    }

                });
            }, isAd: true);
        }
    }

    private void OnCliCkProp2(GameObject o, PointerEventData e)
    {
        var bagCtrl = GameManager.Instance.GameBagControl;
        var consume = Config.GetConfig<Config_GdConstant>().GetConfigById(15).Num;
        if (bagCtrl.GetItemNumberById(GameBagModel.Prop2) > 0)
        {
            _Obj_PropTools.SetActiveEx(true);
            _Obj_RemoveColumn.SetActiveEx(true);
            _Obj_Spwan.SetActiveEx(false);
        }
        else if (bagCtrl.GetItemNumberById(GameBagModel.GOLD) >= consume)
        {
            UIManager.Instance.ShowSecondConfirm($"是否消耗{consume}钞票购买道具", () =>
            {
                AddPropAnim1(Props[1], GameBagModel.Prop2, () =>
                {
                    AddProp(GameBagModel.Prop2, 1, 2);
                });
                var _fightController = GameManager.Instance.CurFightControl;
                _fightController.Model.AddMoney(-consume);
                
            });
        }
        else
        {
            UIManager.Instance.ShowSecondConfirm("是否观看广告获得道具", () =>
            {
                PlatformManager.Instance.ShowRewardedVideoAd(4, (isOk) =>
                {
                    GameManager.Instance.LogManager.Log_AD(4, isOk);
                    if (isOk)
                    {
                        var num = Config.GetConfig<Config_GdConstant>().GetConfigById(18).Num;
                        AddPropAnim2(Props[1], GameBagModel.Prop2, () =>
                        {
                            AddProp(GameBagModel.Prop2, num, 1);
                        });
                    }
                    else
                    {
                        UIManager.Instance.ShowPromptWindow("观看时间不足，无法获取奖励");
                    }

                });


            },isAd: true);
        }
    }

    private void OnCliCkProp3(GameObject o, PointerEventData e)
    {

        var bagCtrl = GameManager.Instance.GameBagControl;
        var consume = Config.GetConfig<Config_GdConstant>().GetConfigById(16).Num;
        if (bagCtrl.GetItemNumberById(GameBagModel.Prop3) > 0)
        {
            PlaySharke();
        }
        else if (bagCtrl.GetItemNumberById(GameBagModel.GOLD) >= consume)
        {
            UIManager.Instance.ShowSecondConfirm($"是否消耗{consume}钞票购买道具", () =>
            {
                var _fightController = GameManager.Instance.CurFightControl;
                _fightController.Model.AddMoney(-consume);
                AddPropAnim1(Props[2], GameBagModel.Prop3, () =>
                {
                    AddProp(GameBagModel.Prop3, 1, 2);
                });
            });
        }
        else
        {

            UIManager.Instance.ShowSecondConfirm("是否观看广告获得道具", () =>
            {
                PlatformManager.Instance.ShowRewardedVideoAd(5, (isOk) =>
                {
                    GameManager.Instance.LogManager.Log_AD(5, isOk);
                    if (isOk)
                    {
                        var num = Config.GetConfig<Config_GdConstant>().GetConfigById(19).Num;
                        AddPropAnim2(Props[2], GameBagModel.Prop3, () =>
                        {
                            AddProp(GameBagModel.Prop3, num, 1);
                        });
                    }
                    else
                    {
                        UIManager.Instance.ShowPromptWindow("观看时间不足，无法获取奖励");
                    }

                });
            },isAd: true);
        }
    }

    public void SetPropItemAnim(string name)
    {
        _Obj_Animator1.Play(name);
        _Obj_Animator2.Play(name);
        _Obj_Animator3.Play(name);

    }


    //添加道具
    private void AddProp(int propId, int addCount, int addType)
    {
        var levelMode = _fightController.LevelController.Model;
        
        GameManager.Instance.GameBagControl.UpdateItems(propId, addCount);
        
        GameManager.Instance.LogManager.Log_GameBuyProp(
            levelMode.LevelId, propId, addType, levelMode.IsEndLess);
    }

    private void AddPropAnim1(Transform target, int id, Action call)
    {
        _Btn_Prop3.enabled = false;
        _Btn_Prop2.enabled = false;
        _Btn_Prop1.enabled = false;
        var item = Instantiate(_Obj_PropFlyIcon, _Obj_Pool);
        var start = _Obj_Pool.transform.InverseTransformPoint(_Obj_Money.transform.position);
        item.rectTransform.localPosition = start;
        GameManager.Instance.GameBagControl.SetImgIcon(id, item);
        Sequence animationSequence = DOTween.Sequence();
        // 第一阶段
        animationSequence.Append(item.rectTransform.DOAnchorPos(Vector2.zero, 0.5f));
        animationSequence.Join(item.rectTransform.DOScale(1.2f, 0.5f));
        animationSequence.Play();    // 播放动画
        AudioManagerNew.Instance.PlayAudio("fight_item_popup_fly.ogg");
        animationSequence.onComplete = () =>
        {
            AddPropAnim2(target, id, call, item);
            animationSequence.Kill();
        };
    }

    private void AddPropAnim2(Transform target, int id, Action call,Image item = null)
    {
        _Btn_Prop3.enabled = false;
        _Btn_Prop2.enabled = false;
        _Btn_Prop1.enabled = false;
        if (item == null)
        {
            item = Instantiate(_Obj_PropFlyIcon, _Obj_Pool);
            item.rectTransform.localPosition = Vector3.zero;
            GameManager.Instance.GameBagControl.SetImgIcon(id, item);
            AudioManagerNew.Instance.PlayAudio("fight_item_popup_fly.ogg");
        }
        item.rectTransform.parent = _Obj_Prop.transform;
        Sequence animationSequence = DOTween.Sequence();
        // 0.5秒停顿
        animationSequence.AppendInterval(0.5f);
        // 第一阶段
        animationSequence.Append(item.rectTransform.DOLocalMove(target.localPosition, 0.5f));
        animationSequence.Join(item.rectTransform.DOScale(1f, 0.5f));
        animationSequence.Play();
        animationSequence.onComplete = () =>
        {
            Destroy(item.gameObject);
            animationSequence.Kill();
            call?.Invoke();
            _Btn_Prop3.enabled = true;
            _Btn_Prop2.enabled = true;
            _Btn_Prop1.enabled = true;
        };
    }
}