using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_GuildWord : ConfigBase
{
    public Dictionary<int, Pb.GuildWord> m_GuildWordDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("GuildWord", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = GuildWordConfig.Descriptor.Parser.ParseFrom(data) as GuildWordConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_GuildWordDic, null))
            {
                m_GuildWordDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_GuildWordDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.GuildWord GetConfigById(int Id)
    {
        if (m_GuildWordDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
