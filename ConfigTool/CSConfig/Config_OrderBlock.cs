using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_OrderBlock : ConfigBase
{
    public Dictionary<int, Pb.OrderBlock> m_OrderBlockDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("OrderBlock", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = OrderBlockConfig.Descriptor.Parser.ParseFrom(data) as OrderBlockConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_OrderBlockDic, null))
            {
                m_OrderBlockDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_OrderBlockDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.OrderBlock GetConfigById(int Id)
    {
        if (m_OrderBlockDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
