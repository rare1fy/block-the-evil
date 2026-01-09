using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_NpcLibrary : ConfigBase
{
    public Dictionary<int, Pb.NpcLibrary> m_NpcLibraryDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("NpcLibrary", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = NpcLibraryConfig.Descriptor.Parser.ParseFrom(data) as NpcLibraryConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_NpcLibraryDic, null))
            {
                m_NpcLibraryDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_NpcLibraryDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.NpcLibrary GetConfigById(int Id)
    {
        if (m_NpcLibraryDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
