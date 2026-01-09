using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_RandomwordHarry : ConfigBase
{
    public Dictionary<int, Pb.RandomwordHarry> m_RandomwordHarryDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("RandomwordHarry", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = RandomwordHarryConfig.Descriptor.Parser.ParseFrom(data) as RandomwordHarryConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_RandomwordHarryDic, null))
            {
                m_RandomwordHarryDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_RandomwordHarryDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.RandomwordHarry GetConfigById(int Id)
    {
        if (m_RandomwordHarryDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
