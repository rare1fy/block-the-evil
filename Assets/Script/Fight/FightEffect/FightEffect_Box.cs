using DG.Tweening;
using Framework;
using System;
using UnityEngine;

/// <summary>
/// 宝箱
/// </summary>
public class FightEffect_Box : FightEffect
{
    public FightEffect_Box(BlockData blockData, UIFightMain uiFightMain) : base(blockData, uiFightMain)
    {
    }

    public override bool CheckComplete(int count)
    {
        ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_C_numberbox_001", o =>
        {
            var eff = GameObject.Instantiate(o, item.transform);
            item.ClearEffectImage();
            eff.GetComponent<Animator>().Play("open");
            DOVirtual.DelayedCall(1f, () =>
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
        System.Random random = new System.Random();
        var id = random.Next(3,6);
        AudioManagerNew.Instance.PlayAudio("fight_item_treasureboxopen.ogg");
        uiFightMain.TriggerBoxEffect(blockData.Pos, id);
        var boxs = GameManager.Instance.CurFightControl.Model.boxDatas;
        for (int i = boxs.Count - 1; i >= 0; i--)
        {
            var box = boxs[i];
            if (box.pos.x == blockData.Pos.x && box.pos.y == blockData.Pos.y)
            {
                boxs.RemoveAt(i);
                break;
            }
        }
        DOVirtual.DelayedCall(0.21f, () =>
        {
            TriggerFightEffect.instance.fightEffects.Remove(this);
        });
    }
}