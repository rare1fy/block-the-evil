using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_FightchatLibrary : ConfigBase
{
    public Dictionary<int, Pb.FightchatLibrary> m_FightchatLibraryDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("FightchatLibrary", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = FightchatLibraryConfig.Descriptor.Parser.ParseFrom(data) as FightchatLibraryConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_FightchatLibraryDic, null))
            {
                m_FightchatLibraryDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_FightchatLibraryDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.FightchatLibrary GetConfigById(int Id)
    {
        if (m_FightchatLibraryDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
