using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_BlockLibrary : ConfigBase
{
    public Dictionary<int, Pb.BlockLibrary> m_BlockLibraryDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("BlockLibrary", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = BlockLibraryConfig.Descriptor.Parser.ParseFrom(data) as BlockLibraryConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_BlockLibraryDic, null))
            {
                m_BlockLibraryDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_BlockLibraryDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.BlockLibrary GetConfigById(int Id)
    {
        if (m_BlockLibraryDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
