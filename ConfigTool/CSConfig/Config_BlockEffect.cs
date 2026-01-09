using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_BlockEffect : ConfigBase
{
    public Dictionary<int, Pb.BlockEffect> m_BlockEffectDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("BlockEffect", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = BlockEffectConfig.Descriptor.Parser.ParseFrom(data) as BlockEffectConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_BlockEffectDic, null))
            {
                m_BlockEffectDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_BlockEffectDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.BlockEffect GetConfigById(int Id)
    {
        if (m_BlockEffectDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
