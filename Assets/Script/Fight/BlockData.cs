using System.Collections.Generic;
using Pb;
using UnityEngine;

public enum EffectType
{
    None       = 0,
    LockOne    = 1,
    LockTwice  = 2,
    LockThrice = 3,
    /// <summary>
    /// 随机生成冰块
    /// </summary>
    CreateIce = 4,
    /// <summary>
    /// 随机生成物品
    /// </summary>
    CreateItem = 5,
    /// <summary>
    /// 炸弹
    /// </summary>
    Bomb = 6,
    /// <summary>
    /// 宝箱
    /// </summary>
    Box = 7,
}

public class BlockData
{
    public const int WhiteColorType = 31;

    public static bool IsLegacyItemColor(int colorType)
    {
        return colorType > 5 && colorType != WhiteColorType;
    }

    public static bool IsTargetColor(int colorType)
    {
        return colorType >= 1 && colorType <= 5;
    }

    public Vector2Int Pos { get; private set; }
    public int ColorType { get; private set; }
    public bool IsOccupied { get; private set; }   //是否被占据
    public int AttachedSpiritId { get; private set; }
    public bool HasAttachedSpirit => AttachedSpiritId > 0;
    
    public EffectType Effect = EffectType.None;
    
    public BlockData(int x, int y)
    {
        Pos = new Vector2Int(x, y);
    }

    /// <summary>
    /// 深拷贝用
    /// </summary>
    /// <param name="blockData"></param>
    public BlockData(BlockData blockData)
    {
        Pos = blockData.Pos;
        ColorType = blockData.ColorType;
        IsOccupied = blockData.IsOccupied;
        AttachedSpiritId = blockData.AttachedSpiritId;
        Effect = blockData.Effect;
    }
    
    public bool TriggerEffect2Destroy()
    {
        switch (Effect)
        {
            case EffectType.LockOne:
                Effect = EffectType.None;
                return false;
            case EffectType.LockTwice:
                Effect = EffectType.LockOne;
                return false;
            case EffectType.LockThrice:
                Effect = EffectType.LockTwice;
                return false;
        }
        return true;
    }
    
    
    public void SetColorType(int colorType)
    {
        ColorType = colorType;
    }
    public void SetIsOccupied(bool isOccupied)
    {
        IsOccupied = isOccupied;
    }

    public void AttachSpirit(int spiritId)
    {
        AttachedSpiritId = spiritId;
    }

    public int DetachSpirit()
    {
        var spiritId = AttachedSpiritId;
        AttachedSpiritId = 0;
        return spiritId;
    }
    
    public void Reset()
    {
        ColorType = 0;
        Effect = EffectType.None;
        IsOccupied = false;
        AttachedSpiritId = 0;
    }
}
