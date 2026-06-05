using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Framework;
using Pb;
using UnityEngine;
using Random = UnityEngine.Random;

public class FightModel : BaseModel
{
    // 网格尺寸
    public const int GRID_WIDTH = 8;
    public const int GRID_HEIGHT = 8;

    /// <summary>
    /// 格子数据
    /// </summary>
    public BlockData[,] MBlockList { get; private set; }
    
    /// <summary>
    /// 钱
    /// </summary>
    public int Money => money;
    private int money;

    /// <summary>
    /// 复活次数
    /// </summary>
    public int Revive { get; private set;}

    /// <summary>
    /// 随机图形
    /// </summary>
    public List<PuzzleData> RandomPuzzleList { get; private set; } = new List<PuzzleData>();
     
    /// <summary>
    /// 遮罩数据
    /// </summary>
    public List<MaskData> MasksData { get; private set; } = new List<MaskData>();

    public List<BoxData> boxDatas = new List<BoxData>();
    
    public void InitFight(int levelId)
    {
        money = GameManager.Instance.GameBagControl.GetItemNumberById(GameBagModel.GOLD);
        var levelBaseData = Config.GetConfig<Config_LevelBase>().GetConfigById(levelId);
        var puzzleDataStrList = levelBaseData.InitialBlock.Split(';');
        var defaultColor = GetDefaultInitialColor(levelBaseData);
        RandomPuzzleList.Clear();
        for (var i = 0; i < 3; i++)
        {
            var puzzleDataStr = puzzleDataStrList[i];
            var puzzleData = puzzleDataStr.Split("#");
            var id = int.Parse(puzzleData[0]);
            var puzzle = new PuzzleData(id);
            puzzle.SetPuzzleData(defaultColor, 0, 0);
            RandomPuzzleList.Add(puzzle);
        }

        LoadGridData(levelBaseData.LevelMap);
    }

    private int GetDefaultInitialColor(LevelBase levelBaseData)
    {
        if (levelBaseData == null || string.IsNullOrEmpty(levelBaseData.BaseColor))
            return 0;

        var colorStrList = levelBaseData.BaseColor.Split(";");
        return int.TryParse(colorStrList[0], out var colorType) ? colorType : 0;
    }

    private void LoadGridData(string fileName)
    {
        ResourceManagerNew.instance.LoadAssetAsync<TextAsset>(fileName, text =>
        {
            var saveData = JsonUtility.FromJson<GridSaveData>(text.text);
            MBlockList = new BlockData[GRID_WIDTH, GRID_HEIGHT];
            boxDatas.Clear();
            for (var i = 0; i < GRID_WIDTH; i++)
            {
                for (var j = 0; j < GRID_HEIGHT; j++)
                {
                    var d = saveData.cells[i * saveData.size + j];
                    var blockData = new BlockData(i, j)
                    {
                        Effect = (EffectType)d.item,
                    };

                    if ((EffectType)d.item == EffectType.Box)
                    {
                        boxDatas.Add(new BoxData(new Vector2Int(i, j)));
                    }

                    blockData.SetColorType(d.color);
                    blockData.SetIsOccupied(d.color > 0 || d.bUsed == 1);
                    MBlockList[i, j] = blockData;
                }
            }

            MasksData.Clear();
            MasksData.AddRange(saveData.masks);
        });
    }
    
    public void AddMoney(int count)
    {
        GameManager.Instance.GameBagControl.UpdateItems(GameBagModel.GOLD, count);
        money = GameManager.Instance.GameBagControl.GetItemNumberById(GameBagModel.GOLD);
    }

    public BlockData GetBlockDataByPos(Vector2Int pos)
    {
        if(pos.x < 0 || pos.x >= GRID_WIDTH)
            return null;
        if(pos.y < 0 || pos.y >= GRID_HEIGHT)
            return null;
        
        return MBlockList[pos.x, pos.y];
    }

    public int GetIsOccupiedCount()
    {
        var t = 0;
        foreach (var blockData in MBlockList)
        {
            if(blockData.IsOccupied)
                t++;
        }
        return t;
    }

    public PuzzleData GetPuzzleData(int index)
    {
        if (index >= RandomPuzzleList.Count)
        {
            Debug.LogError("获取拼图数据错误 index:"+ index);
            return null;
        }
        return RandomPuzzleList[index];
    }

    /// <summary>
    /// 批量设置格子数据
    /// </summary>
    /// <param name="posList"></param>
    /// <param name="isOccupied">被使用</param>
    /// <param name="colorType">颜色类型</param>
    public void SetBlockDataByPosList(List<Vector2Int> posList, bool isOccupied, int colorType)
    {
        foreach (var pos in posList)
        {
            SetBlockDataByPos(pos, isOccupied, colorType);
        }
    }

    /// <summary>
    /// 批量设置格子数据
    /// </summary>
    /// <param name="posList"></param>
    /// <param name="isOccupied">被使用</param>
    /// <param name="colorType">颜色类型</param>
    public void SetBlockDataByPosList(List<Vector2Int> posList, bool isOccupied, int colorType, EffectType effectType)
    {
        foreach (var pos in posList)
        {
            var data = GetBlockDataByPos(pos);
            data.SetColorType(colorType);
            data.SetIsOccupied(isOccupied);
            data.Effect = effectType;
        }
    }

    public void SetBlockDataByPos(Vector2Int pos, bool isOccupied, int colorType)
    {
        var data = GetBlockDataByPos(pos);
        data.SetColorType(colorType);
        data.SetIsOccupied(isOccupied);
    }

    #region  消除统计
    
    public int MaxCombo = 0;   //本局最大连消
    public int CurCombo = 0;   //当前连消
    /// <summary>
    /// 统计消除
    /// </summary>
    public void AddCombo()
    {
        CurCombo++;
        if (CurCombo > MaxCombo)
        {
            MaxCombo = CurCombo;
        }
    }

    public void ClearCombo()
    {
        CurCombo = 0;
    }

    public float GetComboTime()
    {
        int id;
        switch (CurCombo)
        {
            case 1:
                id = 8;
                break;
            case 2:
                id = 9;
                break;
            case 3:
                id = 10; 
                break;
            case 4:
                id = 11;
                break;
            case 5:
                id = 12;
                break;
            default:
                id = 13;
                break;
        }
        return Config.GetConfig<Config_GdConstant>().GetConfigById(id).Num;
    }

    #endregion
}
