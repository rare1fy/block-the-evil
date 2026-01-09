using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_OutWord : ConfigBase
{
    public Dictionary<int, Pb.OutWord> m_OutWordDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("OutWord", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = OutWordConfig.Descriptor.Parser.ParseFrom(data) as OutWordConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_OutWordDic, null))
            {
                m_OutWordDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_OutWordDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.OutWord GetConfigById(int Id)
    {
        if (m_OutWordDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
