using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_ItemBase : ConfigBase
{
    public Dictionary<int, Pb.ItemBase> m_ItemBaseDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("ItemBase", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = ItemBaseConfig.Descriptor.Parser.ParseFrom(data) as ItemBaseConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_ItemBaseDic, null))
            {
                m_ItemBaseDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_ItemBaseDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.ItemBase GetConfigById(int Id)
    {
        if (m_ItemBaseDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
