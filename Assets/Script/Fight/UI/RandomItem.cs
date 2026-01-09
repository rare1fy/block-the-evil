using System;
using System.Collections.Generic;
using Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RandomItem : UIItemBase
{
    public int Index;
    private Dictionary<int, Image> _blockImgDic = new Dictionary<int, Image>();
    [BindNode] private UIButtonExtension _Btn_Click;
    
    [BindNode] private RectTransform _Obj_Panel;
    [BindNode(nodeName:"_Obj_Panel")] private UIGray _Gray;
    
    private Animator _animator;
    protected override void InitItem()
    {
        _animator = GetComponent<Animator>();
        for (int i = 1; i < 17; i++)
        {
            var img = GetNodeByName<Image>($"_Img_Icon{i}");
            _blockImgDic.Add(i, img);
        }
    }

    public void InitPuzzle(int index)
    {
        Index = index;
        _Obj_Panel.gameObject.SetActiveEx(true);
        RefreshPuzzle(true);
        _animator.Play("RandomItem_Show");
    }
    
    private bool _canPut = false;
    public void RefreshPuzzle(bool bInit = false)
    {
        var puzzleData = GameManager.Instance.CurFightControl.Model.GetPuzzleData(Index);
        BShowPanel(!puzzleData.bUsed);
       
        if (!puzzleData.bUsed)
        {
            var lengthCount = puzzleData.PuzzleCfg.Length;
            var widthCount = puzzleData.PuzzleCfg.Width;
            var length = 39 * lengthCount - 3 * (lengthCount - 1);
            var width = 39 * widthCount - 4 * (widthCount - 1);
            _Obj_Panel.sizeDelta = new Vector2(length, width);
            
            var total = puzzleData.PuzzleCfg.Length * puzzleData.PuzzleCfg.Width;
            foreach (var blockItem in _blockImgDic)
            {
                if (blockItem.Key > total)
                {
                    blockItem.Value.gameObject.SetActive(false);
                }
                else
                {
                    blockItem.Value.gameObject.SetActive(true);
                    if (puzzleData.PosColorList.TryGetValue(blockItem.Key, out var color))
                    {
                        blockItem.Value.enabled = true;
                        var colorStr=  Config.GetConfig<Config_BlockColor>().GetColorImg(color);
                        ResourceManagerNew.instance.LoadSpriteAsset(colorStr, blockItem.Value);
                    }
                    else
                    {
                        blockItem.Value.enabled = false;
                    }
                }
            }
            
            var canPut = GameManager.Instance.CurFightControl.CheckPuzzleCanPut(puzzleData);
            _Btn_Click.enabled = canPut;
            if (bInit)
            {
                _canPut = canPut;
                _Gray.DoGray(!canPut);
            }
            else if (_canPut != canPut)
            {
                _canPut = canPut;
                _Gray.DoGray(!canPut);
            }
        }
        else
        {
            _Btn_Click.enabled = false;
        }
    }
    
    public void BindBtn(Action<GameObject, PointerEventData> begin, Action<GameObject, PointerEventData> on, Action<GameObject, PointerEventData> end)
    {
        AddListener(ui_listener_type.onButtonBeginDrag, "_Btn_Click", begin);
        AddListener(ui_listener_type.onButtonDrag, "_Btn_Click", on);
        AddListener(ui_listener_type.onButtonEndDrag, "_Btn_Click", end);
    }

    public void BShowPanel(bool bShow)
    {
        _Obj_Panel.gameObject.SetActiveEx(bShow);
    }

    public bool GetCanPut()
    {
        return _canPut;
    }
}