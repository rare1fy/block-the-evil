using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_GuildPerform : ConfigBase
{
    public Dictionary<int, Pb.GuildPerform> m_GuildPerformDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("GuildPerform", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = GuildPerformConfig.Descriptor.Parser.ParseFrom(data) as GuildPerformConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_GuildPerformDic, null))
            {
                m_GuildPerformDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_GuildPerformDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.GuildPerform GetConfigById(int Id)
    {
        if (m_GuildPerformDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
