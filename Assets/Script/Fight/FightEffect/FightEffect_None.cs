using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;


public class FightEffect_None: FightEffect
{
    public FightEffect_None(BlockData blockData, UIFightMain uiFightMain) : base(blockData, uiFightMain)
    {
        this.blockData = blockData;
        this.uiFightMain = uiFightMain;
    }

    public override bool CheckComplete(int count)
    {
        return true;
    }

    public override void Complete(int count)
    {
        blockData.Reset();
    }

    public override void CompleteEnd()
    {

    }

    public override void Destroy() 
    {
        TriggerFightEffect.instance.fightEffects.Remove(this);
    }
}