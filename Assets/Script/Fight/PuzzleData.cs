using System.Collections.Generic;
using Pb;
using UnityEngine;

public class PuzzleData
{
    public int Id  { get; private set; }
    public BlockBase PuzzleCfg { get; private set; }
    public Dictionary<int, int> PosColorList; 
    public bool bUsed = false;
    public int ColorType;
    private readonly List<int> _posList;
    public List<int> PosList => _posList;
    
    public PuzzleData(int id)
    {
        Id = id;
        PuzzleCfg = Config.GetConfig<Config_BlockBase>().GetConfigById(Id);
        
        var blockStr = PuzzleCfg.Block.Split(";");
        _posList = new List<int>();
        foreach (var blockIndex in blockStr)
        {
            var index = int.Parse(blockIndex);
            _posList.Add(index);
        }
    }

    public void SetPuzzleData(int colorType, int itemType, int itemCount)
    {
        var randomList = Util.ShuffleAndTakeCopy(_posList, itemCount);
        PosColorList = new();
        ColorType = colorType;
        foreach (var pos in _posList)
        {
            if (randomList.Contains(pos))
            {
                PosColorList[pos] = itemType;
            }
            else
            {
                PosColorList[pos] = colorType;
            }
        }
    }

    public int PosCount()
    {
        return _posList.Count;
    }

    //获取以第一个点为原点的相对坐标
    public List<Vector2Int> GetCoordinates()
    {
        var puzzlePosList = new List<Vector2Int>();
        var lengthCount = PuzzleCfg.Length;

        foreach (var pos in _posList)
        {
            var t = pos - 1;
            var x = t / lengthCount;  // 行
            var y = t % lengthCount;   // 列
            puzzlePosList.Add(new Vector2Int(x, y));
        }

        var origin = puzzlePosList[0];
        for (var i = 0; i < puzzlePosList.Count; i++)
        {
            puzzlePosList[i] -= origin;
        }
        
        return puzzlePosList;
    }
    
    
}   