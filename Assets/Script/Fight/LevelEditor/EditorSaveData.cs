using System;
using System.Collections.Generic;
using UnityEngine;

//格子数据类
[Serializable]
public class GridCellData
{
    public int color = 0;
    public int item = 0;
    public int bUsed = 0;

    public GridCellData(int color, int item)
    {
        this.color = color;
        this.item = item;
    }
}


[Serializable]
public class MaskData
{
    public int PosX = 0;
    public int PosY = 0;
    public int Length = 0; // 竖
    public int Width = 0; // 横
    public int Type = 0;

    public MaskData(int posX, int posY, int length, int width, int type)
    {
        PosX = posX;
        PosY = posY;
        Length = length;
        Width = width;
        Type = type;
    }
}

public class BoxData
{
    public int wait = Config.GetConfig<Config_GdConstant>().GetConfigById(25).Num;
    public Vector2Int pos;
    public BoxData(Vector2Int pos)
    {
        this.pos = pos;
    }
}

// ===== 关卡保存用的数据结构 =====
[Serializable]
public class GridSaveData
{
    public int size;
    public List<GridCellData> cells = new List<GridCellData>();
    public List<MaskData> masks = new List<MaskData>();

    public GridSaveData(int gridSize, GridCellData[,] gridData, List<MaskData> maskDatas)
    {
        size = gridSize;
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                cells.Add(gridData[x, y]);
            }
        }

        masks.AddRange(maskDatas);
    }
}