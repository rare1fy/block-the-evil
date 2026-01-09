using Microsoft.Win32.SafeHandles;
using Pb;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 剧情数据类
/// </summary>
public class PerformModel : BaseModel
{
    public const string PerformDataKey = "PerformData_Key";
    public List<int> historyIds = new();

    public PerformData performData;
    public void InitPerformModel()
    {
        AnalysisHistory();
    }

    private void AnalysisHistory()
    {
        if (PlayerPrefs.HasKey(PerformDataKey))
        {
            string strData = PlayerPrefs.GetString(PerformDataKey);
            var datas = strData.Split("#");
            foreach (var data in datas)
            {
                if (!string.IsNullOrEmpty(data))
                {
                    historyIds.Add(int.Parse(data));
                }
            }
            historyIds.Sort();
        }
    }

    private void SetHistory()
    {
        string strData = string.Empty;
        foreach (var item in historyIds)
        {
            strData += $"{item}#";
        }
        PlayerPrefs.SetString(PerformDataKey, strData);
    }

    public void AddHistory(int id)
    {
        historyIds.Add(id);
        SetHistory();
    }
}

/// <summary>
/// 剧目数据
/// </summary>
public class PerformData
{
    public int groupId;
    public List<PerformEventBase> guildPerforms = new();
    public PerformEventBase curPerform;
    public bool isOver = false;

    public PerformData(int groupId)
    {
        this.groupId = groupId;
        // var cfgs = Config.GetConfig<Config_GuildPerform>().m_GuildPerformDic;
        // foreach (var cfg in cfgs.Values)
        // {
        //     if (cfg.Group == groupId)
        //     {
        //         guildPerforms.Add(GetEvent(cfg));
        //     }
        // }
        // guildPerforms.Sort((a, b) => { return a.Id - b.Id; });
    }

    public void Start()
    {
        if (guildPerforms.Count > 0)
        {
            curPerform = guildPerforms.First();
            guildPerforms.RemoveAt(0);
            EventDispatchCenter.Instance.Dispatch(SDEvents.PERFORM_START, groupId);
        }
    }

    public void Next()
    {
        if (guildPerforms.Count > 0) 
        {
            curPerform = guildPerforms.First();
            guildPerforms.RemoveAt(0);
            EventDispatchCenter.Instance.Dispatch(SDEvents.PERFORM_NEXT, groupId);
        }
        else
        {
            isOver = true;
            GameManager.Instance.PerformControl.Model.AddHistory(groupId);
            EventDispatchCenter.Instance.Dispatch(SDEvents.PERFORM_OVER, groupId);
        }
    }

    public PerformEventBase GetEvent(GuildPerform guildPerform)
    {
        switch (guildPerform.Type)
        {
            case 1:
                return new PerformEventChat(guildPerform.Id, guildPerform, this);
            case 2:
                return new PerformEventAnimator(guildPerform.Id, guildPerform, this);
            case 3:
                return new PerformEventCutTo(guildPerform.Id, guildPerform, this);
            case 4:
                return new PerformEventFight(guildPerform.Id, guildPerform, this);
            case 5:
                return new PerformEventMusic(guildPerform.Id, guildPerform, this);
            default:
                return new PerformEventChat(guildPerform.Id, guildPerform, this);
        }
    }
}


