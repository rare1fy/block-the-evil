
using System.Collections.Generic;

public partial class Config_LevelOrder
{
    public List<int> GetOrderIdList(int levelId)
    {
        var idList = new List<int>();
        foreach (var order in m_LevelOrderDic)
        {
            if(order.Value.Level == levelId)
                idList.Add(order.Value.Id);
        }
        return idList;
    }
    
}
