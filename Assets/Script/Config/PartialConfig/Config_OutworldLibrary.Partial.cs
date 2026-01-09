using JetBrains.Annotations;
using System.Collections.Generic;

public partial class Config_OutworldLibrary
{
    public int GetOutWorldIdByConfig(int npcId, int Leavl, int feelLv)
    {
        foreach (var item in m_OutworldLibraryDic.Values)
        {
            if (item.PlayerId == npcId && Leavl >= item.Leavl && feelLv >= item.Level)
            {
                return item.Wordto;
            }
        }
        return 0;
    }

    public List<int> GetOutWorldList(int npcId, int Leavl, int feelLv)
    {
        List<int> ret = new List<int>();
        foreach (var item in m_OutworldLibraryDic.Values)
        {
            if (item.PlayerId == npcId && Leavl >= item.Leavl && feelLv >= item.Level)
            {
                ret.Add(item.Wordto);
            }
        }
        ret.Sort((a, b) =>
        {
            return a - b;
        });
        return ret;
    }
}
