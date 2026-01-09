using UnityEngine;

public partial class Config_NpcitemLibrary 
{
    public int GetRandomItemId(int id)
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
