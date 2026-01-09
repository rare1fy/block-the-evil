using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_SighinBase : ConfigBase
{
    public Dictionary<int, Pb.SighinBase> m_SighinBaseDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("SighinBase", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = SighinBaseConfig.Descriptor.Parser.ParseFrom(data) as SighinBaseConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_SighinBaseDic, null))
            {
                m_SighinBaseDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_SighinBaseDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.SighinBase GetConfigById(int Id)
    {
        if (m_SighinBaseDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
