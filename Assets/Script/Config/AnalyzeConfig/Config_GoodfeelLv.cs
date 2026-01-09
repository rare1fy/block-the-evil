using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_GoodfeelLv : ConfigBase
{
    public Dictionary<int, Pb.GoodfeelLv> m_GoodfeelLvDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("GoodfeelLv", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = GoodfeelLvConfig.Descriptor.Parser.ParseFrom(data) as GoodfeelLvConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_GoodfeelLvDic, null))
            {
                m_GoodfeelLvDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_GoodfeelLvDic.TryAdd(item.Lv, item);
            }
        }
    }

    public Pb.GoodfeelLv GetConfigById(int Id)
    {
        if (m_GoodfeelLvDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
