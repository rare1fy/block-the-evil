using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_GdConstant : ConfigBase
{
    public Dictionary<int, Pb.GdConstant> m_GdConstantDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("GdConstant", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = GdConstantConfig.Descriptor.Parser.ParseFrom(data) as GdConstantConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_GdConstantDic, null))
            {
                m_GdConstantDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_GdConstantDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.GdConstant GetConfigById(int Id)
    {
        if (m_GdConstantDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
