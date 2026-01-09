using System;
using System.Collections.Generic;
using Framework;
using UnityEngine;
using UnityEngine.UI;

public class RandomItemRay : UIItemBase
{
    public int Index;
    private Dictionary<int, Image> _blockImgDic = new Dictionary<int, Image>();
    private Dictionary<int, DragRay> _rayDic = new Dictionary<int, DragRay>();
    private List<DragRay> _rayList = new List<DragRay>();
    public PuzzleTargetData _targetData = new PuzzleTargetData();
    
    [BindNode] private RectTransform _Obj_Panel;
    protected override void InitItem()
    {
        for (var i = 1; i < 17; i++)
        {
            var itemName = $"_Img_Icon{i}";
            var img = GetNodeByName<Image>(itemName);
            var ray = GetNodeByName<DragRay>(itemName);
            ray.Init();
            
            _blockImgDic.Add(i, img);
            _rayDic.Add(i, ray);
        }
        
        _targetData.RayPosColorList = new Dictionary<Vector2Int, int>();
    }

    public void CloneItem(RandomItem item)
    {
        Index = item.Index;
        var puzzleData = GameManager.Instance.CurFightControl.Model.GetPuzzleData(Index);
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
    }

    public void CreateRayDray(Canvas canvas)
    {
        _rayList.Clear();
        var puzzleData = GameManager.Instance.CurFightControl.Model.GetPuzzleData(Index);
        if (_rayDic.TryGetValue(puzzleData.PosList[0], out var dragRay))
        {
            dragRay.SetCanvas(canvas);
            _rayList.Add(dragRay);
        }
    }

    public bool CheckRay()
    {
        if(_rayList.Count <= 0)
            return false;
        
        _targetData.RayPosColorList.Clear();
        var fightControl = GameManager.Instance.CurFightControl;
        var puzzleData = GameManager.Instance.CurFightControl.Model.GetPuzzleData(Index);
        
        foreach (var ray in _rayList)
        {
            var pos = ray.RayCast();
            if (pos.x < 0)  //超出屏幕或者 不在范围内
            {
                return false;
            }

            if (fightControl.CheckPosDataIsOcc(pos)) //位置被占用
            {
                return false;
            }
            
            var iPosList =  puzzleData.GetCoordinates();  //相对坐标

            for (var i = 0; i < iPosList.Count; i++)
            {
                var iPos = iPosList[i];
                var targetPos = pos + iPos;
                if (fightControl.CheckPosDataIsOcc(targetPos)) //目标格子被占
                {
                    return false;
                }
                
                var posIndex = puzzleData.PosList[i];
                var colorType = puzzleData.PosColorList[posIndex];
                _targetData.RayPosColorList[targetPos] = colorType;
            }
        }
        
        EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_SHOW_TEMP_BLOCK, _targetData);
        return true;
    }
}

public class PuzzleTargetData
{
    public Dictionary<Vector2Int, int> RayPosColorList = new Dictionary<Vector2Int, int>();
}