using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using DG.Tweening;
using Framework;
using UnityEngine;
using UnityEngine.UI;

public class BlockItem : UIItemBase
{
    [BindNode] private Image _Img_Color;
    [BindNode] private Image _Img_Effect;
    [BindNode(true)] private GameObject _Obj_FxParent;
    [BindNode(true)] private GameObject _Obj_Notice;
    [BindNode] public CustomText _Txt_Count;

    private Animator _animator;
    private CanvasGroup _canvasGroup;
    protected override void InitItem()
    {
        _animator = GetComponent<Animator>();
        _animator.enabled = false;
        _canvasGroup = GetComponent<CanvasGroup>();
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_SHOW_TEMP_BLOCK, SetTempBlock);
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_BLOCK_HOLD_EFFECT, PlayHoldEffect);
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_BLOCK_CLEAR_EFFECT, PlayClearEffect);
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_BLOCK_PUTDOWN_EFFECT, PutDownEffect);
        EventDispatchCenter.Instance.Registry(SDEvents.BLOCK_BOX_REFRESH, SetBoxCount);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_SHOW_TEMP_BLOCK, SetTempBlock);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_BLOCK_HOLD_EFFECT, PlayHoldEffect);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_BLOCK_CLEAR_EFFECT, PlayClearEffect);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_BLOCK_PUTDOWN_EFFECT, PutDownEffect);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.BLOCK_BOX_REFRESH, SetBoxCount);
    }

    [HideInInspector]
    public Vector2Int Pos;
    public void SetPos(Vector2Int pos)
    {
        Pos = pos;
        RefreshItem();
    }
    
    public void RefreshItem()
    {
        var blockData = GameManager.Instance.CurFightControl.Model.GetBlockDataByPos(Pos);
        _canvasGroup.alpha = 1;
        SetNeedImage(blockData.ColorType);
        _Img_Effect.gameObject.SetActive(blockData.Effect != 0);
        _Txt_Count.gameObject.SetActiveEx(blockData.Effect == EffectType.Box);
        SetBoxCount();
        if (blockData.Effect != 0)
        {
            var effectIconStr = Config.GetConfig<Config_BlockEffect>().GetConfigById((int)blockData.Effect).Img;
            ResourceManagerNew.instance.LoadSpriteAsset(effectIconStr, _Img_Effect);
        }

        var showNotice = blockData.HasAttachedSpirit
                         || GameManager.Instance.CurFightControl.LevelController.CheckItemOrderNeed(blockData.ColorType);
        _Obj_Notice.SetActiveEx(showNotice);
    }
    
    public void ClearEffectImage()
    {
        _Img_Effect.gameObject.SetActiveEx(false);
    }

    private void SetBoxCount(object obj = null)
    {
        var blockData = GameManager.Instance.CurFightControl.Model.GetBlockDataByPos(Pos);
        var boxDatas = GameManager.Instance.CurFightControl.Model.boxDatas;
        if (blockData.Effect == EffectType.Box)
        {
            _Txt_Count.text = boxDatas[0].wait.ToString();
        }
    }

    /// <summary>
    /// 拖拽显示
    /// </summary>
    private void SetTempBlock(object param = null)
    {
        if (param is PuzzleTargetData data)
        { 
            var curBlockData = GameManager.Instance.CurFightControl.Model.GetBlockDataByPos(Pos);
            if (!curBlockData.IsOccupied)
            {
                if (data.RayPosColorList.TryGetValue(Pos, out var color))
                {
                    SetNeedImage(color);
                    _canvasGroup.alpha = 0.5f;
                }
                else
                {
                    SetNeedImage(0);
                }
            }
        }
    }
    
    private void SetNeedImage(int colorType)
    {
        if (colorType == 0)
        {
            ResourceManagerNew.instance.LoadSpriteAsset("UI_zdn_img_touming", _Img_Color);
            return;
        }
        var colorCfg = Config.GetConfig<Config_BlockColor>().GetConfigById(colorType);
        ResourceManagerNew.instance.LoadSpriteAsset(colorCfg.Img, _Img_Color);
    }

    /// <summary>
    /// 放下方块特效
    /// </summary>
    private void PutDownEffect(object param)
    {
        if (param is Vector2Int pos)
        {
            if (pos == Pos)
            {
                transform.transform.DOScale(Vector3.one * 0.9f, 0.05f).SetLoops(2, LoopType.Yoyo);
                var curBlockData = GameManager.Instance.CurFightControl.GetDragTempData(Pos);
                var colorCfg = Config.GetConfig<Config_BlockColor>().GetConfigById(curBlockData.ColorType);
                // ResourceManagerNew.instance.LoadAssetAsync<GameObject>(colorCfg.Fx, fxObj =>
                // {
                //     var fx = Instantiate(fxObj, transform);
                //     DOVirtual.DelayedCall(0.1f, () =>
                //     {
                //         Destroy(fx);
                //     }); 
                // });
            }
        }
    }

    private GameObject _tempFx;
    /// <summary> 
    /// 消除预览特效
    /// </summary>
    private void PlayHoldEffect(object param)
    {
        if (param is Vector2Int pos)
        {
            if (pos == Pos)
            {
                if (_tempFx is not null)
                {
                    Destroy(_tempFx);
                    _tempFx = null;
                }
                
                var curBlockData = GameManager.Instance.CurFightControl.GetDragTempData(Pos);
                var colorCfg = Config.GetConfig<Config_BlockColor>().GetConfigById(curBlockData.ColorType);
                if (!string.IsNullOrEmpty(colorCfg.Fx))
                {
                    ResourceManagerNew.instance.LoadAssetAsync<GameObject>(colorCfg.Fx, fxObj =>
                    {
                        _tempFx = Instantiate(fxObj, transform);
                    });
                }
            }
        }
        else
        {
            if (_tempFx)
            {
                Destroy(_tempFx);
                _tempFx = null;
            }
            DestroyCloneChildren();
        }
    }

    private void PlayClearEffect(object param)
    {
        if (param is Vector2Int pos)
        {
            if (pos == Pos)
            {
                ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_C_TBC_Boom002", fxObj =>
                {
                    _animator.enabled = true;
                    _animator.Play("BlockItem_01");
                    DOVirtual.DelayedCall(0.2f, () =>
                    {
                        var fx = Instantiate(fxObj, transform);
                        fx.SetActiveEx(true);
                    });
                    DOVirtual.DelayedCall(1f, () =>
                    {
                        _animator.enabled = false;
                    });
                    
                });
                
                var fightCtrl = GameManager.Instance.CurFightControl;
                var curCombo = fightCtrl.Model.CurCombo;
                if (curCombo > 1)
                {
                    ResourceManagerNew.instance.LoadAssetAsync<GameObject>("BlockFinishEffect", fxObj =>
                    {
                        var fx = Instantiate(fxObj, _Obj_FxParent.transform);
                        fx.transform.localPosition = Vector3.zero;
                        var cls = fx.GetComponent<BlockFinishEffect>();
                        var config = Config.GetConfig<Config_FighteffectBase>().GetEffectConfig(2, curCombo);
                        if (fightCtrl.LevelController.Model.IsEndLess || fightCtrl.LevelController.Model.LevelBaseData.Modle == 2)
                        {
                            cls.ShowScore(config.Score / 8);
                        }
                        else
                        {
                            cls.ShowMoney(config.Reward / 8);
                        }
                    });
                }
            }
        }
    }

    public void CloseNoticeEffect()
    {
        _Obj_Notice.SetActiveEx(false);
    }

    private void DestroyCloneChildren()
    {
        var clones = new List<Transform>();
        
        foreach (Transform child in transform)
        {
            if (child.name.ToLower().Contains("clone"))
            {
                clones.Add(child);
            }
        }
        
        if (clones.Count == 0)
        {
            return;
        }
        
        // 从后往前销毁
        for (int i = clones.Count - 1; i >= 0; i--)
        {
            if (clones[i] != null)
            {
                Destroy(clones[i].gameObject);
            }
        }
    }

}
