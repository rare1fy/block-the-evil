using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_DefineError : ConfigBase
{
    public Dictionary<int, Pb.DefineError> m_DefineErrorDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("DefineError", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = DefineErrorConfig.Descriptor.Parser.ParseFrom(data) as DefineErrorConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_DefineErrorDic, null))
            {
                m_DefineErrorDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_DefineErrorDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.DefineError GetConfigById(int Id)
    {
        if (m_DefineErrorDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
