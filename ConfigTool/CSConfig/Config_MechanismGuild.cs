using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_MechanismGuild : ConfigBase
{
    public Dictionary<int, Pb.MechanismGuild> m_MechanismGuildDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("MechanismGuild", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = MechanismGuildConfig.Descriptor.Parser.ParseFrom(data) as MechanismGuildConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_MechanismGuildDic, null))
            {
                m_MechanismGuildDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_MechanismGuildDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.MechanismGuild GetConfigById(int Id)
    {
        if (m_MechanismGuildDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
