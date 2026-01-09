using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_ShareWechatImage : ConfigBase
{
    public Dictionary<int, Pb.ShareWechatImage> m_ShareWechatImageDic = new();

    public override void LoadConfigStep()
    {
        Config.instance._configLoadData.TryAdd("ShareWechatImage", ReadConfig);
    }

    private void ReadConfig(Stream data)
    {
        var tempConfig = ShareWechatImageConfig.Descriptor.Parser.ParseFrom(data) as ShareWechatImageConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_ShareWechatImageDic, null))
            {
                m_ShareWechatImageDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_ShareWechatImageDic.TryAdd(item.Id, item);
            }
        }
    }

    public Pb.ShareWechatImage GetConfigById(int Id)
    {
        if (m_ShareWechatImageDic.TryGetValue(Id, out var config))
        {
            return config;
        }
        return null;
    }
}
