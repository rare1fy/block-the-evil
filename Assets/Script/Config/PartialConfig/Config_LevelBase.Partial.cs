using Pb;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class Config_LevelBase
{
    //获取随机块的难度
    public int RandomDif(string str)
    {
        var random = Random.Range(0, 101);
        //Debug.LogError("随机数:"+random);
        var list = Util.GetRewardConfig(str);
        foreach (var reward in list)
        {
            if (random <= reward.Number)
            {
                return reward.Id;
            }
            random -= (int)reward.Number;
        }
        return 0;
    }
}
