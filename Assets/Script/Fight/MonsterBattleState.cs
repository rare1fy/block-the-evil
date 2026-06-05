using System;
using System.Collections.Generic;
using System.Linq;

public enum StageRequirementType
{
    Color = 1,
    Spirit = 2,
}

public class StageRequirement
{
    public StageRequirementType RequirementType { get; private set; }
    public int NeedColor { get; private set; }
    public int NeedCount { get; private set; }
    public int RemainingCount { get; private set; }
    public float Timer { get; private set; }

    public bool IsComplete => RemainingCount <= 0;

    public StageRequirement(StageRequirementType requirementType, int needColor, int needCount, float timer = 0f)
    {
        RequirementType = requirementType;
        NeedColor = needColor;
        NeedCount = Math.Max(0, needCount);
        RemainingCount = NeedCount;
        Timer = timer;
    }

    public bool TryConsumeColor(int colorType)
    {
        if (RequirementType != StageRequirementType.Color || IsComplete || colorType != NeedColor)
            return false;

        RemainingCount--;
        return true;
    }

    public bool TryConsumeSpirit()
    {
        if (RequirementType != StageRequirementType.Spirit || IsComplete)
            return false;

        RemainingCount--;
        return true;
    }
}

public class MonsterBattleEntry
{
    public int MonsterId { get; private set; }
    public bool IsBoss { get; private set; }
    public int EnterOrder { get; private set; }
    public int SlotIndex { get; private set; }
    public List<StageRequirement> Stages { get; private set; }
    public int CurrentStageIndex { get; private set; }

    public bool IsCaptured => CurrentStageIndex >= Stages.Count;
    public StageRequirement CurrentStage => IsCaptured ? null : Stages[CurrentStageIndex];

    public MonsterBattleEntry(int monsterId, bool isBoss, int enterOrder, int slotIndex, List<StageRequirement> stages)
    {
        MonsterId = monsterId;
        IsBoss = isBoss;
        EnterOrder = enterOrder;
        SlotIndex = slotIndex;
        Stages = stages ?? new List<StageRequirement>();
        CurrentStageIndex = 0;
        AdvanceCompletedStages();
    }

    public bool TryConsumeColor(int colorType)
    {
        var stage = CurrentStage;
        if (stage == null || !stage.TryConsumeColor(colorType))
            return false;

        AdvanceCompletedStages();
        return true;
    }

    public bool TryConsumeSpirit()
    {
        var stage = CurrentStage;
        if (stage == null || !stage.TryConsumeSpirit())
            return false;

        AdvanceCompletedStages();
        return true;
    }

    private void AdvanceCompletedStages()
    {
        while (CurrentStageIndex < Stages.Count && Stages[CurrentStageIndex].IsComplete)
        {
            CurrentStageIndex++;
        }
    }
}

public class MonsterColorConsumeResult
{
    public Dictionary<int, int> ConsumedByMonsterId { get; private set; } = new Dictionary<int, int>();
    public Dictionary<int, int> OverflowByColor { get; private set; } = new Dictionary<int, int>();
    public List<int> CapturedMonsterIds { get; private set; } = new List<int>();

    public void AddConsumed(int monsterId)
    {
        if (ConsumedByMonsterId.ContainsKey(monsterId))
        {
            ConsumedByMonsterId[monsterId]++;
        }
        else
        {
            ConsumedByMonsterId.Add(monsterId, 1);
        }
    }

    public void AddOverflow(int colorType)
    {
        if (OverflowByColor.ContainsKey(colorType))
        {
            OverflowByColor[colorType]++;
        }
        else
        {
            OverflowByColor.Add(colorType, 1);
        }
    }
}

public class MonsterBattleState
{
    public const int DefaultNoProgressGuaranteeSteps = 5;

    private readonly List<MonsterBattleEntry> _activeMonsters = new List<MonsterBattleEntry>();
    private int _nextEnterOrder;
    private int _stepsSinceMonsterProgress;

    public IReadOnlyList<MonsterBattleEntry> ActiveMonsters => _activeMonsters;
    public int ActiveMonsterCount => _activeMonsters.Count;
    public int StepsSinceMonsterProgress => _stepsSinceMonsterProgress;
    public bool NeedTargetProgressGuarantee => _stepsSinceMonsterProgress >= DefaultNoProgressGuaranteeSteps;

    public MonsterBattleEntry AddMonster(int monsterId, bool isBoss, int slotIndex, List<StageRequirement> stages)
    {
        var entry = new MonsterBattleEntry(monsterId, isBoss, _nextEnterOrder++, slotIndex, stages);
        if (!entry.IsCaptured)
            _activeMonsters.Add(entry);
        return entry;
    }

    public void RemoveMonster(int monsterId)
    {
        _activeMonsters.RemoveAll(monster => monster.MonsterId == monsterId);
    }

    public bool HasMonster(int monsterId)
    {
        return _activeMonsters.Any(monster => monster.MonsterId == monsterId);
    }

    public MonsterColorConsumeResult ConsumeClearedColors(IEnumerable<int> clearedColorTypes)
    {
        var result = new MonsterColorConsumeResult();
        var hasProgress = false;

        foreach (var colorType in clearedColorTypes)
        {
            var target = FindOldestMonsterNeedingColor(colorType);
            if (target == null)
            {
                result.AddOverflow(colorType);
                continue;
            }

            target.TryConsumeColor(colorType);
            result.AddConsumed(target.MonsterId);
            hasProgress = true;

            if (target.IsCaptured)
            {
                result.CapturedMonsterIds.Add(target.MonsterId);
                _activeMonsters.Remove(target);
            }
        }

        if (hasProgress)
            _stepsSinceMonsterProgress = 0;
        else
            _stepsSinceMonsterProgress++;

        return result;
    }

    public bool ConsumeSpirit(int monsterId)
    {
        var target = _activeMonsters.FirstOrDefault(monster => monster.MonsterId == monsterId);
        if (target == null || !target.TryConsumeSpirit())
            return false;

        _stepsSinceMonsterProgress = 0;
        if (target.IsCaptured)
            _activeMonsters.Remove(target);

        return true;
    }

    public int GetMostNeededColor(IDictionary<int, int> availableColorCounts = null)
    {
        var bestColor = 0;
        var bestScore = -1f;
        var bestEnterOrder = int.MaxValue;

        foreach (var monster in _activeMonsters.OrderBy(monster => monster.EnterOrder).ThenBy(monster => monster.SlotIndex))
        {
            var stage = monster.CurrentStage;
            if (stage == null || stage.RequirementType != StageRequirementType.Color || stage.IsComplete)
                continue;

            var available = 0;
            if (availableColorCounts != null)
                availableColorCounts.TryGetValue(stage.NeedColor, out available);
            var score = stage.RemainingCount / (float)Math.Max(1, available);

            if (score > bestScore || (Math.Abs(score - bestScore) < 0.001f && monster.EnterOrder < bestEnterOrder))
            {
                bestScore = score;
                bestColor = stage.NeedColor;
                bestEnterOrder = monster.EnterOrder;
            }
        }

        return bestColor;
    }

    public List<int> GetActiveNeededColors()
    {
        var colors = new List<int>();
        foreach (var monster in _activeMonsters.OrderBy(monster => monster.EnterOrder).ThenBy(monster => monster.SlotIndex))
        {
            var stage = monster.CurrentStage;
            if (stage == null || stage.RequirementType != StageRequirementType.Color || stage.IsComplete)
                continue;

            if (!colors.Contains(stage.NeedColor))
                colors.Add(stage.NeedColor);
        }

        return colors;
    }

    public List<int> GetMonsterIdsNeedingSpirit()
    {
        var monsterIds = new List<int>();
        foreach (var monster in _activeMonsters.OrderBy(monster => monster.EnterOrder).ThenBy(monster => monster.SlotIndex))
        {
            var stage = monster.CurrentStage;
            if (stage == null || stage.RequirementType != StageRequirementType.Spirit || stage.IsComplete)
                continue;

            monsterIds.Add(monster.MonsterId);
        }

        return monsterIds;
    }

    public void MarkNoProgressStep()
    {
        _stepsSinceMonsterProgress++;
    }

    private MonsterBattleEntry FindOldestMonsterNeedingColor(int colorType)
    {
        return _activeMonsters
            .OrderBy(monster => monster.EnterOrder)
            .ThenBy(monster => monster.SlotIndex)
            .FirstOrDefault(monster =>
            {
                var stage = monster.CurrentStage;
                return stage != null
                       && stage.RequirementType == StageRequirementType.Color
                       && !stage.IsComplete
                       && stage.NeedColor == colorType;
            });
    }
}
