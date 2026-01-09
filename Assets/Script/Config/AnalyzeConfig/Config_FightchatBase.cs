using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_FightchatBase : ConfigBase
{
    public Dictionary<int, Pb.FightchatBase> m_FightchatBaseDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("FightchatBase", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = FightchatBaseConfig.Descriptor.Parser.ParseFrom(data) as FightchatBaseConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_FightchatBaseDic, null))
            {
                m_FightchatBaseDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_FightchatBaseDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.FightchatBase GetConfigById(int Id)
    {
        if (m_FightchatBaseDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
