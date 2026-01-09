using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_LevelTreasurechest : ConfigBase
{
    public Dictionary<int, Pb.LevelTreasurechest> m_LevelTreasurechestDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("LevelTreasurechest", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = LevelTreasurechestConfig.Descriptor.Parser.ParseFrom(data) as LevelTreasurechestConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_LevelTreasurechestDic, null))
            {
                m_LevelTreasurechestDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_LevelTreasurechestDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.LevelTreasurechest GetConfigById(int Id)
    {
        if (m_LevelTreasurechestDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
