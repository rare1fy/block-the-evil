using System;
using System.Collections.Generic;
using System.Linq;
using Pb;
using UnityEngine;
using Random = UnityEngine.Random;

public class FightLevelController
{
    private FightLevelModel model;

    public FightLevelModel Model
    {
        get
        {
            if (ReferenceEquals(model, null))
            {
                model = new FightLevelModel();
            }

            return model;
        }
    }

    private FightController _fightController => GameManager.Instance.CurFightControl;

    public FightLevelController(int levelId)
    {
        Model.InitData(levelId);
    }

    /// <summary>
    /// 无尽模式额外数据
    /// </summary>
    public void InitEndlessOrder(LevelWujingModle config)
    {
        Model.LevelId = config.Id; //前面是用的是随机出来的关卡id去创建数据  无尽模式要改用无尽模式的id  
        Model.IsEndLess = true;
        Model.Target = config.Npc;

        //构造一个特殊的订单作为第一个
        var itemLibrary = Config.GetConfig<Config_LevelOrder>().GetConfigById(Model.OrderList[^1]).Npcitem;
        var fightOrderData = new FightOrderData(0, config.SpNpc, itemLibrary);
        Model.FightOrders.Add(fightOrderData);
        Model.RegisterOrderAsMonster(fightOrderData);

        CreateFightOrder(2);
    }

    /// <summary>
    /// 结算消除
    /// </summary>
    public void TotalClearBlock(List<Vector2Int> posList, UIFightMain ui)
    {
        var blockDataList = new List<BlockData>();
        foreach (var pos in posList)
        {
            var blockData = _fightController.Model.GetBlockDataByPos(pos);
            if (blockData != null)
            {
                blockDataList.Add(blockData);
            }
        }

        foreach (var blockData in blockDataList)
        {
            if (blockData.HasAttachedSpirit)
                Model.MonsterBattleState.ConsumeSpirit(blockData.DetachSpirit());
        }

        Model.MonsterBattleState.ConsumeClearedColors(blockDataList
            .Where(blockData => blockData.ColorType > 0)
            .Select(blockData => blockData.ColorType));

        Dictionary<FightOrderData, List<BlockData>> data = new();

        foreach (var fightOrderData in Model.FightOrders)
        {
            for (var i = blockDataList.Count - 1; i >= 0; i--)
            {
                var blockData = blockDataList[i];
                if (!fightOrderData.CheckNeedListFinish()) //判断是否完成
                {
                    if (fightOrderData.SetNeedOrderData(blockData)) //完成订单
                    {
                        if (data.TryGetValue(fightOrderData, out var blockDatas))
                        {
                            blockDatas.Add(new BlockData(blockData));
                        }
                        else
                        {
                            blockDatas = new();
                            data.Add(fightOrderData, blockDatas);
                            blockDatas.Add(new BlockData(blockData));
                        }

                        blockDataList.RemoveAt(i); //移除
                    }
                }
            }
        }

        ui._Obj_OrderPanel.PlayFlyEffect(data);
        for (var i = blockDataList.Count - 1; i >= 0; i--)
        {
            var blockData = blockDataList[i];
            if (blockData.ColorType > 5)
            {
                blockDataList.Remove(blockData);
            }
        }

        GameManager.Instance.MusicControl.AddBlockCount(blockDataList);
        EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_MUSIC_BLOCK_ANIM, blockDataList, false);
    }

    #region 生成拼图相关

    //获取随机颜色
    private int GetRandomColorFromPool()
    {
        var colorPool = Model.ColorPool;
        if (colorPool.Count <= 0)
            return 0;

        return colorPool[0];
    }

    /// <summary>
    /// 获取一个方块
    /// </summary>
    public PuzzleData GetPuzzle(int puzzleId)
    {
        var puzzle = new PuzzleData(puzzleId);
        var colorType = GetRandomColorFromPool();
        puzzle.SetPuzzleData(colorType, 0, 0);
        return puzzle;
    }

    /// <summary>
    /// 是否能触发elo
    /// </summary>
    /// <returns></returns>
    public bool BELO()
    {
        var targetCount = _fightController.Model.MBlockList.Length - Model.LevelBaseData.EloStar;
        var curCount = _fightController.Model.GetIsOccupiedCount();
        return targetCount <= curCount && Model.BELOEffect();
    }

    /// <summary>
    /// 动态拼图获取
    /// </summary>
    /// <returns></returns>
    public int GetRandomPuzzleId()
    {
        var isOccupied = _fightController.Model.GetIsOccupiedCount();
        var easy = Config.GetConfig<Config_GdConstant>().GetConfigById(4).Num; //24
        var dif = Config.GetConfig<Config_GdConstant>().GetConfigById(5).Num; //48
        if (isOccupied < easy)
        {
            var difLevel = Config.GetConfig<Config_LevelBase>().RandomDif(Model.LevelBaseData.Simple);
            return Config.GetConfig<Config_BlockBase>().GetRandomPuzzleIndex(difLevel).Id;
        }

        if (isOccupied > dif)
        {
            var difLevel = Config.GetConfig<Config_LevelBase>().RandomDif(Model.LevelBaseData.Hard);
            return Config.GetConfig<Config_BlockBase>().GetRandomPuzzleIndex(difLevel).Id;
        }

        var dl = Config.GetConfig<Config_LevelBase>().RandomDif(Model.LevelBaseData.Normal);
        return Config.GetConfig<Config_BlockBase>().GetRandomPuzzleIndex(dl).Id;
    }

    /// <summary>
    /// 找到一个可以消除的拼图
    /// </summary>
    /// <returns></returns>
    private PuzzleData _temp;

    public int GetPuzzleCanComplete()
    {
        var blockList = _fightController.Model.MBlockList;
        for (var i = Model.PuzzlePool.Count - 1; i >= 0; i--)
        {
            _temp = new PuzzleData(Model.PuzzlePool[i]);
            var posList = _temp.GetCoordinates();
            var targetPosList = new List<Vector2Int>();
            var rowList = new List<int>(); // 行
            var colList = new List<int>(); // 列

            foreach (var blockData in blockList)
            {
                if (blockData.IsOccupied) //占用的格子不处理
                    continue;

                targetPosList.Clear();
                rowList.Clear();
                colList.Clear();

                var canPlace = true;
                foreach (var pos in posList)
                {
                    var targetPos = blockData.Pos + pos;

                    targetPosList.Add(targetPos);
                    if (!rowList.Contains(targetPos.x))
                        rowList.Add(targetPos.x);
                    if (!colList.Contains(targetPos.y))
                        colList.Add(targetPos.y);

                    var targetBlockData = _fightController.Model.GetBlockDataByPos(targetPos);
                    if (targetBlockData == null || targetBlockData.IsOccupied) //目标格子不存在或者目标格子被占
                    {
                        canPlace = false;
                        break;
                    }
                }

                if (!canPlace) //如果这个格子都放不下 直接下一个         
                    continue;

                foreach (var raw in rowList) //遍历行
                {
                    if (CheckLineBeComplete(blockList.GetRow(raw), targetPosList))
                    {
                        return Model.PuzzlePool[i];
                    }
                }

                foreach (var col in colList) //遍历列
                {
                    if (CheckLineBeComplete(blockList.GetColumn(col), targetPosList))
                    {
                        return Model.PuzzlePool[i];
                    }
                }
            }
        }

        return 1;
    }

    /// <summary>
    /// 检测这些格子是否满足条件
    /// </summary>
    /// <param name="blocksData"></param>
    /// <param name="posList"></param>
    /// <returns></returns>
    private bool CheckLineBeComplete(BlockData[] blocksData, List<Vector2Int> posList)
    {
        foreach (var blockData in blocksData)
        {
            if (blockData.ColorType != 0) // 有颜色
                continue;
            if (blockData.IsOccupied) //被占用
                return false;
            if (!posList.Contains(blockData.Pos)) //需要补全的方块在不在列表里
                return false;
        }

        return true;
    }

    #endregion

    #region 订单

    public void InitCreateOrder()
    {
        FillOpenMonsterSlots();
    }

    /// <summary>
    /// 检查订单
    /// </summary>
    ///  有消除的时候返回 true
    public bool CheckFightOrder()
    {
        return CheckFightOrderAndGetFinished().Count > 0;
    }

    public List<FightOrderData> CheckFightOrderAndGetFinished()
    {
        var posList = new List<int>();
        var finishList = new List<FightOrderData>();
        for (var i = Model.FightOrders.Count - 1; i >= 0; i--) //检查是否有已经完成的订单
        {
            var fightOrder = Model.FightOrders[i];
            if (fightOrder.CheckNeedListFinish() && Model.IsOrderBattleComplete(fightOrder))
            {
                finishList.Add(new FightOrderData(fightOrder));
                Model.FinishOrderList.Add(fightOrder);
                Model.UnregisterOrderMonster(fightOrder);
                Model.FightOrders.RemoveAt(i);
                OrderFinish(fightOrder);
                posList.Add(fightOrder.Index);
            }
        }

        if (posList.Count > 0)
            FillOpenMonsterSlots();

        return finishList;
    }

    private void FillOpenMonsterSlots()
    {
        if (IsLevelTargetFinish())
            return;

        var desiredSlots = Model.GetDesiredActiveMonsterSlots();
        for (var index = 0; index < Model.GetMaxActiveMonsterSlots() && Model.FightOrders.Count < desiredSlots; index++)
        {
            if (Model.OrderList.Count <= 0)
                break;

            if (Model.FightOrders.Exists(orderData => orderData.Index == index))
                continue;

            AddOrderCount(index);
        }
    }

    /// <summary>
    /// 创建订单
    /// </summary>
    private void CreateFightOrder(int index, bool isInit = false)
    {
        var orderData = Model.FightOrders.Find(x => x.Index == index);
        if (orderData != null)
        {
            Model.UnregisterOrderMonster(orderData);
            Model.FightOrders.Remove(orderData);
        }

        var orderId = 0;
        if (Model.OrderList.Count > 1)
        {
            orderId = Model.OrderList[0]; //每次都取第一个
            Model.OrderList.RemoveAt(0);
        }
        else
        {
            orderId = Model.OrderList[0];
        }

        var fightOrderData = new FightOrderData(index, orderId);
        Model.FightOrders.Add(fightOrderData);
        var isBoss = Model.LevelBaseData != null && Model.LevelBaseData.Boss == 1 && fightOrderData.Index == 0;
        Model.RegisterOrderAsMonster(fightOrderData, isBoss);
        var alreadyCreateOrder = Model.FinishOrderList.Count + Model.FightOrders.Count;

        if (Model.ColorUnlockDic.TryGetValue(alreadyCreateOrder, out var colorType))
        {
            Model.ColorItemPool.Add(colorType); //添加一个新物品颜色
            UIManager.Instance.ShowUI("UIFightNewColor", null, colorType);
        }

        if (!isInit)
            CheckOpenChat(fightOrderData);
    }

    /// <summary>
    /// 订单完成
    /// </summary>
    /// <param name="fightOrderData"></param>
    private void OrderFinish(FightOrderData fightOrderData)
    {
        var orderCfg = Config.GetConfig<Config_OrderBlock>().GetConfigById(fightOrderData.ItemId);
        var reward = Util.GetRewardConfig(orderCfg.Reward)[0];
        var score = Config.GetConfig<Config_GdConstant>().GetConfigById(28).Num;
        Model.AddLevelScore(score);
        GameManager.Instance.CurFightControl.Model.AddMoney((int)reward.Number);
        Model.AddOrderDataWithNpc(fightOrderData);
        CheckOpenChat(fightOrderData);
    }

    public bool CheckOpenChat(FightOrderData fightOrderData)
    {
        if (Model.IsEndLess)
            return false;

        if (Model.NpcDic.TryGetValue(fightOrderData.NpcId, out var npcData))
        {
            var hasChat =
                GameManager.Instance.CurFightControl.ChatController.CheckNpcChat(fightOrderData.NpcId, npcData.order);
            if (hasChat)
            {
                int npcId = fightOrderData.NpcId;
                int pos = fightOrderData.Index;
                var data = new FightChatViewData()
                {
                    npcId = npcId,
                    pos = pos,
                    CloseAction = (id, exp) => { ChatEndNewSystem(); }
                };
                UIManager.Instance.ShowUI("UIFightChat", param: data);
            }

            return hasChat;
        }

        return false;
    }

    private void ChatEndNewSystem()
    {
        var lvBase = Model.LevelBaseData;
        var Mechanism = lvBase.Mechanism;
        if (Mechanism != 0)
        {
            UIManager.Instance.ShowUI("UIFightNewSystem", param: Mechanism);
        }
    }

    public void CheckOpenChatEndless(FightOrderData fightOrderData)
    {
        if (Model.NpcDic.TryGetValue(fightOrderData.NpcId, out var npcData))
        {
            var hasChat = GameManager.Instance.CurFightControl.ChatController.CheckNpcChat(fightOrderData.NpcId, npcData.order);
            if (hasChat)
            {
                int npcId = fightOrderData.NpcId;
                int pos = fightOrderData.Index;
                var data = new FightChatViewData()
                {
                    npcId = npcId,
                    pos = pos,
                    CloseAction = (a, b) => { EndlessNewSystem(); }
                };
                UIManager.Instance.ShowUI("UIFightChat", param: data);
            }
        }
    }

    private void EndlessNewSystem()
    {
        int HistoryHighScore = GameManager.Instance.EndlessControl.HistoryHighScore;
        if (HistoryHighScore == 0)
        {
            UIManager.Instance.ShowUI("UIFightNewSystem", param: 999);
        }
    }

    /// <summary>
    /// 增加订单数量
    /// </summary>
    public void AddOrderCount(int index)
    {
        if (Model.OrderList.Count <= 0)
            return;

        if (!Model.FightOrderIndex.Contains(index))
        {
            Model.FightOrderIndex.Add(index);
        }

        if (Model.FightOrders.Exists(orderData => orderData.Index == index))
            return;

        CreateFightOrder(index, index == 0 && Model.FinishOrderList.Count == 0);
        EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_REFRESH_ORDER_ITEM);
    }
    
    /// <summary>
    /// 检查物品订单是否需要
    /// </summary>
    /// <param name="blockId"></param>
    /// <returns></returns>
    public bool CheckItemOrderNeed(int blockId)
    {
        foreach (var fightOrder in Model.FightOrders)
        {
            if (!fightOrder.CheckNeedListFinish())
            {
                if (blockId == fightOrder.NeedBlockId)
                    return true;
            }
        }

        return false;
    }

    #endregion


    //关卡目标是否达成
    public bool IsLevelTargetFinish()
    {
        if (Model.IsEndLess)
            return false;
        
        switch (Model.LevelBaseData.Modle)
        {
            case 1:
                return Model.FinishOrderList.Count >= Model.Target;
            case 2:
                return Model.LevelScore >= Model.Target;
        }

        return false;
    }


    public int GetLevelProgress()
    {
        if (Model.IsEndLess)
        {
            return 100;
            //return 
        }
        switch (Model.LevelBaseData.Modle)
        {
            case 2:
                return (int)Math.Round((float)Model.LevelScore / Model.Target * 100);
            default:
                return (int)Math.Round((float)Model.FinishOrderList.Count / Model.Target * 100);
        }
    }


    public List<ItemConfig> GetFinalReward()
    {
        var rewardList = new List<ItemConfig>();
        //rewardList.Add(new ItemConfig()
        //{
        //    Id = 1,
        //    Number = _fightController.Model.Money,
        //});
        //rewardList.AddRange(Util.GetRewardConfig(Model.LevelBaseData.Reward));
        return rewardList;
    }
}
