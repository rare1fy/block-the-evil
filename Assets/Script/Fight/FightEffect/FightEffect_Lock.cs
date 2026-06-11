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
        var previewEffect = blockData.Effect;

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
            if (previewEffect == EffectType.None)
            {
                return true;
            }

            previewEffect = BreakLockOnce(previewEffect);
        }

        return false;
    }

    public override void Complete(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (blockData.Effect == EffectType.None)
            {
                blockData.Reset();
                return;
            }

            blockData.Effect = BreakLockOnce(blockData.Effect);
        }
    }

    private EffectType BreakLockOnce(EffectType effect)
    {
        switch (effect)
        {
            case EffectType.LockOne:
                return EffectType.None;
            case EffectType.LockTwice:
                return EffectType.LockOne;
            case EffectType.LockThrice:
                return EffectType.LockTwice;
            default:
                return EffectType.None;
        }
    }

    public override void Destroy()
    {
        TriggerFightEffect.instance.fightEffects.Remove(this);
    }
}
