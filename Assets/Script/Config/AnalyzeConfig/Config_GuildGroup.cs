using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_GuildGroup : ConfigBase
{
    public Dictionary<int, Pb.GuildGroup> m_GuildGroupDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("GuildGroup", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = GuildGroupConfig.Descriptor.Parser.ParseFrom(data) as GuildGroupConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_GuildGroupDic, null))
            {
                m_GuildGroupDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_GuildGroupDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.GuildGroup GetConfigById(int Id)
    {
        if (m_GuildGroupDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
