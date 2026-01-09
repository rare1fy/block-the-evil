using DG.Tweening;
using Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 炸弹
/// </summary>
public class FightEffect_Bomb : FightEffect
{
    public FightEffect_Bomb(BlockData blockData, UIFightMain uiFightMain) : base(blockData, uiFightMain)
    {
    }

    public override bool CheckComplete(int count)
    {
        ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_C_TNTboom001", obj =>
        {
            var eff = GameObject.Instantiate(obj, item.transform);
            item.ClearEffectImage();
            AudioManagerNew.Instance.PlayAudio("fight_item_bomb.ogg");
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

    public override void CompleteEnd()
    {
        List<Vector2Int> clearBlocks = new List<Vector2Int>();
        var ctrl = GameManager.Instance.CurFightControl;
        for (int i = -2; i <= 2; i++)
        {
            for (int j = -2; j <= 2; j++)
            {
                var pos = blockData.Pos - new Vector2Int(i, j);
                if ((i == 0 && j == 0)|| pos.x < 0 || pos.y < 0 || pos.x >= 8 || pos.y >= 8)
                {
                    continue;
                }
                var data = ctrl.Model.GetBlockDataByPos(pos);
                if (data.ColorType != 0 &&(data.Effect == EffectType.None || data.Effect == EffectType.LockOne || data.Effect == EffectType.LockTwice || data.Effect == EffectType.LockThrice))
                {
                    clearBlocks.Add(data.Pos);
                }
            }
        }
        uiFightMain.PlayClearEffect(clearBlocks);
        //ctrl.LevelController.TotalClearBlock(clearBlocks,uiFightMain);
        ctrl.Model.SetBlockDataByPosList(clearBlocks, false, 0, EffectType.None); //更改格子数据
        //uiFightMain._Obj_OrderPanel.PlayFinishEffect(ctrl.LevelController.Model.FightOrders);
        ctrl.RefreshAllPuzzleItem();
        //var isFinish = ctrl.LevelController.CheckFightOrder();
        //var animTime = isFinish ? 0.5f : 0f; //订单有完成的就多一个播特效的时间
        DOVirtual.DelayedCall(0.2f, () =>
        {
            uiFightMain.RefreshAllBlock();
            TriggerFightEffect.instance.fightEffects.Remove(this);

        });
    }
}