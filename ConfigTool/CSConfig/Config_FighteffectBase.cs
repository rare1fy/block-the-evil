using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_FighteffectBase : ConfigBase
{
    public Dictionary<int, Pb.FighteffectBase> m_FighteffectBaseDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("FighteffectBase", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = FighteffectBaseConfig.Descriptor.Parser.ParseFrom(data) as FighteffectBaseConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_FighteffectBaseDic, null))
            {
                m_FighteffectBaseDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_FighteffectBaseDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.FighteffectBase GetConfigById(int Id)
    {
        if (m_FighteffectBaseDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
