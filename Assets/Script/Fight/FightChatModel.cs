using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FightChatModel:BaseModel
{
    public const string FightChatDataKey = "FightChatDataKey_";
    public Dictionary<int, FightChatData> FightChats = new();
    public bool bInChat = false;
}

public class FightChatData
{
    /// <summary>
    /// npcId
    /// </summary>
    public int npcId;
    /// <summary>
    /// 历史聊天（只存开始id）
    /// </summary>
    public List<int> historyStartIds = new();
    /// <summary>
    /// 当前聊天id
    /// </summary>
    public int curId = 0;

    public FightChatData(int npcId)
    {
        this.npcId = npcId;
        AnalysisChatData();
    }

    private void AnalysisChatData()
    {
        if (PlayerPrefs.HasKey(FightChatModel.FightChatDataKey + npcId))
        {
            var strData = PlayerPrefs.GetString(FightChatModel.FightChatDataKey + npcId);
            var strList = strData.Split("#");
            foreach (var str in strList)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    historyStartIds.Add(int.Parse(str));
                }
            }
        }
    }

    /// <summary>
    /// 关卡结束后缓存
    /// </summary>
    public void SetChatPrefsData()
    {
        if (historyStartIds.Count > 0)
        {
            var strData = string.Empty;
            foreach (var item in historyStartIds)
            {
                strData += $"{item}#";
            }
            PlayerPrefs.SetString(FightChatModel.FightChatDataKey + npcId, strData);
        }
    }

    /// <summary>
    /// 进入下一对话
    /// </summary>
    /// <param name="id"></param>
    public void Next(int id)
    {
        curId = id;
        if (id != 0)
        {
            EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_FIGHT_CHAT_NEXT);
        }
    }
    
    
}

public class FightChatViewData
{
    public int npcId;
    public Action<int,int> CloseAction;
    public int pos;
}