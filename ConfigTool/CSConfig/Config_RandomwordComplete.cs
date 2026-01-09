using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_RandomwordComplete : ConfigBase
{
    public Dictionary<int, Pb.RandomwordComplete> m_RandomwordCompleteDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("RandomwordComplete", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = RandomwordCompleteConfig.Descriptor.Parser.ParseFrom(data) as RandomwordCompleteConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_RandomwordCompleteDic, null))
            {
                m_RandomwordCompleteDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_RandomwordCompleteDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.RandomwordComplete GetConfigById(int Id)
    {
        if (m_RandomwordCompleteDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
