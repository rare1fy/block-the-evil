using Pb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;

public partial class Config_NpcUnlock : ConfigBase
{
    /// <summary>
    /// 通过关卡数获得解锁npcId
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public int GetNpcIdByLevel(int level)
    {
        int idx = (int)Math.Floor(level / (float)5);
        if (level != 0 && level % 5 == 0)
        {
            idx--;
        }
        var lst = m_NpcUnlockDic.Values.ToList();
        lst.Sort((a, b) =>
        {
            return a.Id - b.Id;
        });
        if (lst.Count > idx)
        {
            return lst[idx].Npcid;
        }
        else
        {
            return lst[lst.Count - 1].Npcid;
        }
    }
}
