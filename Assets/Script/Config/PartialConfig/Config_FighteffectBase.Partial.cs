using Pb;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class Config_FighteffectBase
{
    
    /// <summary>
    /// 获取对应类型的combo效果
    /// </summary>
    /// <param name="type">多消1 连消2</param>
    /// <param name="combo"></param>
    /// <returns></returns>
    public FighteffectBase GetEffectConfig(int type, int combo)
    {
        var list = GetTypeList(type);

        if (combo - 1 > list.Count) //超过最大值
        {
            return list[^1];
        }

        foreach (var fightEffect in list)
        {
            if (combo == fightEffect.Num)
                return fightEffect;
        }
        
        Debug.Log($"没有找到对应的FighteffectBase  type:{type}  combo:{combo}");
        return null;
    }
    
    
    private List<FighteffectBase> GetTypeList(int type)
    {
        var list = new List<FighteffectBase>();
        foreach (var data in m_FighteffectBaseDic)
        {
            if (data.Value.Type == type)
            {
                list.Add(data.Value);
            }
        }
        return list;
    }
}
