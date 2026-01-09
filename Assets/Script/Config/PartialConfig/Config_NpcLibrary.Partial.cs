using Pb;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class Config_NpcLibrary
{
    public int GetRandomNpcId(int id)
    {
        var config = GetConfigById(id);
        var npcIdListStr = config.Npcid.Split(";");

        if (npcIdListStr.Length == 1)
        {
            return int.Parse(npcIdListStr[0]);
        }
        
        return int.Parse(npcIdListStr[Random.Range(0, npcIdListStr.Length)]);
    }
}
