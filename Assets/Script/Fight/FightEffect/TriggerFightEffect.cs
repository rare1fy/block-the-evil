using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TriggerFightEffect : Singleton<TriggerFightEffect>
{
    public List<FightEffect> fightEffects = new List<FightEffect>();
    public Dictionary<Vector2Int, int> triggerCounts;

    /// <summary>
    /// 创建效果对象
    /// </summary>
    /// <param name="blockDatas"></param>
    /// <param name="completedLines"></param>
    public void InitFightEffects(Dictionary<Vector2Int, int> triggerCounts, UIFightMain uiFightMain)
    {
        fightEffects.Clear();
        this.triggerCounts = triggerCounts;
        foreach(var pos in triggerCounts.Keys)
        {
            BlockData blockData = GameManager.Instance.CurFightControl.Model.GetBlockDataByPos(pos);
            fightEffects.Add(CreateFightEffect(blockData, uiFightMain));
        }
    }

    private FightEffect CreateFightEffect(BlockData blockData, UIFightMain uiFightMain)
    {
        switch (blockData.Effect)
        {
            case EffectType.None:
                return new FightEffect_None(blockData, uiFightMain);
            case EffectType.LockOne:
            case EffectType.LockTwice:
            case EffectType.LockThrice:
                return new FightEffect_Lock(blockData, uiFightMain);
            case EffectType.CreateIce:
                return new FightEffect_CreateIce(blockData, uiFightMain);
            case EffectType.CreateItem:
                return new FightEffect_CreateItem(blockData, uiFightMain);
            case EffectType.Bomb:
                return new FightEffect_Bomb(blockData, uiFightMain);
            case EffectType.Box:
                return new FightEffect_Box(blockData, uiFightMain);
        }
        return new FightEffect_None(blockData, uiFightMain);
    }

    /// <summary>
    /// 检测消除
    /// </summary>
    public List<Vector2Int> CheckComplete()
    {
        var ret = new List<Vector2Int>();
        foreach (var effect in fightEffects)
        {
            if(triggerCounts.TryGetValue(effect.blockData.Pos, out int count))
            {
                if(effect.CheckComplete(count))
                {
                    ret.Add(effect.blockData.Pos);
                }
            }
        }
        return ret;
    }

    /// <summary>
    /// 消除
    /// </summary>
    public void Complete()
    {
        for (int i = fightEffects.Count - 1; i >= 0; i--)
        {
            var effect = fightEffects[i];
            if (triggerCounts.TryGetValue(effect.blockData.Pos, out int count))
            {
                effect.Complete(count);
            }
        }
    }

    public void CompleteEnd()
    {

        for (int i = fightEffects.Count - 1; i >= 0; i--)
        {
            var effect = fightEffects[i];
            if (triggerCounts.TryGetValue(effect.blockData.Pos, out int count))
            {
                effect.CompleteEnd();
            }
        }
    }


    /// <summary>
    /// 消除结束后
    /// </summary>
    public void Destroy()
    {
        for (int i = fightEffects.Count - 1; i >= 0; i--)
        {
            if (triggerCounts.TryGetValue(fightEffects[i].blockData.Pos, out int count))
            {
                fightEffects[i].Destroy();
            }
        }
    }

    /// <summary>
    /// 加成这次消除是否完成
    /// </summary>
    /// <returns></returns>
    public bool ChekeOver()
    {
        return fightEffects.Count == 0;
    }


    public override void Dispose()
    {
    }
}