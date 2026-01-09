using DG.Tweening;
using Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 冰块（锁）
/// </summary>
public class FightEffect_Lock :FightEffect
{
    public FightEffect_Lock(BlockData blockData, UIFightMain uiFightMain) :base(blockData, uiFightMain)
    {
    }

    public override bool CheckComplete(int count)
    {
        var clone = new BlockData(blockData);

        string animName = "Ice";
        var startNum = (int)blockData.Effect;
        var endNum = startNum - count <= 0 ? 0 : startNum - count;
        animName += $"{startNum}_{endNum}";
        ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_C_ice_001", o =>
        {
            var eff = GameObject.Instantiate(o, item.transform);
            item.ClearEffectImage();
            eff.GetComponent<Animator>().Play(animName);
            AudioManagerNew.Instance.PlayAudio("fight_item_icebreak.ogg");
            DOVirtual.DelayedCall(0.5f, () =>
            {
                GameObject.Destroy(eff);
            });
        });

        for (int i = 0; i < count; i++)
        {
            switch (blockData.Effect)
            {
                case EffectType.None:
                    return true;
                case EffectType.LockOne:
                    clone.Effect = EffectType.None;
                    break;
                case EffectType.LockTwice:
                    clone.Effect = EffectType.LockOne;
                    break;
                case EffectType.LockThrice:
                    clone.Effect = EffectType.LockTwice;
                    break;
            }
        }
        return false;
    }

    public override void Complete(int count)
    {
        for (int i = 0; i < count; i++)
        {
            switch (blockData.Effect)
            {
                case EffectType.None:
                    blockData.Reset();
                    break;
                case EffectType.LockOne:
                    blockData.Effect = EffectType.None;
                    break;
                case EffectType.LockTwice:
                    blockData.Effect = EffectType.LockOne;
                    break;
                case EffectType.LockThrice:
                    blockData.Effect = EffectType.LockTwice;
                    break;
            }
        }
    }

    public override void Destroy()
    {
        TriggerFightEffect.instance.fightEffects.Remove(this);
    }
}