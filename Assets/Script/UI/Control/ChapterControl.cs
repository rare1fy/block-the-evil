using Pb;
using System.Collections.Generic;
using UnityEngine;

public class ChapterControl : BaseControl
{
    public const string Architecture = "TetrisBarClient_Architecture";
    public const string ScheduleRewords = "TetrisBarClient_ScheduleRewords";
    public const string Chapter = "TetrisBarClient_Chapter";

    private ChapterModel model;
    public ChapterModel Model {  
        
        get { 
            if (model == null)
                model = new ChapterModel();
            return model; 
        }
        private set { model = value; }
    }

    protected override void OnInitControl() 
    {
        EventDispatchCenter.Instance.Registry(SDEvents.CHANGE_LEAVL, CheckAndAddChapter);
        var ChapterBaseList = Config.GetConfig<Config_ChapterBase>().GetShowChapterCfg();
        var leavl = GameManager.Instance.PlayerControl.PlayerModel.Level;
        foreach (var Chapter in ChapterBaseList)
        {
            if(Chapter.Lvmin <= leavl)
            {
                AnalysisChapter(Chapter.Id);
            }
        }
        AnalysisScheduleRewords();
        AnalysisChapterPrefRewords();
        AnalysisChapter();
    }

    public void AnalysisChapter()
    {
        if (PlayerPrefs.HasKey(Chapter))
        {
            Model.curChapter = PlayerPrefs.GetInt(Chapter);
        }
        else
        {
            Model.curChapter = 1;
            SetChaperPerfs();
        }
    }

    private void SetChaperPerfs()
    {
        PlayerPrefs.SetInt(Chapter, Model.curChapter);
    }

    /// <summary>
    /// 处理本地缓存章节信息
    /// </summary>
    /// <param name="id"></param>
    private void SetChapterData(int id)
    {
        if(Model.ChapterDic.TryGetValue(id, out var data))
        {
            var strData = "";
            foreach (var item in data.buildedIds)
            {
                strData += $"{item};";
            }
            PlayerPrefs.SetString(Architecture + id, strData);
        }
    }

    /// <summary>
    /// 进游戏 解析缓存的章节
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private void AnalysisChapter(int id)
    {
        if (PlayerPrefs.HasKey(Architecture + id))
        {
            var strData = PlayerPrefs.GetString(Architecture + id);
            var strList = strData.Split(";");
            List<int> buildIds = new List<int>();
            foreach (var item in strList)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    buildIds.Add(int.Parse(item));
                }
            }
            Model.ChapterDic.TryAdd(id, new ChapterData(id, buildIds));
        }
        else
        {
            //第一次进游戏
            Model.ChapterDic.TryAdd(id, new ChapterData(id));
            SetChapterData(id);
        }
    }

    /// <summary>
    /// 关卡增加 检测新章节并处理数据
    /// </summary>
    public void CheckAndAddChapter(object obj = null)
    {
        var ChapterBaseDic = Config.GetConfig<Config_ChapterBase>().m_ChapterBaseDic;
        var leavl = GameManager.Instance.PlayerControl.PlayerModel.Level;
        foreach (var Chapter in ChapterBaseDic.Values)
        {
            if (Chapter.Lvmin <= leavl && !Model.ChapterDic.TryGetValue(Chapter.Id, out var data))
            {
                data = new ChapterData(Chapter.Id);
                Model.ChapterDic.Add(Chapter.Id, data);
                SetChapterData(Chapter.Id);
            }
        }
    }

    /// <summary>
    /// 建造
    /// </summary>
    /// <param name="chapterId">章节</param>
    /// <param name="buildId">建造Id</param>  
    /// <param name="clickPos">点击位置</param>
    public void OnBuild(int chapterId, int buildId)
    {
        if (!Model.ChapterDic.TryGetValue(chapterId, out var data))
        {
            Debug.LogError("章节为解锁，无法进行建筑！！");
        }
        data.buildedIds.Add(buildId);
        data.UnBuildIds.Remove(buildId);
        SetChapterData(chapterId);
        EventDispatchCenter.Instance.Dispatch(SDEvents.BUILD_ARCHITECTURE, buildId);
        Model.isBuilding = true;
        Model.lastBuildId = buildId;
    }

    /// <summary>
    /// 关卡刷新或建筑建造后调用 用于处理章节切换
    /// </summary>
    /// <returns></returns>
    public bool RefreshChapter()
    {
        var level = GameManager.Instance.PlayerControl.PlayerModel.Level;
        var curChapter = Model.curChapter;
        var chapterCfg = Config.GetConfig<Config_ChapterBase>().GetConfigById(curChapter);
        if (IsOpenNextChapter()&& !IsMaxChapter() && level >= chapterCfg.Lvmax)
        {
            Model.curChapter++;
            SetChaperPerfs();
            return true;
        }
        return false;
    }

    public bool IsMaxChapter()
    {
        var chapterNum = GetChapterCount();
        return chapterNum >= Model.curChapter;
    }

    public bool IsOpenNextChapter()
    {
        var curChapter = Model.curChapter;
        var chapterCfg = Config.GetConfig<Config_ChapterBase>().GetConfigById(curChapter);
        var maxBuild = GetChapterMaxBuild(curChapter);
        var curBuild = GetChapterBuildCount(curChapter);
        var level = GameManager.Instance.PlayerControl.PlayerModel.Level;
        return curBuild >= maxBuild ;
    }

    /// <summary>
    /// 检测是否有上一个建造建筑
    /// </summary>
    /// <returns></returns>
    public bool CheckLastBuild()
    {
        return Model.lastBuildId > 0;
    }

    /// <summary>
    /// 是否为上一次建造的建筑
    /// </summary>
    /// <param name="buildId"></param>
    /// <returns></returns>
    public bool IsLastBuild(int buildId)
    {
        return Model.lastBuildId == buildId;
    }

    public List<BuildBase> GetShowBuild(int id)
    {
        var ret = new List<BuildBase>();
        var tmp = new List<BuildBase>();

        BuildBase lastCfg = null;
        int batch = 0;
        
        if (Model.lastBuildId > 0)
        {
            lastCfg = Config.GetConfig<Config_BuildBase>().GetConfigById(Model.lastBuildId);
            ret.Add(lastCfg);
        }

        if (Model.ChapterDic.TryGetValue(id, out var data))
        {
            for (int i = 0; i < data.UnBuildIds.Count; i++)
            {
                var build = Config.GetConfig<Config_BuildBase>().GetConfigById(data.UnBuildIds[i]);
                if (batch != 0 && build.Batch != batch)
                {
                    break;
                }
                else
                {
                    batch = build.Batch;
                    tmp.Add(build);
                }
            }
        }
        if (lastCfg == null)
        {
            return tmp;
        }
        else if (lastCfg.Batch == batch)
        {
            ret.AddRange(tmp);
            ret.Sort((a, b) =>
            {
                return a.Id - b.Id;
            });
            return ret;
        }
        else
        {
            return ret;
        }
    }



    /// <summary>
    /// 返回当前章节可建造的建筑
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public List<BuildBase> GetCanBuildItems(int id)
    {
        List<BuildBase> items = new List<BuildBase>();
        if(Model.ChapterDic.TryGetValue(id, out var data))
        {
            int batch = 0;
            for (int i = 0; i < data.UnBuildIds.Count; i++)
            {
                var build = Config.GetConfig<Config_BuildBase>().GetConfigById(data.UnBuildIds[i]);
                if (batch != 0 && build.Batch != batch)
                {
                    return items;
                }
                else
                {
                    batch = build.Batch; 
                    items.Add(build);
                }
            }
        }
        return items;
    }

    /// <summary>
    /// 检查建造关卡是否满足
    /// </summary>
    /// <param name="buildId"></param>
    /// <returns></returns>
    public bool CheckBuildLeavl(int buildId)
    {
        var leavl = GameManager.Instance.PlayerControl.PlayerModel.Level;
        var cfg = Config.GetConfig<Config_BuildBase>().GetConfigById(buildId);
        var leavlChack = leavl >= cfg.Unlocklevel;
        return leavlChack;
    }

    /// <summary>
    /// 获取章节已建造的数量
    /// </summary>
    /// <param name="id">章节id</param>
    /// <returns></returns>
    public int GetChapterBuildCount(int id)
    {
        if(Model.ChapterDic.TryGetValue(id,out var data))
        {
            return data.buildedIds.Count;
        }
        return 0;
    }

    public List<int> GetCompletedBuild()
    {
        if (Model.ChapterDic.TryGetValue(Model.curChapter, out var data))
        {
            return data.buildedIds;
        }
        return new List<int>();

    }

    public int GetChapterMaxBuild(int chapterId)
    {
        var cfgs = Config.GetConfig<Config_BuildBase>().m_BuildBaseDic;
        int ret = 0;
        foreach (var cfg in cfgs)
        {
            if (cfg.Value.Group == chapterId)
                ret++;

        }
        return ret;
    }

    /// <summary>
    /// 是否还有没有建造的建筑
    /// </summary>
    /// <returns></returns>
    public bool CheckHasNotBuilt()
    {
        if (Model.ChapterDic.TryGetValue(Model.curChapter, out var chapterData))
        {
            return chapterData.UnBuildIds.Count > 0;
        }
        return false;
    }

    ///// <summary>
    ///// 获得当前章节的建造奖励
    ///// </summary>
    ///// <param name="id">奖励配置id</param>
    ///// <returns></returns>
    //public List<BuildReward> GetBuildRewards(int id)
    //{
    //    var ret = new List<BuildReward>();
    //    var cfgs = Config.GetConfig<Config_BuildReward>().m_BuildRewardDic;
    //    foreach (var cfg in cfgs.Values) 
    //    { 
    //       if(cfg.Group == id)
    //        {
    //            ret.Add(cfg);
    //        }
    //    }
    //    ret.Sort((a, b) => a.Maxnum - b.Maxnum);
    //    return ret;
    //}

    /// <summary>
    /// 缓存已领取的进度奖励
    /// </summary>
    /// <param name="id">奖励配置id</param>
    private void SetScheduleRewordsData(int id)
    {
        string strData = "";
        foreach (var item in Model.ScheduleRewords)
        {
            strData += $"{item};";
        }
        PlayerPrefs.SetString(ScheduleRewords,strData);
    }

    /// <summary>
    /// 解析初始化已领取的进度奖励
    /// </summary>
    private void AnalysisScheduleRewords()
    {
        if (PlayerPrefs.HasKey(ScheduleRewords))
        {
            string strData = PlayerPrefs.GetString(ScheduleRewords);
            var datas = strData.Split(";");
            foreach (var item in datas)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    Model.ScheduleRewords.Add(int.Parse(item));
                }
            }
        }
    }

    /// <summary>
    /// 建造进度奖励是否领取
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool CheckScheduleReword(int id)
    {
        return Model.ScheduleRewords.Exists(p => p == id);
    }

    /// <summary>
    /// 获得可显示章节总数
    /// </summary>
    /// <returns></returns>
    public int GetChapterCount()
    {
        var chapter = Config.GetConfig<Config_ChapterBase>().GetShowChapterCfg();
        return chapter.Count;
    }

    /// <summary>
    /// 当前章节存在可建造的建筑
    /// </summary>
    /// <returns></returns>
    public bool CheckChapterCanBuilding()
    {
        var buildList = GetCanBuildItems(Model.curChapter);
        foreach (var buildCfg in buildList)
        {
            var bagCtrl = GameManager.Instance.GameBagControl;
            var Needglod = Util.AnalysisItem(buildCfg.Needglod);
            var id = Needglod.x;
            var count = Needglod.y;
            var checkLv = CheckBuildLeavl(buildCfg.Id);
            var chatCheck = GameManager.Instance.DialogueControl.CheckDialogue(buildCfg.Unlockchatid);
            var isEnough = bagCtrl.CheckItemIsEnough(id, count);
            return isEnough && checkLv && chatCheck;
        }
        return false;
    }

    public void SetChapterRewords(int chapter)
    {
        if(!Model.ChapterRewords.Exists(p => p == chapter))
        {
            Model.ChapterRewords.Add(chapter);
            var cfg = Config.GetConfig<Config_ChapterBase>().GetConfigById(chapter);
            var data = Util.AnalysisItem(cfg.Reward);
            GameManager.Instance.GameBagControl.UpdateItems(data.x, data.y);
            var itemCfg = Config.GetConfig<Config_ItemBase>().GetConfigById(data.x);
            UIManager.Instance.ShowPromptWindow($"获得{itemCfg.ItemName} * {data.y}");
            
            string strData = "";
            foreach (var item in Model.ChapterRewords)
            {
                strData += $"{item};";
            }
            PlayerPrefs.SetString("ChapterRewords", strData);
        }
        EventDispatchCenter.Instance.Dispatch(SDEvents.BUILD_REWORD);
    }

    public void AnalysisChapterPrefRewords()
    {
        if (PlayerPrefs.HasKey("ChapterRewords"))
        {
            string strData = PlayerPrefs.GetString("ChapterRewords");
            var datas = strData.Split(";");
            foreach (var item in datas)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    Model.ChapterRewords.Add(int.Parse(item));
                }
            }
        }
    }

    protected override void OnCloseControl() 
    { 
        EventDispatchCenter.Instance.UnRegistry(SDEvents.CHANGE_LEAVL, CheckAndAddChapter);

    }

}

