using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_RankWujing : ConfigBase
{
    public Dictionary<int, Pb.RankWujing> m_RankWujingDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("RankWujing", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = RankWujingConfig.Descriptor.Parser.ParseFrom(data) as RankWujingConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_RankWujingDic, null))
            {
                m_RankWujingDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_RankWujingDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.RankWujing GetConfigById(int Id)
    {
        if (m_RankWujingDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
