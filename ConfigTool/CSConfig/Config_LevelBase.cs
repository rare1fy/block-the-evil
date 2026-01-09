using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_LevelBase : ConfigBase
{
    public Dictionary<int, Pb.LevelBase> m_LevelBaseDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("LevelBase", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = LevelBaseConfig.Descriptor.Parser.ParseFrom(data) as LevelBaseConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_LevelBaseDic, null))
            {
                m_LevelBaseDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_LevelBaseDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.LevelBase GetConfigById(int Id)
    {
        if (m_LevelBaseDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
