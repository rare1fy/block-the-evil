using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_NpcUnlock : ConfigBase
{
    public Dictionary<int, Pb.NpcUnlock> m_NpcUnlockDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("NpcUnlock", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = NpcUnlockConfig.Descriptor.Parser.ParseFrom(data) as NpcUnlockConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_NpcUnlockDic, null))
            {
                m_NpcUnlockDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_NpcUnlockDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.NpcUnlock GetConfigById(int Id)
    {
        if (m_NpcUnlockDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
