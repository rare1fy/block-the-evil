using DG.Tweening;
using Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class UIFightMain : UIBase
{
    [BindNode] private Image _Obj_PropFlyIcon;
    [BindNode(true)] private GameObject _Obj_FlyEff;
    [BindNode] private Transform _Obj_Prop1;
    [BindNode] private Transform _Obj_Prop2;
    [BindNode] private Transform _Obj_Prop3;
    List<Transform> Props = new List<Transform>();

    public void InitEffect()
    {
        Props = new List<Transform>() { _Obj_Prop1, _Obj_Prop2, _Obj_Prop3 };
    }

    /// <summary>
    /// 打开宝箱
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="propId"></param>
    public void TriggerBoxEffect(Vector2Int pos, int propId)
    {
        int idx = (pos.x) * 8 + (pos.y);
        var item = _blockItemList[idx];
        var obj = Instantiate(_Obj_PropFlyIcon, _Obj_Prop.transform);
        GameManager.Instance.GameBagControl.SetImgIcon(propId, obj);
        var endPos = Props[propId - 3];
        obj.transform.localPosition = _Obj_Prop.transform.InverseTransformPoint(item.transform.position);
        StartScaleAnimation(obj.transform,() =>
        {
            obj.transform.DOLocalMove(endPos.localPosition, 0.3f)
                .OnComplete(() =>
                {
                    GameManager.Instance.GameBagControl.UpdateItems(propId, 1);
                    RefreshItem();
                    Destroy(obj);
                });
        });
        
    }

    public void RemoveBox(BlockItem item)
    {
        ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_C_numberbox_001", o =>
        {
            var eff = GameObject.Instantiate(o, item.transform);
            item.ClearEffectImage();
            eff.GetComponent<Animator>().Play("hide");
            DOVirtual.DelayedCall(0.5f, () =>
            {
                GameObject.Destroy(eff);
                item.RefreshItem();
            });
        });
    }

    public void SetBoxNumColor(BlockItem item)
    {
        item._Txt_Count.color = Color.red;
        DOVirtual.DelayedCall(0.1f, () =>
        {
            item._Txt_Count.color = Color.white;
        });
    }

    void StartScaleAnimation(Transform obj ,Action action)
    {
        // 创建动画序列
        Sequence scaleSequence = DOTween.Sequence();
        // 添加到序列：放大
        scaleSequence.Append(obj.DOScale(1.3f, 0.2f).SetEase(Ease.InOutQuad));
        // 添加到序列：缩小回原始尺寸
        scaleSequence.Append(obj.DOScale(1, 0.2f).SetEase(Ease.InOutQuad));

        // 设置循环模式为无限循环
        scaleSequence.SetLoops(-1, LoopType.Restart);
        // 播放动画
        scaleSequence.Play();
        DOVirtual.DelayedCall(0.6f, () =>
        {
            scaleSequence.Kill();
            obj.localScale = new Vector3(1, 1, 1);
            action?.Invoke();
        });
    }


    /// <summary>
    /// 触发冰桶
    /// </summary>
    /// <param name="cur"></param>
    /// <param name="target"></param>
    public void TriggerCreateIce(BlockItem curItem, BlockData target)
    {
        int targetIdx = (target.Pos.x)* 8 + (target.Pos.y);
        var targetItem = _blockItemList[targetIdx];
        target.Effect = EffectType.LockTwice;
        ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_C_Bingtong03_Trail", (obj) =>
        {
            var fly = Instantiate(obj, _Obj_Pool);
            fly.transform.localPosition = _Obj_Pool.transform.InverseTransformPoint(curItem.transform.position);
            var endPos = _Obj_Pool.transform.InverseTransformPoint(targetItem.transform.position);
            fly.transform.DOLocalMove(endPos, 0.2f)
                .OnComplete(() =>
                {
                    Destroy(fly);
                    ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_C_Bingtong02", o =>
                    {
                        var eff = Instantiate(o, targetItem.transform);
                        DOVirtual.DelayedCall(0.2f, () =>
                        {
                            targetItem.RefreshItem();
                            Destroy(eff);
                        });
                    });
                });
        });
    }

    /// <summary>
    /// 触发道具添加
    /// </summary>
    /// <param name="cur"></param>
    /// <param name="target"></param>
    /// <param name="itemId"></param>
    public void TriggerCreateItem(BlockItem curItem, BlockData target, int itemId)
    {
        int targetIdx = (target.Pos.x) * 8 + (target.Pos.y);
        var targetItem = _blockItemList[targetIdx];
        target.Effect = EffectType.None;
        target.SetColorType(itemId);
        string sprintName = Config.GetConfig<Config_BlockColor>().GetConfigById(itemId).Img1; 
        ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_C_Boxboom003_Trail", (obj) =>
        {
            var fly = Instantiate(obj, _Obj_Pool);
            fly.transform.localPosition = _Obj_Pool.transform.InverseTransformPoint(curItem.transform.position);
            var endPos = _Obj_Pool.transform.InverseTransformPoint(targetItem.transform.position);
            fly.transform.DOLocalMove(endPos, 0.2f)
                .OnComplete(() =>
                {
                    Destroy(fly);

                    ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_C_Boxboom002", o =>
                    {
                        var eff = Instantiate(o, targetItem.transform);
                        DOVirtual.DelayedCall(0.2f, () =>
                        {
                            targetItem.RefreshItem();
                            Destroy(eff);
                        });
                    });
                });
        });
    }


    public void PlayLockEffect(BlockData blockData)
    {
        string animName = "Ice";
        var startNum = (int)blockData.Effect;
        animName += $"{startNum}_0";
        var item = GetBlockItemByPos(blockData.Pos);
        ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_C_ice_001", o =>
        {
            var eff = GameObject.Instantiate(o, item.transform);
            item.ClearEffectImage();
            eff.GetComponent<Animator>().Play(animName);
            AudioManagerNew.Instance.PlayAudio("fight_item_icebreak.ogg");
            DOVirtual.DelayedCall(0.5f, () =>
            {
                GameObject.Destroy(eff);
            });
        });
    }

    public BlockItem GetBlockItemByPos(Vector2Int pos)
    {
        int curIdx = (pos.x) * 8 + (pos.y);
        return _blockItemList[curIdx];
    }



}
