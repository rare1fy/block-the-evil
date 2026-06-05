using System;
using System.Collections.Generic;
using Pb;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class FightLevelModel : BaseModel
{
    private const int MaxPendingRewardColors = 3;
    private const int NormalEnemyPressureBlocksPerMonster = 3;
    private const int BossEnemyPressureBlocksPerMonster = 4;
    private const int NormalMaxEnemyPressureBlocks = 12;
    private const int BossMaxEnemyPressureBlocks = 16;
    private const int EarlyLevelInitialTargetColorBlocks = 3;
    private const int MiddleLevelInitialTargetColorBlocks = 2;
    private const int LateLevelInitialTargetColorBlocks = 1;
    private const int BossInitialTargetColorBlocks = 1;
    private const int EarlyLevelMaxActiveMonsters = 1;
    private const int MiddleLevelMaxActiveMonsters = 2;
    private const int LateLevelMaxActiveMonsters = 3;
    private const int BossWarmupMaxActiveMonsters = 2;

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
    public bool HasUsedFreeAdRevive { get; private set; }
    public int CopperReviveTimes { get; private set; }
    public bool BossMonsterCreated { get; private set; }

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

    public MonsterBattleState MonsterBattleState { get; private set; } = new MonsterBattleState();

    private int _nextBattleMonsterId = 1;
    private readonly List<int> _pendingRewardColors = new List<int>();

    /// <summary>
    /// npc数据
    /// </summary>
    public Dictionary<int, NpcInfo> NpcDic = new Dictionary<int, NpcInfo>();
    
    public void InitData(int levelId)
    {
        CreateNpcInfoData();
        ReviveTimes = 0;
        HasUsedFreeAdRevive = false;
        CopperReviveTimes = 0;
        BossMonsterCreated = false;
        _pendingRewardColors.Clear();
        _nextBattleMonsterId = 1;
        MonsterBattleState = new MonsterBattleState();
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

    public void RegisterOrderAsMonster(FightOrderData orderData, bool isBoss = false)
    {
        if (orderData == null || orderData.NeedBlockCount <= 0)
            return;

        var stages = new List<StageRequirement>
        {
            new StageRequirement(StageRequirementType.Color, orderData.NeedBlockId, orderData.NeedBlockCount)
        };
        if (isBoss)
        {
            stages.Add(new StageRequirement(StageRequirementType.Spirit, 0, 1));
        }

        orderData.BattleMonsterId = _nextBattleMonsterId++;
        MonsterBattleState.AddMonster(
            orderData.BattleMonsterId,
            isBoss,
            orderData.Index,
            stages);
    }

    public void UnregisterOrderMonster(FightOrderData orderData)
    {
        if (orderData == null || orderData.BattleMonsterId <= 0)
            return;

        MonsterBattleState.RemoveMonster(orderData.BattleMonsterId);
        orderData.BattleMonsterId = 0;
    }

    public bool IsOrderBattleComplete(FightOrderData orderData)
    {
        return orderData == null
               || orderData.BattleMonsterId <= 0
               || !MonsterBattleState.HasMonster(orderData.BattleMonsterId);
    }

    public int QueueTargetColorRewards(int count, IDictionary<int, int> availableColorCounts = null)
    {
        if (count <= 0)
            return 0;

        var combinedColorCounts = BuildCombinedAvailableColorCounts(availableColorCounts);

        var queuedCount = 0;
        for (var i = 0; i < count && _pendingRewardColors.Count < MaxPendingRewardColors; i++)
        {
            var colorType = MonsterBattleState.GetMostNeededColor(combinedColorCounts);
            if (colorType <= 0)
                break;

            _pendingRewardColors.Add(colorType);
            if (combinedColorCounts.ContainsKey(colorType))
            {
                combinedColorCounts[colorType]++;
            }
            else
            {
                combinedColorCounts.Add(colorType, 1);
            }
            queuedCount++;
        }

        return queuedCount;
    }

    private Dictionary<int, int> BuildCombinedAvailableColorCounts(IDictionary<int, int> availableColorCounts)
    {
        var combinedColorCounts = availableColorCounts != null
            ? new Dictionary<int, int>(availableColorCounts)
            : new Dictionary<int, int>();

        foreach (var colorType in _pendingRewardColors)
        {
            if (combinedColorCounts.ContainsKey(colorType))
            {
                combinedColorCounts[colorType]++;
            }
            else
            {
                combinedColorCounts.Add(colorType, 1);
            }
        }

        return combinedColorCounts;
    }

    public int ConsumePendingRewardColor()
    {
        if (_pendingRewardColors.Count <= 0)
            return 0;

        var colorType = _pendingRewardColors[0];
        _pendingRewardColors.RemoveAt(0);
        return colorType;
    }

    public bool HasPendingRewardColor()
    {
        return _pendingRewardColors.Count > 0;
    }

    public int GetEnemyPressureBlocksPerMonster()
    {
        return LevelBaseData != null && LevelBaseData.Boss == 1
            ? BossEnemyPressureBlocksPerMonster
            : NormalEnemyPressureBlocksPerMonster;
    }

    public int GetMaxEnemyPressureBlocks()
    {
        return LevelBaseData != null && LevelBaseData.Boss == 1
            ? BossMaxEnemyPressureBlocks
            : NormalMaxEnemyPressureBlocks;
    }

    public int GetInitialTargetColorBlockCount()
    {
        if (LevelBaseData == null)
            return 0;

        if (LevelBaseData.Boss == 1)
            return BossInitialTargetColorBlocks;

        if (LevelBaseData.Id <= 10)
            return EarlyLevelInitialTargetColorBlocks;

        return LevelBaseData.Id <= 30
            ? MiddleLevelInitialTargetColorBlocks
            : LateLevelInitialTargetColorBlocks;
    }

    public int GetMaxActiveMonsterSlots()
    {
        if (LevelBaseData == null)
            return EarlyLevelMaxActiveMonsters;

        if (LevelBaseData.Boss == 1)
            return BossWarmupMaxActiveMonsters;

        if (LevelBaseData.Id <= 10)
            return EarlyLevelMaxActiveMonsters;

        return LevelBaseData.Id <= 30
            ? MiddleLevelMaxActiveMonsters
            : LateLevelMaxActiveMonsters;
    }

    public int GetDesiredActiveMonsterSlots()
    {
        if (LevelBaseData == null || OrderList.Count <= 0)
            return 0;

        if (LevelBaseData.Boss == 1)
        {
            if (BossMonsterCreated)
                return 1;

            var warmupRemaining = Target - FinishOrderList.Count - 1;
            if (warmupRemaining <= 0)
                return FightOrders.Count <= 0 ? 1 : 0;

            return Math.Min(BossWarmupMaxActiveMonsters, warmupRemaining);
        }

        var maxSlots = GetMaxActiveMonsterSlots();
        if (maxSlots >= LateLevelMaxActiveMonsters && FinishOrderList.Count < 2)
            return MiddleLevelMaxActiveMonsters;

        return maxSlots;
    }

    public bool ShouldCreateBossMonster()
    {
        return LevelBaseData != null
               && LevelBaseData.Boss == 1
               && !BossMonsterCreated
               && FinishOrderList.Count >= Target - 1
               && FightOrders.Count <= 1;
    }

    public void MarkBossMonsterCreated()
    {
        BossMonsterCreated = true;
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

    public bool CanUseFreeAdRevive()
    {
        return !HasUsedFreeAdRevive;
    }

    public int GetNextCopperReviveCost()
    {
        switch (CopperReviveTimes)
        {
            case 0:
                return 100;
            case 1:
                return 500;
            default:
                return 1000;
        }
    }

    public void MarkFreeAdReviveUsed()
    {
        HasUsedFreeAdRevive = true;
    }

    public void MarkCopperReviveUsed()
    {
        CopperReviveTimes++;
    }
    
    public void Clear()
    {
        ReviveTimes = 0;
        HasUsedFreeAdRevive = false;
        CopperReviveTimes = 0;
        BossMonsterCreated = false;
        _pendingRewardColors.Clear();
        ColorPool.Clear();
        FightOrders.Clear();
        FinishOrderList.Clear();
        _nextBattleMonsterId = 1;
        MonsterBattleState = new MonsterBattleState();
    }
}

public class FightOrderData
{
    public int Index  { get; private set; } //位置
    public int NpcId  { get; private set; } //人物
    public int ItemId { get; private set; }  //物品
    public int NeedBlockId { get; private set; }  //需要的方块id
    public int NeedBlockCount { get; private set; }  //需要的方块数量
    public int BattleMonsterId { get; set; }

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
        BattleMonsterId = data.BattleMonsterId;
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
