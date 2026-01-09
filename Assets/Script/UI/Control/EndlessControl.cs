using System;
using System.Collections.Generic;
using Pb;
using UnityEngine;
using Random = UnityEngine.Random;

public class EndlessControl : BaseControl
{
    /// <summary>
    /// 历史最高分
    /// </summary>
    public int HistoryHighScore { get; private set; }

    /// <summary>
    /// 历史通关
    /// </summary>
    public int PassEndlessLevelId { get; private set; } = 1;
    
    private long _lastFinishEndlessTimeStamp;
    
    protected override void OnInitControl() 
    {
        PassEndlessLevelId = PlayerPrefs.GetInt("HistoryPassEndlessFinialLevelId", 1);
        HistoryHighScore = PlayerPrefs.GetInt("EndlessHighScore", 0);
        var timestampStr = PlayerPrefs.GetString("lastFinishEndlessTime", 0.ToString());
        _lastFinishEndlessTimeStamp = long.TryParse(timestampStr, out var result) ? result : 0;
       
    }

    protected override void OnCloseControl()
    {
        
    }
    
    // 结束无尽模式
    public void EndlessFinish(LevelWujingModle endlessConfig)
    {
        var endlessId = endlessConfig.Id;
        var levelScore = GameManager.Instance.CurFightControl.LevelController.Model.LevelScore;
        if (levelScore > HistoryHighScore)
        {
            HistoryHighScore = levelScore;
            PlayerPrefs.SetInt("EndlessHighScore", levelScore);
        }
        if (endlessId > PassEndlessLevelId && endlessConfig.Npc <= levelScore) 
        {
            PassEndlessLevelId = endlessId;
            PlayerPrefs.SetInt("HistoryPassEndlessFinialLevelId", endlessId);
        }
    }

    public int GetEndlessLevelId()
    {
        if (IsEndNpc())
        {
            return PassEndlessLevelId;
        }
        var curCfg = Config.GetConfig<Config_LevelWujingModle>().GetConfigById(PassEndlessLevelId);
        var owned = GameManager.Instance.StageControl.CheckNpcOwned(curCfg.SpNpc);

        if (owned) //如果已经拥有就检测是不是同一天
        {
            var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (Util.IsCrossDayInEast8(_lastFinishEndlessTimeStamp, unixTimestamp))
            {
                return PassEndlessLevelId + 1;
            }
        }
        return PassEndlessLevelId;
    }
    
    public bool IsEndNpc()
    {
        return PassEndlessLevelId >= Config.GetConfig<Config_LevelWujingModle>().m_LevelWujingModleDic.Count;
    }

    public string GetNextDayTime()
    {
        TimeSpan timeLeft = Util.GetTimeUntilNextCrossDay(null);
        return $"{(int)timeLeft.TotalHours:00}时{timeLeft.Minutes:00}分";
    }

    public LevelWujingModle GetNextCfg()
    {
        int id = PassEndlessLevelId + 1;
        if (IsEndNpc())
        {
            id = PassEndlessLevelId;
        }
        return Config.GetConfig<Config_LevelWujingModle>().GetConfigById(id);
    }

    public int RandomGetLevelId()
    {
        var idList = new List<int>();
        foreach (var levelBaseCfg in Config.GetConfig<Config_LevelBase>().m_LevelBaseDic)
        {
            if (levelBaseCfg.Value.Id > 10 && levelBaseCfg.Value.Boss == 0)
            {
                idList.Add(levelBaseCfg.Value.Id);
            }
        }
        
        var index = Random.Range(0, idList.Count);
        return idList[index];
    }

    public void ChangeLastFinishEndlessTime()
    {
        _lastFinishEndlessTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        PlayerPrefs.SetString("lastFinishEndlessTime", _lastFinishEndlessTimeStamp.ToString());
    }
}