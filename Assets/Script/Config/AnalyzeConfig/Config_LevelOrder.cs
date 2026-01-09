using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_LevelOrder : ConfigBase
{
    public Dictionary<int, Pb.LevelOrder> m_LevelOrderDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("LevelOrder", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = LevelOrderConfig.Descriptor.Parser.ParseFrom(data) as LevelOrderConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_LevelOrderDic, null))
            {
                m_LevelOrderDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_LevelOrderDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.LevelOrder GetConfigById(int Id)
    {
        if (m_LevelOrderDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
