using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public class ChapterModel
{
    /// <summary>
    /// 章节的建筑数据
    /// </summary>
    public Dictionary<int, ChapterData> ChapterDic = new Dictionary<int, ChapterData>();
    /// <summary>
    /// 已领取的进度奖励
    /// </summary>
    public List<int> ScheduleRewords = new();

    public List<int> ChapterRewords = new();
    /// <summary>
    /// 当前显示的章节
    /// </summary>
    public int curChapter;
    /// <summary>
    /// 是否建筑中
    /// </summary>
    public bool isBuilding = false;

    public int lastBuildId = -1;
}

/// <summary>
/// 每章节建筑数据
/// </summary>
public class ChapterData
{
    /// <summary>
    /// 章节id
    /// </summary>
    int id;
    /// <summary>
    /// 已建筑id
    /// </summary>
    public List<int> buildedIds = new();
    /// <summary>
    /// 未建筑id
    /// </summary>
    public List<int> UnBuildIds = new();

    public ChapterData(int id) 
    {
        this.id = id;
        InitUnBuildIds();
    }
    public ChapterData( int id, List<int> buildedIds)
    {
        this.id = id;
        this.buildedIds = buildedIds;
        InitUnBuildIds();
    }

    private void InitUnBuildIds()
    {
        var cfgs = Config.GetConfig<Config_BuildBase>().m_BuildBaseDic;
        UnBuildIds.Clear();
        if (buildedIds.Count == 0)
        {
            foreach (var cfg in cfgs)
            {
                if (cfg.Value.Group == id)
                {
                    UnBuildIds.Add(cfg.Key);
                }
            }
        }
        else
        {
            foreach (var cfg in cfgs)
            {
                if (!buildedIds.Exists(p=> p == cfg.Key) && cfg.Value.Group == id)
                {
                    UnBuildIds.Add(cfg.Key);
                }
            }
        }
        UnBuildIds.Sort((a, b) => { return a - b; });
    }
}