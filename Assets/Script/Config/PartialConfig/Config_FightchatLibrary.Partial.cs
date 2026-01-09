using Pb;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public partial class Config_FightchatLibrary
{
    public List<int> GetUnLockWorldToIds(int id, int level, int order)
    {
        var list = m_FightchatLibraryDic.Values.ToList();
        list.Sort((a,b) => a.Id - b.Id);
        List<int> ints = new();
        foreach (var item in list)
        {
            if (id == item.Npcid && item.Level <= level && order >= item.Order)
            {
                ints.Add(item.Wordto);
            }
        }
        return ints;
    }
}
