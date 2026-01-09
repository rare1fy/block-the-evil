using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_NpcitemLibrary : ConfigBase
{
    public Dictionary<int, Pb.NpcitemLibrary> m_NpcitemLibraryDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("NpcitemLibrary", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = NpcitemLibraryConfig.Descriptor.Parser.ParseFrom(data) as NpcitemLibraryConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_NpcitemLibraryDic, null))
            {
                m_NpcitemLibraryDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_NpcitemLibraryDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.NpcitemLibrary GetConfigById(int Id)
    {
        if (m_NpcitemLibraryDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
