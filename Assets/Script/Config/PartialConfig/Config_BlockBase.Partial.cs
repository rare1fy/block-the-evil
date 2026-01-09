using System.Collections.Generic;
using Pb;
using UnityEngine;

public partial class Config_BlockBase
{
    public BlockBase GetRandomPuzzleIndex(int type)
    {
        var blockList = GetBlockBase(type);
        var r = Random.Range(0, blockList.Count);
        return blockList[r];
    }
    
    public List<BlockBase> GetBlockBase(int type)
    {
        var list = new List<BlockBase>();
        foreach (var blockBase in m_BlockBaseDic)
        {
            if (blockBase.Value.Difficulty == type)
            {
                list.Add(blockBase.Value);
            }
        }
        return list;
    }
}