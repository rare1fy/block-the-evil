using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_BlockColor : ConfigBase
{
    public Dictionary<int, Pb.BlockColor> m_BlockColorDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("BlockColor", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = BlockColorConfig.Descriptor.Parser.ParseFrom(data) as BlockColorConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_BlockColorDic, null))
            {
                m_BlockColorDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_BlockColorDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.BlockColor GetConfigById(int Id)
    {
        if (m_BlockColorDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
