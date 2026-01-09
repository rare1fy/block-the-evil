using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_BlockBase : ConfigBase
{
    public Dictionary<int, Pb.BlockBase> m_BlockBaseDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("BlockBase", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = BlockBaseConfig.Descriptor.Parser.ParseFrom(data) as BlockBaseConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_BlockBaseDic, null))
            {
                m_BlockBaseDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_BlockBaseDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.BlockBase GetConfigById(int Id)
    {
        if (m_BlockBaseDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
