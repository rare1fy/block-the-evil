using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_LevelWujingModle : ConfigBase
{
    public Dictionary<int, Pb.LevelWujingModle> m_LevelWujingModleDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("LevelWujingModle", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = LevelWujingModleConfig.Descriptor.Parser.ParseFrom(data) as LevelWujingModleConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_LevelWujingModleDic, null))
            {
                m_LevelWujingModleDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_LevelWujingModleDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.LevelWujingModle GetConfigById(int Id)
    {
        if (m_LevelWujingModleDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
