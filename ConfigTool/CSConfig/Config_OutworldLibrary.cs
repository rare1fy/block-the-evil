using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_OutworldLibrary : ConfigBase
{
    public Dictionary<int, Pb.OutworldLibrary> m_OutworldLibraryDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("OutworldLibrary", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = OutworldLibraryConfig.Descriptor.Parser.ParseFrom(data) as OutworldLibraryConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_OutworldLibraryDic, null))
            {
                m_OutworldLibraryDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_OutworldLibraryDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.OutworldLibrary GetConfigById(int Id)
    {
        if (m_OutworldLibraryDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
