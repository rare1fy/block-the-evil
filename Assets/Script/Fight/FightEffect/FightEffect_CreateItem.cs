using DG.Tweening;
using Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 生成item
/// </summary>
public class FightEffect_CreateItem : FightEffect
{
    public FightEffect_CreateItem(BlockData blockData, UIFightMain uiFightMain):base(blockData, uiFightMain)
    {
    }

    public override bool CheckComplete(int count)
    {
        ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_C_Boxboom001", obj =>
        {
            AudioManagerNew.Instance.PlayAudio("fight_item_zhixiangziopen.ogg");
            var eff = GameObject.Instantiate(obj, item.transform);
            item.ClearEffectImage();
            DOVirtual.DelayedCall(0.5f, () =>
            {
                GameObject.Destroy(eff);
            });
        });
        return true;
    }

    public override void Complete(int count)
    {
        blockData.Reset();
    }

    public override void Destroy() 
    {
        List<BlockData> blockDatas = new List<BlockData>();
        var ctrl = GameManager.Instance.CurFightControl;
        //遍历所有方块
        for (int i = 0; i < FightModel.GRID_HEIGHT; i++)
        {
            for (int j = 0; j < FightModel.GRID_WIDTH; j++)
            {
                var data = ctrl.Model.MBlockList[i, j];

                if (data.IsOccupied
                    && data.Effect == EffectType.None
                    && !ctrl.IsItemBlok(data)
                    && data.ColorType > 0
                    && (data.ColorType <= 5 || data.ColorType == BlockData.WhiteColorType))
                {
                    blockDatas.Add(data);
                }
            }
        }

        if (blockDatas.Count > 0)
        {
            System.Random random = new System.Random();
            var colorCounts = GetCurrentBoardColorCounts();
            var Count = Config.GetConfig<Config_GdConstant>().GetConfigById(24).Num;
            Count = Count > blockDatas.Count ? blockDatas.Count : Count;
            for (int i = 0; i < Count; i++)
            {
                var colorType = GetCreateItemColor(colorCounts);
                if (colorType <= 0)
                    break;

                var blockIdx = random.Next(0, blockDatas.Count);
                uiFightMain.TriggerCreateItem(item, blockDatas[blockIdx], colorType);
                blockDatas.RemoveAt(blockIdx);
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
        DOVirtual.DelayedCall(0.21f, () =>
        {
            TriggerFightEffect.instance.fightEffects.Remove(this);
        });
    }

    private int GetCreateItemColor(Dictionary<int, int> colorCounts)
    {
        var ctrl = GameManager.Instance.CurFightControl;
        var targetColor = ctrl.LevelController.Model.MonsterBattleState.GetMostNeededColor(colorCounts);
        if (targetColor > 0)
            return targetColor;

        return 0;
    }

    private Dictionary<int, int> GetCurrentBoardColorCounts()
    {
        var colorCounts = new Dictionary<int, int>();
        var ctrl = GameManager.Instance.CurFightControl;
        for (int i = 0; i < FightModel.GRID_HEIGHT; i++)
        {
            for (int j = 0; j < FightModel.GRID_WIDTH; j++)
            {
                var data = ctrl.Model.MBlockList[i, j];
                if (!data.IsOccupied || data.ColorType <= 0)
                    continue;

                if (colorCounts.ContainsKey(data.ColorType))
                {
                    colorCounts[data.ColorType]++;
                }
                else
                {
                    colorCounts.Add(data.ColorType, 1);
                }
            }
        }

        return colorCounts;
    }
}
