using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_CallwordLibrary : ConfigBase
{
    public Dictionary<int, Pb.CallwordLibrary> m_CallwordLibraryDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("CallwordLibrary", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = CallwordLibraryConfig.Descriptor.Parser.ParseFrom(data) as CallwordLibraryConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_CallwordLibraryDic, null))
            {
                m_CallwordLibraryDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_CallwordLibraryDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.CallwordLibrary GetConfigById(int Id)
    {
        if (m_CallwordLibraryDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
