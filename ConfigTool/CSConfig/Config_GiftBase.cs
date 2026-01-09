using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_GiftBase : ConfigBase
{
    public Dictionary<int, Pb.GiftBase> m_GiftBaseDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("GiftBase", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = GiftBaseConfig.Descriptor.Parser.ParseFrom(data) as GiftBaseConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_GiftBaseDic, null))
            {
                m_GiftBaseDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_GiftBaseDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.GiftBase GetConfigById(int Id)
    {
        if (m_GiftBaseDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
