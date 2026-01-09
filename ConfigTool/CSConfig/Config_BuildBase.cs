using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_BuildBase : ConfigBase
{
    public Dictionary<int, Pb.BuildBase> m_BuildBaseDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("BuildBase", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = BuildBaseConfig.Descriptor.Parser.ParseFrom(data) as BuildBaseConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_BuildBaseDic, null))
            {
                m_BuildBaseDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_BuildBaseDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.BuildBase GetConfigById(int Id)
    {
        if (m_BuildBaseDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
