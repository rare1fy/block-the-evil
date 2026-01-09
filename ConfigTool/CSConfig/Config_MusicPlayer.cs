using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_MusicPlayer : ConfigBase
{
    public Dictionary<int, Pb.MusicPlayer> m_MusicPlayerDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("MusicPlayer", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = MusicPlayerConfig.Descriptor.Parser.ParseFrom(data) as MusicPlayerConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_MusicPlayerDic, null))
            {
                m_MusicPlayerDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_MusicPlayerDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.MusicPlayer GetConfigById(int Id)
    {
        if (m_MusicPlayerDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
