using Pb;
using System.Collections.Generic;
using System.IO;

public partial class Config_ChapterBase : ConfigBase
{
    public List<ChapterBase> GetShowChapterCfg()
    {
        List<ChapterBase> ret = new();
        foreach (var item in m_ChapterBaseDic.Values)
        {
            if (item.Hide == 1)
            {
                ret.Add(item);
            }
        }
        ret.Sort((a, b) => a.Id - b.Id);
        return ret;
    }
}
