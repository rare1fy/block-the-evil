using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_GoodfeelNpc : ConfigBase
{
    public Dictionary<int, Pb.GoodfeelNpc> m_GoodfeelNpcDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("GoodfeelNpc", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = GoodfeelNpcConfig.Descriptor.Parser.ParseFrom(data) as GoodfeelNpcConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_GoodfeelNpcDic, null))
            {
                m_GoodfeelNpcDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_GoodfeelNpcDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.GoodfeelNpc GetConfigById(int Id)
    {
        if (m_GoodfeelNpcDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
