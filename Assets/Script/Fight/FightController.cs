using System;
using DG.Tweening;
using Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class FightController : BaseControl
{
    /// <summary>
    /// 战斗结束
    /// </summary>
    public bool IsGameEnd { get; set; }  = false;
    public float ComboTime = 0f;
    private long _startTime;
    

    private FightModel model;
    public FightModel Model
    {
        get
        {
            if (ReferenceEquals(model, null))
            {
                model = new FightModel();
            }

            return model;
        }
    }
    
    protected override void OnInitControl() { }
    protected override void OnCloseControl() { }

    public FightLevelController LevelController; //关卡
    public FightChatController ChatController; //对话

    public IEnumerator PreloadFight(int levelId)
    {
        Model.InitFight(levelId);
        ChatController = new FightChatController();
        LevelController = new FightLevelController(levelId);
        LevelController.InitCreateOrder();
        ApplyInitialTargetColorsToHand();
        
        yield return new WaitForSeconds(0.1f);
        UIManager.Instance.PreLoadUI("UIFightMain");
    }

    public IEnumerator FightStart(int levelId)
    {
        Model.InitFight(levelId);
        ChatController = new FightChatController();
        LevelController = new FightLevelController(levelId);
        LevelController.InitCreateOrder();
        ApplyInitialTargetColorsToHand();

        _startTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        yield return new WaitForSeconds(0.1f);
        UIManager.Instance.ShowUI("UIFightMain");
        fightStart = true;
    }

    public IEnumerator FightEndlessStart()
    {
        var endlessId = GameManager.Instance.EndlessControl.GetEndlessLevelId();
        var endlessConf = Config.GetConfig<Config_LevelWujingModle>().GetConfigById(endlessId);
        var levelId = GameManager.Instance.EndlessControl.RandomGetLevelId();
        Debug.Log($"无尽模式开始 endlessId:{endlessId}   levelId:{levelId}");
        
        Model.InitFight(levelId);
        ChatController = new FightChatController();
        LevelController = new FightLevelController(levelId);
        LevelController.InitEndlessOrder(endlessConf);
        ApplyInitialTargetColorsToHand();
        AudioManagerNew.Instance.FadeStopMusic();
        GameManager.Instance.MusicControl.PlayPickMainBGM();
        
        _startTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        yield return new WaitForSeconds(0.1f);
        UIManager.Instance.ShowUI("UIFightMain");
        fightStart = true;
    }


    public bool fightStart { get; set; } = false;
    public void FixedUpdate()
    {
        if(!fightStart || IsGameEnd)
            return;
        if (!ChatController.Model.bInChat)
        {
            UpdateComboTime(Time.deltaTime);
        }
    }

    /// <summary>
    /// 拼图放置下时
    /// </summary>
    public IEnumerator OnPuzzlePutDown(UIFightMain ui, int puzzleIndex, Dictionary<Vector2Int, int> posColorList)
    {
        foreach (var posColor in posColorList)
        {
            Model.SetBlockDataByPos(posColor.Key, true, posColor.Value);
        }
        PlatformManager.Instance.ShortVibration(1);
        
        Model.RandomPuzzleList[puzzleIndex].bUsed = true;
        ui.RefreshAllBlock(); //更新格子显示
        ui.RefreshPuzzleOnShow();
        //yield return new WaitForSeconds(0.2f); //放置特效等待时间
        var completedLineCount = CheckCompletedLines(Model.MBlockList); //检查是否可以消除
        if (completedLineCount.triggerCounts.Count > 0) //有消除
        {
            var triggerEffect = TriggerFightEffect.instance;
            triggerEffect.InitFightEffects(completedLineCount.triggerCounts,ui);
            var clearBlocks = triggerEffect.CheckComplete();//获得可消除位置
            Model.AddCombo(); //记录消除数据
            ComboTime = Model.GetComboTime(); //记录连消时间
            ui.PlayClearEffect(clearBlocks);                //播放消除特效
            CalculateComboResult(ui, completedLineCount);   //处理多消 连消
            yield return new WaitForSeconds(0.3f);
            
            LevelController.TotalClearBlock(clearBlocks,ui); //订单数据结算
            GrantTargetColorRewards(completedLineCount);
            triggerEffect.Complete();      //更改格子数据
            ui.RefreshAllBlock();          //刷新所有方块
            triggerEffect.CompleteEnd();   //更改格子数据
            triggerEffect.Destroy();       //移除触发信息
            while (!triggerEffect.ChekeOver())
            {
                yield return new WaitForSeconds(0.05f);
            }
        }
        else
        {
            LevelController.Model.MonsterBattleState.MarkNoProgressStep();
        }
        
        CheckRemoveBox(ui);
        if (!LevelController.IsLevelTargetFinish())
        {
            ApplyBossSpirits();
            ApplyMonsterPressure();
            ui.RefreshAllBlock();
        }
        RefreshAllPuzzleItem();
        
        //如果有对话要执行 就卡住不结算
        while (ChatController.Model.bInChat)
        {
            yield return null;
        }
    
        CheckGameEnd(ui);
    }

    
    private void CalculateComboResult(UIFightMain ui, CompletedLines clearData)
    {
        var onceClearCount = clearData.completedRows.Count + clearData.completedCols.Count;  //单次消除了几行
        var curCombo = Model.CurCombo;
        
        var clearConfig = Config.GetConfig<Config_FighteffectBase>().GetEffectConfig(1, onceClearCount);  //多消
        var comboConfig = Config.GetConfig<Config_FighteffectBase>().GetEffectConfig(2, curCombo);   //连消
        var comboScore = comboConfig?.Score ?? 0;
        LevelController.Model.AddLevelScore(clearConfig.Score + comboScore);
        ui.PlayComboEffect(clearConfig, comboConfig); //播放连击特效
    }

    private void GrantTargetColorRewards(CompletedLines clearData)
    {
        var rewardCount = 0;
        var onceClearCount = clearData.completedRows.Count + clearData.completedCols.Count;
        var clearConfig = Config.GetConfig<Config_FighteffectBase>().GetEffectConfig(1, onceClearCount);
        if (clearConfig != null && clearConfig.Num > 1)
            rewardCount++;

        var comboConfig = Config.GetConfig<Config_FighteffectBase>().GetEffectConfig(2, Model.CurCombo);
        if (comboConfig != null && comboConfig.Num > 1)
            rewardCount++;

        if (rewardCount <= 0)
            return;

        if (LevelController.Model.QueueTargetColorRewards(rewardCount, GetAvailableHandColorCounts()) <= 0)
            return;

        ApplyPendingRewardColorsToAvailableHand();
    }

    private Dictionary<int, int> GetAvailableHandColorCounts()
    {
        var colorCounts = new Dictionary<int, int>();
        foreach (var puzzleData in Model.RandomPuzzleList)
        {
            if (puzzleData.bUsed)
                continue;

            foreach (var colorType in puzzleData.PosColorList.Values)
            {
                if (colorType <= 0)
                    continue;

                if (colorCounts.ContainsKey(colorType))
                {
                    colorCounts[colorType]++;
                }
                else
                {
                    colorCounts.Add(colorType, 1);
                }
            }
        }

        return colorCounts;
    }

    private void ApplyInitialTargetColorsToHand()
    {
        var neededColors = LevelController.Model.MonsterBattleState.GetActiveNeededColors();
        if (neededColors.Count <= 0)
            return;

        var colorIndex = 0;
        foreach (var puzzleData in Model.RandomPuzzleList)
        {
            if (puzzleData.bUsed)
                continue;

            puzzleData.DyeFirstBlocks(neededColors[colorIndex], 1);
            colorIndex++;
            if (colorIndex >= neededColors.Count)
                break;
        }
    }
    
    public void RefreshAllPuzzleItem()
    {
        var allUsed = CheckToCreateNewPuzzle();
        if (allUsed)
        {
            CreateNewRandomPuzzleList();
        }
        EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_REFRESH_PUZZLE_ITEM, allUsed, false);
    }
 
    /// <summary>
    /// 生成随机拼图
    /// </summary>
    /// <param name="difficulty"> 生成组(如果不指定就根据算法随机)</param>
    public void CreateNewRandomPuzzleList(int difficulty = 0)
    {
        Model.RandomPuzzleList.Clear();
        var eloCount = 1;  //规定elo只能触发一次
        var guaranteeColor = LevelController.Model.MonsterBattleState.NeedTargetProgressGuarantee
            ? LevelController.Model.MonsterBattleState.GetMostNeededColor()
            : 0;
        var guaranteeUsed = guaranteeColor <= 0;
        
        for (var i = 0; i < 3; i++)
        {
            PuzzleData puzzleData;
            if (difficulty != 0)
            {
                var puzzleCfg = Config.GetConfig<Config_BlockBase>().GetRandomPuzzleIndex(difficulty);
                puzzleData = LevelController.GetPuzzle(puzzleCfg.Id);
            }
            else if (eloCount > 0 && LevelController.BELO()) //触发elo
            {
                eloCount--;
                var puzzleId = LevelController.GetPuzzleCanComplete();
                Debug.LogError($"触发elo    puzzleId:{puzzleId}");
                puzzleData = LevelController.GetPuzzle(puzzleId);
            }
            else
            {
                var puzzleId = LevelController.GetRandomPuzzleId();
                puzzleData = LevelController.GetPuzzle(puzzleId);
            }

            var rewardColor = LevelController.Model.ConsumePendingRewardColor();
            if (rewardColor > 0)
            {
                puzzleData.DyeFirstBlocks(rewardColor, 1);
            }
            else if (!guaranteeUsed)
            {
                puzzleData.DyeFirstBlocks(guaranteeColor, 1);
                guaranteeUsed = true;
            }

            Model.RandomPuzzleList.Add(puzzleData);
        }
    }

    private bool ApplyPendingRewardColorsToAvailableHand()
    {
        var changed = false;
        foreach (var puzzleData in Model.RandomPuzzleList)
        {
            if (puzzleData.bUsed || !LevelController.Model.HasPendingRewardColor())
                continue;

            var rewardColor = LevelController.Model.ConsumePendingRewardColor();
            if (rewardColor <= 0)
                continue;

            puzzleData.DyeFirstBlocks(rewardColor, 1);
            changed = true;
        }

        return changed;
    }


    /// <summary>
    /// 格子是否被占用
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool CheckPosDataIsOcc(Vector2Int pos)
    {
        var data = model.GetBlockDataByPos(pos);
        if (data == null || data.IsOccupied)
        {
            return true;
        }
  
        return false;
    }

    /// <summary>
    /// 检查是否可以创建新的拼图
    /// </summary>
    public bool CheckToCreateNewPuzzle()
    {
        var allUsed = true;
        foreach (var puzzleData in Model.RandomPuzzleList)
        {
            allUsed = allUsed && puzzleData.bUsed;
        }

        return allUsed;
    }
    
    private void UpdateComboTime(float time)
    {
        ComboTime -= time;
        if (ComboTime <= 0)
        {
            Model.ClearCombo();
        }
        EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_UPDATE_COMBO_TIME);
    }
    
    #region 使用道具

    /// <summary>
    /// 使用摇酒瓶
    /// </summary>
    public void UseShaker()
    {
        Model.RandomPuzzleList.Clear(); //清空现有的拼图
        CreateNewRandomPuzzleList(1); //生成一些新的
        EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_REFRESH_MAIN_All_ITEM);
    }

    /// <summary>
    /// 使用香槟
    /// </summary>
    /// <param name="idx"></param>
    /// <param name="ui"></param>
    /// <returns></returns>
    public IEnumerator UseRemoveColumn(int idx, UIFightMain ui)
    {
        List<Vector2Int> blocksToClear = new();
        Dictionary<Vector2Int, int> triggerCounts = new();
        for (int i = 0; i < FightModel.GRID_HEIGHT; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                int x = (idx - 1) * 2 + j;
                int y = i;
                var pos = new Vector2Int(y, x);
                var data = Model.GetBlockDataByPos(pos);
                if (data.IsOccupied && data.ColorType != 0 && (data.Effect == EffectType.None || data.Effect == EffectType.LockOne || data.Effect == EffectType.LockTwice || data.Effect == EffectType.LockThrice))
                {
                    var trigerPos = new Vector2Int(y, x);
                    blocksToClear.Add(trigerPos);
                    triggerCounts.Add(trigerPos, 1);
                }

            }
        }
        var triggerEffect = TriggerFightEffect.instance;
        triggerEffect.InitFightEffects(triggerCounts, ui);
        var clearBlocks = triggerEffect.CheckComplete();//获得可消除位置
        ui.PlayClearEffect(blocksToClear);        //播放消除特效
        yield return new WaitForSeconds(0.5f);
        triggerEffect.Complete();      //更改格子数据
        triggerEffect.CompleteEnd();   //更改格子数据
        triggerEffect.Destroy();       //移除触发信息
        while (!triggerEffect.ChekeOver())
        {
            yield return new WaitForSeconds(0.05f);
        }
        ui.RefreshAllBlock(); //刷新所有方块
        RefreshAllPuzzleItem();
        CheckGameEnd(ui);
    }

    public IEnumerator UseRemoveItem(Vector2Int pos, UIFightMain ui)
    {
        List<Vector2Int> blocksToClear = new();
        Dictionary<Vector2Int, int> triggerCounts = new();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                var removePos = pos + new Vector2Int(i, j);
                if (removePos.x < 0|| removePos.x >= FightModel.GRID_WIDTH || removePos.y < 0 || removePos.y >= FightModel.GRID_HEIGHT)
                    continue;
                var data = Model.GetBlockDataByPos(removePos);
                if (data.IsOccupied && data.ColorType != 0 && (data.Effect == EffectType.None || data.Effect == EffectType.LockOne || data.Effect == EffectType.LockTwice || data.Effect == EffectType.LockThrice))
                {
                    blocksToClear.Add(removePos);
                    triggerCounts.Add(removePos, 1);
                }
            }
        }
        var triggerEffect = TriggerFightEffect.instance;
        triggerEffect.InitFightEffects(triggerCounts, ui);
        var clearBlocks = triggerEffect.CheckComplete();//获得可消除位置
        ui.PlayClearEffect(blocksToClear); //播放消除特效
        yield return new WaitForSeconds(0.5f);
        triggerEffect.Complete();      //更改格子数据
        triggerEffect.CompleteEnd();   //更改格子数据
        triggerEffect.Destroy();       //移除触发信息
        while (!triggerEffect.ChekeOver())
        {
            yield return new WaitForSeconds(0.05f);
        }
        //LevelController.TotalClearBlock(blocksToClear, ui); //订单数据结算
        ui.RefreshAllBlock(); //刷新所有方块
        RefreshAllPuzzleItem();
        CheckGameEnd(ui);
    }

    private bool CheckLockEffect(BlockData blockData)
    {
        if (blockData.Effect == EffectType.LockOne || blockData.Effect == EffectType.LockTwice || blockData.Effect == EffectType.LockThrice)
            return true;
        return false;
    }



    #endregion

    #region 消除部分

    /// <summary>
    /// 方块结算
    /// </summary>
    public void BlockCompleteData(CompletedLines clearData)
    {
        foreach (var clearTrigger in clearData.triggerCounts)
        {
            var blockData = Model.GetBlockDataByPos(clearTrigger.Key);
            for (int i = 0; i < clearTrigger.Value; i++)
            {
                if (blockData.TriggerEffect2Destroy())
                {
                    blockData.Reset();
                }
            }
        }
    }


    private BlockData[,] _dragTempBlockData = new BlockData[8, 8]; //拖拽临时格子数据

    /// <summary>
    /// 消除预览
    /// </summary>
    /// <param name="puzzleData"></param>
    /// <returns></returns>
    public CompletedLines GetDragTempBlockCompletedData(PuzzleTargetData puzzleData)
    {
        _dragTempBlockData = Util.Copy2DArray(Model.MBlockList, b => new BlockData(b));
        foreach (var posColor in puzzleData.RayPosColorList)
        {
            var pos = posColor.Key;
            var data = _dragTempBlockData[pos.x, pos.y];
            data.SetIsOccupied(true);
            data.SetColorType(posColor.Value);
        }

        return CheckCompletedLines(_dragTempBlockData);
    }

    public BlockData GetDragTempData(Vector2Int pos)
    {
        return _dragTempBlockData[pos.x, pos.y];
    }

    /// <summary>
    /// 检查是否可以消除
    /// </summary>
    /// <returns></returns>
    private CompletedLines CheckCompletedLines(BlockData[,] blockList)
    {
        var completedRows = new List<int>();
        var completedCols = new List<int>();
        var triggerCounts = new Dictionary<Vector2Int, int>(); // 记录每个方块的触发次数

        // 检查哪些行被完全占满
        for (var y = 0; y < FightModel.GRID_HEIGHT; y++)
        {
            bool rowComplete = true;
            for (var x = 0; x < FightModel.GRID_WIDTH; x++)
            {
                if (!blockList[x, y].IsOccupied || blockList[x,y].ColorType == 0)
                {
                    rowComplete = false;
                    break;
                }
            }

            if (rowComplete)
            {
                completedRows.Add(y);
                // 记录这一行的所有方块
                for (var x = 0; x < FightModel.GRID_WIDTH; x++)
                {
                    var pos = new Vector2Int(x, y);
                    triggerCounts.TryAdd(pos, 0);
                    triggerCounts[pos]++; // 增加触发次数
                }
            }
        }

        // 检查哪些列被完全占满
        for (int x = 0; x < FightModel.GRID_WIDTH; x++)
        {
            var colComplete = true;
            for (var y = 0; y < FightModel.GRID_HEIGHT; y++)
            {
                if (!blockList[x, y].IsOccupied || blockList[x, y].ColorType == 0)
                {
                    colComplete = false;
                    break;
                }
            }

            if (colComplete)
            {
                completedCols.Add(x);
                // 记录这一列的所有方块
                for (var y = 0; y < FightModel.GRID_HEIGHT; y++)
                {
                    var pos = new Vector2Int(x, y);
                    triggerCounts.TryAdd(pos, 0);
                    triggerCounts[pos]++; // 增加触发次数
                }
            }
        }
        
        return new CompletedLines()
        {
            completedRows = completedRows,
            completedCols = completedCols,
            triggerCounts = triggerCounts
        };
    }
    
    /// <summary>
    /// 获取需要清除的方块
    /// </summary>
    /// <param name="completedLines"></param>
    /// <returns></returns>
    public List<Vector2Int> GetClearBlocks(CompletedLines completedLines)
    {
        var blocksToClear = new List<Vector2Int>();
        // 处理所有需要触发的方块
        foreach (var kvp in completedLines.triggerCounts)
        {
            var pos = kvp.Key;
            var count = kvp.Value;
            
            for (int i = 0; i < count; i++)
            {
                blocksToClear.Add(pos);
            }
        }
        return blocksToClear;
    }

    #endregion

    #region 机制

    private void ApplyMonsterPressure()
    {
        var activeMonsterCount = LevelController.Model.MonsterBattleState.ActiveMonsterCount;
        if (activeMonsterCount <= 0)
            return;

        var currentPressureCount = 0;
        var candidates = new List<BlockData>();
        foreach (var blockData in Model.MBlockList)
        {
            if (CheckLockEffect(blockData))
            {
                currentPressureCount++;
                continue;
            }

            if (!blockData.IsOccupied
                || blockData.ColorType <= 0
                || blockData.ColorType > 5
                || blockData.Effect != EffectType.None
                || blockData.HasAttachedSpirit
                || IsItemBlok(blockData))
                continue;

            candidates.Add(blockData);
        }

        var pressurePerTurn = activeMonsterCount * LevelController.Model.GetEnemyPressureBlocksPerMonster();
        var pressureCount = Math.Min(
            pressurePerTurn,
            Math.Max(0, LevelController.Model.GetMaxEnemyPressureBlocks() - currentPressureCount));
        pressureCount = Math.Min(pressureCount, candidates.Count);
        for (var i = 0; i < pressureCount; i++)
        {
            var index = UnityEngine.Random.Range(0, candidates.Count);
            candidates[index].Effect = EffectType.LockTwice;
            candidates.RemoveAt(index);
        }
    }

    private void ApplyBossSpirits()
    {
        var monsterIds = LevelController.Model.MonsterBattleState.GetMonsterIdsNeedingSpirit();
        if (monsterIds.Count <= 0)
            return;

        foreach (var monsterId in monsterIds)
        {
            if (HasAttachedSpirit(monsterId))
                continue;

            var target = GetRandomSpiritTargetBlock();
            if (target == null)
                return;

            target.AttachSpirit(monsterId);
        }
    }

    private bool HasAttachedSpirit(int monsterId)
    {
        foreach (var blockData in Model.MBlockList)
        {
            if (blockData.AttachedSpiritId == monsterId)
                return true;
        }

        return false;
    }

    private BlockData GetRandomSpiritTargetBlock()
    {
        var preferredCandidates = new List<BlockData>();
        var fallbackCandidates = new List<BlockData>();
        foreach (var blockData in Model.MBlockList)
        {
            if (!blockData.IsOccupied
                || blockData.ColorType <= 0
                || blockData.Effect != EffectType.None
                || blockData.HasAttachedSpirit)
                continue;

            fallbackCandidates.Add(blockData);
            if (blockData.ColorType <= 5 && !IsItemBlok(blockData))
                preferredCandidates.Add(blockData);
        }

        var candidates = preferredCandidates.Count > 0 ? preferredCandidates : fallbackCandidates;
        if (candidates.Count <= 0)
            return null;

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }
    
    private void CheckRemoveBox(UIFightMain ui)
    {
        bool isRemove = false;
        foreach (var data in Model.boxDatas)
        {
            data.wait--;
            var blockData = Model.GetBlockDataByPos(data.pos);
            var item = ui.GetBlockItemByPos(data.pos);
            if (data.wait <= 0)
            {
                blockData.Reset();
                isRemove = true;
                ui.SetBoxNumColor(item);
                ui.RemoveBox(item);
            }
            else
            {
                ui.SetBoxNumColor(item);
                item.RefreshItem();
            }
        }
        if (isRemove)
        {
            Model.boxDatas.Clear();
            AudioManagerNew.Instance.PlayAudio("fight_item_treasureboxdisappear.ogg");
        }
    }

    public bool IsItemBlok(BlockData blockData)
    {
        return LevelController.Model.ColorItemPool.Contains(blockData.ColorType);
    }

    public bool IsMaskBlock(Vector2Int pos)
    {
        var masks = Model.MasksData;
        foreach (var mask in masks)
        {
            if (mask.PosX >= pos.x && mask.PosX + mask.Length <= pos.x && mask.PosY >= pos.y && mask.PosY + mask.Width <= pos.y)
            {
                return true;
            }
        }
        return false;
    }

    #endregion

    #region 判断游戏结束
    
    public void CheckGameEnd(UIFightMain ui = null)
    {
        if (LevelController.IsLevelTargetFinish())
        {
            ui.ShowOrHideTarget(true);
            ui.PlayTargetAudio();
            ui.ShowWait(3f,action:() =>
            {
                ui.ShowOrHideTarget(false);
                UIManager.Instance.ShowUI("UIFightEnd", null, true);
            });
        }
        else
        {
            foreach (var puzzleData in Model.RandomPuzzleList)
            {
                if (!puzzleData.bUsed)
                {
                    if (CheckPuzzleCanPut(puzzleData)) //还有可以放置的拼图
                    {
                        return;
                    }
                }
            }
            Model.ClearCombo();

            GameManager.Instance.LogManager.Log_GameDead(LevelController.Model.LevelId,
                LevelController.Model.IsEndLess);
            ui.ShowWait(0.5f,action: () =>
            {
                if (LevelController.Model.IsEndLess
                    && LevelController.Model.ReviveTimes >= LevelController.Model.GetMaxReviveTimes())
                {
                    EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_FIGHT_END);
                }
                else
                {
                    UIManager.Instance.ShowUI("UIFailContinue");
                }
            });
        }
    }
    
    /// <summary>
    /// 检查现有棋盘是否可以放下剩余的拼图
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool CheckPuzzleCanPut(PuzzleData data)
    {
        var puzzlePosList = data.GetCoordinates(); //获取相对坐标
        string Occupied = "Occupied:";
        foreach (var blockData in Model.MBlockList)
        {
            if (blockData.IsOccupied) //占用的格子不处理
            {
                continue;
            }
            Occupied += $"{blockData.Pos.x},{blockData.Pos.y},{blockData.IsOccupied}";
            var canPut = true;
            foreach (var pos in puzzlePosList)
            {
                var targetPos = blockData.Pos + pos;
                var targetBlockData = Model.GetBlockDataByPos(targetPos);
                if (targetBlockData == null || targetBlockData.IsOccupied) //目标格子不存在或者目标格子被占
                {
                    if (targetBlockData!= null)
                    {
                        Occupied += $"targetOccupied:{targetBlockData.Pos.x},{targetBlockData.Pos.y}##";
                    }
                    canPut = false;
                    break;
                }
            }

            if (!canPut)
                continue;
            return true;
        }
        return false;
    }

    #endregion

    //复活
    public void GameContinue()
    {
        IsGameEnd = false;
        LevelController.Model.ReviveTimes++;   //统计复活次数
        Model.RandomPuzzleList.Clear();        //清空现有的拼图
        CreateNewRandomPuzzleList(1);  //生成一些新的
        CheckGameEnd();

        GameManager.Instance.LogManager.Log_GameRevive(LevelController.Model.LevelId, LevelController.Model.ReviveTimes, LevelController.Model.IsEndLess);
        EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_REFRESH_MAIN_All_ITEM);
        EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_GAME_CONTINUE);
    }
    
    //游戏结束
    public void FightEnd(bool isWin, bool isEndless = false)
    {
        var levelModel = LevelController.Model;
        GameManager.Instance.LogManager.Log_GameFinish(levelModel.LevelId, levelModel.LevelScore, isWin, isEndless);
        if (isWin)
        {
            var infoList = levelModel.NpcDic.Values.ToList();
            ChatController.SetPrefsData();
            GameManager.Instance.PlayerControl.EndFight(levelModel.LevelId,
                infoList, LevelController.GetFinalReward(),isEndless);
        }
        
        PlayerDataManager.instance.StaminaSave();
#if WEIXINMINIGAME && !UNITY_EDITOR
        var dungeonId = isEndless ? 2 : 1;
        var dungeonName = isEndless ? "无尽挑战" : "普通关卡";
        var oper = isWin ? 2 : 3;
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var gameTime = now - _startTime;
        var progress = LevelController.GetLevelProgress();
        
        PlatformManager.Instance.GetPlatformAdapter<WeChatAdapter>().ReportCustomEvent("dungeon", new Dictionary<string, object>()
        {
            { "dungeon_Id", dungeonId},
            { "dungeon_name", dungeonName },
            { "check_id", LevelController.Model.LevelId },
            { "check_name", $"第{LevelController.Model.LevelId}关" },
            { "oper", oper },
            { "start_time", _startTime },
            { "game_time", gameTime },
            { "map_id", 0 },
            { "progress", progress },
        });
#endif
        
    }

    public void ClearData()
    {
        _dragTempBlockData = null;
        model = null;
        ChatController = null;
        
        LevelController?.Model.Clear();
        LevelController = null;
        IsGameEnd = false;
        fightStart = false;
    }
}

public struct CompletedLines
{
    public List<int> completedRows; // 被占满的行索引
    public List<int> completedCols; // 被占满的列索引
    public Dictionary<Vector2Int, int> triggerCounts; // 记录每个方块的触发次数
}
