using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;


public class FightEffect
{
    public BlockData blockData;
    public UIFightMain uiFightMain;
    public BlockItem item;
    public FightEffect(BlockData blockData, UIFightMain uiFightMain)
    {
        this.blockData = blockData;
        this.uiFightMain = uiFightMain;
        this.item = uiFightMain.GetBlockItemByPos(blockData.Pos);
    }

    public virtual bool CheckComplete(int count)
    {
        return true;
    }

    public virtual void Complete(int count)
    {
    }

    public virtual void CompleteEnd()
    {

    }

    public virtual void Destroy() 
    {
    }
}