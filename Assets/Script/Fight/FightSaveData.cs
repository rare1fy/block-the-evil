using System;
using System.Collections.Generic;

//关卡储存结构
[Serializable]
public class FightSaveData
{
    /// <summary>
    /// 关卡ID
    /// </summary>
    private int LevelID;

    /// <summary>
    /// 模式类型 (0: 普通 1:无尽)
    /// </summary>
    private int ModelType;

    /// <summary>
    /// 是否无尽模式
    /// </summary>
    private bool IsEndless;

    /// <summary>
    /// 当前分数
    /// </summary>
    private int CurScore;

    /// <summary>
    /// 已经复活的次数
    /// </summary>
    public int ReviveTimes;

    /// <summary>
    /// 待完成订单
    /// </summary>
    public List<int> OrderList;
}

[Serializable]
public class FightOrderSaveData
{
    public int Index;            //位置
    public int NpcId;            //人物
    public int ItemId;           //物品
    public int NeedBlockId;      //需要的方块id
    public int NeedBlockCount;   //需要的方块数量
}