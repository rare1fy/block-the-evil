using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_CallwordBase : ConfigBase
{
    public Dictionary<int, Pb.CallwordBase> m_CallwordBaseDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("CallwordBase", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = CallwordBaseConfig.Descriptor.Parser.ParseFrom(data) as CallwordBaseConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_CallwordBaseDic, null))
            {
                m_CallwordBaseDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_CallwordBaseDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.CallwordBase GetConfigById(int Id)
    {
        if (m_CallwordBaseDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
