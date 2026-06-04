using System;
using System.Collections.Generic;
using Pb;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class FightLevelModel : BaseModel
{
    /// <summary>
    /// 章节id
    /// </summary>
    public int LevelId { get; set; }

    /// <summary>
    /// 是否是无尽模式
    /// </summary>
    public bool IsEndLess { get; set; }

    /// <summary>
    /// 目标
    /// </summary>
    public int Target { get; set; }

    /// <summary>
    /// 章节数据
    /// </summary>
    public LevelBase LevelBaseData { get; private set; }
    
    /// <summary>
    /// 章节分数
    /// </summary>
    public int LevelScore = 0;

    /// <summary>
    /// 已复活次数
    /// </summary>
    public int ReviveTimes { get; set; }

    /// <summary>
    /// 待完成订单列表
    /// </summary>
    public List<int> OrderList { get; private set; } = new List<int>();

    /// <summary>
    /// 颜色解锁数据 (key: 当局第n个订单  value: 增加的颜色)
    /// </summary>
    public Dictionary<int, int> ColorUnlockDic = new Dictionary<int, int>();

    /// <summary>
    /// 已完成的订单
    /// </summary>
    public List<FightOrderData> FinishOrderList { get; private set; } = new List<FightOrderData>();

    /// <summary>
    /// 颜色随机池
    /// </summary>
    public List<int> ColorPool { get; private set; } = new List<int>();

    /// <summary>
    /// 物品随机池
    /// </summary>
    public List<int> ColorItemPool { get; private set; } = new List<int>();

    /// <summary>
    /// 图形随机池
    /// </summary>
    public List<int> PuzzlePool { get; private set; } = new List<int>();

    /// <summary>
    /// 同时执行的订单数量
    /// </summary>
    public List<int> FightOrderIndex = new List<int>();

    /// <summary>
    /// 执行中的订单
    /// </summary>
    public List<FightOrderData> FightOrders { get; private set; } = new List<FightOrderData>();

    /// <summary>
    /// npc数据
    /// </summary>
    public Dictionary<int, NpcInfo> NpcDic = new Dictionary<int, NpcInfo>();
    
    public void InitData(int levelId)
    {
        CreateNpcInfoData();
        ReviveTimes = 0;
        LevelId = levelId;
        LevelBaseData = Config.GetConfig<Config_LevelBase>().GetConfigById(levelId);
        Target = LevelBaseData.Modletarget;
        OrderList = Config.GetConfig<Config_LevelOrder>().GetOrderIdList(levelId);
        
        if (!string.IsNullOrEmpty(LevelBaseData.ColorUnlock))
        {
            var unlockData = Util.GetRewardConfig(LevelBaseData.ColorUnlock);
            foreach (var unlock in unlockData)
            {
                ColorUnlockDic.Add(unlock.Id, (int)unlock.Number);
            }
        }

        //创建颜色随机池
        var colorStrList = LevelBaseData.BaseColor.Split(";");
        foreach (var color in colorStrList)
        {
            ColorPool.Add(int.Parse(color));
        }

        //创建物品随机池
        var colorItemStrList = LevelBaseData.BaseItem.Split(";");
        foreach (var colorItem in colorItemStrList)
        {
            ColorItemPool.Add(int.Parse(colorItem));
        }

        //创建图形随机池
        var poolStr = Config.GetConfig<Config_BlockLibrary>().GetConfigById(LevelBaseData.BlockPool).BlockPool;
        var puzzleStrList = poolStr.Split(";");
        foreach (var puzzle in puzzleStrList)
        {
            PuzzlePool.Add(int.Parse(puzzle));
        }
    }

    /// <summary>
    /// 关卡累计得分
    /// </summary>
    public void AddLevelScore(int score)
    {
        LevelScore += score;
        EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_UPDATE_SCORE, LevelScore);
    }
    
    /// <summary>
    /// 创建npc数据
    /// </summary>
    private void CreateNpcInfoData()
    {
        NpcDic.Clear();
        foreach (var npcData in GameManager.Instance.NpcControl.Model.NpcDic)
        {
            var data = npcData.Value;
            NpcDic.Add(npcData.Key, new NpcInfo()
            {
                id = data.Id,
                order = data.Order
            });
        }
    }

    //添加npc订单数据
    public void AddOrderDataWithNpc(FightOrderData data)
    {
        if (NpcDic.TryGetValue(data.NpcId, out var npcInfo))
        {
            npcInfo.order += 1;
        }
    }

    /// <summary>
    /// 获取随机拼图id
    /// </summary>
    /// <returns></returns>
    public int GetRandomPuzzleId()
    {
        return PuzzlePool[Random.Range(0, PuzzlePool.Count)];
    }

    /// <summary>
    /// 获取随机拼图创建物品数量
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public int GetRandomPuzzleCreateItemCount(int count)
    {
        var final = 0;
        for (var i = 0; i < count; i++)
        {
            var prob = LevelBaseData.ItemPercent;
            var r = Random.Range(0, 100);
            if (r < prob)
            {
                final += 1;
            }
        }

        return final;
    }
    
    /// <summary>
    /// ELO是否生效
    /// </summary>
    /// <returns></returns>
    public bool BELOEffect()
    {
        var basePercent = LevelBaseData.EloPercent;
        var deadPercent = LevelBaseData.EloDead * ReviveTimes;
        var total = basePercent + deadPercent;
        
        return Random.Range(0, 100) < total;
    }
    
    public int GetMaxReviveTimes()
    {
        if (IsEndLess)
        {
            return Config.GetConfig<Config_GdConstant>().GetConfigById(27).Num;
        }

        return Config.GetConfig<Config_GdConstant>().GetConfigById(26).Num;
    }
    
    public void Clear()
    {
        ReviveTimes = 0;
        ColorPool.Clear();
        FightOrders.Clear();
        FinishOrderList.Clear();
    }
}

public class FightOrderData
{
    public int Index  { get; private set; } //位置
    public int NpcId  { get; private set; } //人物
    public int ItemId { get; private set; }  //物品
    public int NeedBlockId { get; private set; }  //需要的方块id
    public int NeedBlockCount { get; private set; }  //需要的方块数量

    public FightOrderData(int index, int orderId)
    {
        Index = index;
        var orderConfig = Config.GetConfig<Config_LevelOrder>().GetConfigById(orderId);
        NpcId = GetRandomNpc(orderConfig.Npc);
        ItemId = Config.GetConfig<Config_NpcitemLibrary>().GetRandomItemId(orderConfig.Npcitem);
        var orderBlockCfg = Config.GetConfig<Config_OrderBlock>().GetConfigById(ItemId);
        var blockStr = orderBlockCfg.Block.Split("#");
        NeedBlockId = int.Parse(blockStr[0]);
        NeedBlockCount = int.Parse(blockStr[1]);
    }

    /// <summary>
    /// 无尽模式用
    /// </summary>
    public FightOrderData(int index, int npcId, int itemLibraryId)
    {
        Index = index;
        NpcId = npcId;
        ItemId = Config.GetConfig<Config_NpcitemLibrary>().GetRandomItemId(itemLibraryId);
        var orderBlockCfg = Config.GetConfig<Config_OrderBlock>().GetConfigById(ItemId);
        var blockStr = orderBlockCfg.Block.Split("#");
        NeedBlockId = int.Parse(blockStr[0]);
        NeedBlockCount = int.Parse(blockStr[1]);
    }

    /// <summary>
    /// 深拷贝用
    /// </summary>
    public FightOrderData(FightOrderData data)
    {
        Index = data.Index;
        NpcId = data.NpcId;
        ItemId = data.ItemId;
        NeedBlockId = data.NeedBlockId;
        NeedBlockCount = data.NeedBlockCount;
    }

    /// <summary>
    /// 完成需求
    /// </summary>
    /// <param name="blockData"></param>
    public bool SetNeedOrderData(BlockData blockData)
    {
        if (blockData.ColorType == NeedBlockId)
        {
            if (NeedBlockCount >= 1)
            {
                NeedBlockCount -= 1;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 查看订单是否完成
    /// </summary>
    /// <returns></returns>
    public bool CheckNeedListFinish()
    {
        return NeedBlockCount <= 0;
    }

    private int GetRandomNpc(int npcListId)
    {
        var config = Config.GetConfig<Config_NpcLibrary>().GetConfigById(npcListId);
        var npcIdListStr = config.Npcid.Split(";");

        if (npcIdListStr.Length == 1)
        {
            return int.Parse(npcIdListStr[0]);
        }

        var idList = npcIdListStr.Select(int.Parse).ToList();

        foreach (var fightOrderData in GameManager.Instance.CurFightControl.LevelController.Model.FightOrders)
        {
            if (idList.Contains(fightOrderData.NpcId))
            {
                idList.Remove(fightOrderData.NpcId);
            }
        }

        return idList[Random.Range(0, idList.Count)];
    }
}