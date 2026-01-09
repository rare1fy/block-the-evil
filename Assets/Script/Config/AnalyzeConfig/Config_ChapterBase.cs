using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_ChapterBase : ConfigBase
{
    public Dictionary<int, Pb.ChapterBase> m_ChapterBaseDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("ChapterBase", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = ChapterBaseConfig.Descriptor.Parser.ParseFrom(data) as ChapterBaseConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_ChapterBaseDic, null))
            {
                m_ChapterBaseDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_ChapterBaseDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.ChapterBase GetConfigById(int Id)
    {
        if (m_ChapterBaseDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
