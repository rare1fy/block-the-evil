using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_NpcBase : ConfigBase
{
    public Dictionary<int, Pb.NpcBase> m_NpcBaseDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("NpcBase", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = NpcBaseConfig.Descriptor.Parser.ParseFrom(data) as NpcBaseConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_NpcBaseDic, null))
            {
                m_NpcBaseDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_NpcBaseDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.NpcBase GetConfigById(int Id)
    {
        if (m_NpcBaseDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
