using System.Collections.Generic;
using System;
using System.Linq;

public partial class Config_NpcBase : ConfigBase
{
    List<int> Surplus = new List<int>();

   public List<int> GetRandomNormalNpcIds()
    {
        List<int> ret = new List<int>();
        var rendom = new Random();
        var num = rendom.Next(1,6);
        while(num > 0)
        {
            if (Surplus.Count == 0) 
            { 
                Surplus = m_NpcBaseDic.Values.Where(p => p.Special == 0).Select(p=>p.Id).ToList();
            }
            var idx = rendom.Next(0, Surplus.Count);
            if (ret.Contains(Surplus[idx])) 
            {
                idx = rendom.Next(0, Surplus.Count);
            }
            ret.Add(Surplus[idx]);
            Surplus.RemoveAt(idx);
            num--;
        }
        return ret;
    }
}
