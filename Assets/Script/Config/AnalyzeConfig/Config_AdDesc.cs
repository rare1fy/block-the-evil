using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_AdDesc : ConfigBase
{
    public Dictionary<int, Pb.AdDesc> m_AdDescDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("AdDesc", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = AdDescConfig.Descriptor.Parser.ParseFrom(data) as AdDescConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_AdDescDic, null))
            {
                m_AdDescDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_AdDescDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.AdDesc GetConfigById(int Id)
    {
        if (m_AdDescDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
