using DG.Tweening;
using Framework;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 生成冰块(锁)
/// </summary>
public class FightEffect_CreateIce : FightEffect
{
    public FightEffect_CreateIce(BlockData blockData, UIFightMain uiFightMain) : base(blockData, uiFightMain)
    {
    }

    public override bool CheckComplete(int count)
    {
        ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_C_Bingtong01", obj =>
        {
            AudioManagerNew.Instance.PlayAudio("fight_item_icefridge.ogg");
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

                if (data.IsOccupied && data.Effect == EffectType.None && BlockData.IsBasicClearColor(data.ColorType))
                {
                    blockDatas.Add(data);
                }
            }
        }

        if (blockDatas.Count > 0)
        {
            System.Random random = new System.Random();
            var Count = Config.GetConfig<Config_GdConstant>().GetConfigById(23).Num;
            Count = Count > blockDatas.Count ? blockDatas.Count : Count;
            for (int i = 0; i < Count; i++)
            {
                var idx = random.Next(0, blockDatas.Count);
                uiFightMain.TriggerCreateIce(item, blockDatas[idx]);
                blockDatas.RemoveAt(idx);
            }
        }
        DOVirtual.DelayedCall(0.21f, () =>
        {
            TriggerFightEffect.instance.fightEffects.Remove(this);
        });
    }
}
