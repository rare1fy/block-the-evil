using Pb;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class FightChatController
{
    private FightChatModel _model;
    public FightChatModel Model
    {
        get
        {
            if (_model == null)
                _model = new FightChatModel();
            return _model;
        }
    }

    public FightChatController()
    {
        Model.FightChats.Clear();
        var cfgs = Config.GetConfig<Config_NpcBase>().m_NpcBaseDic;
        foreach (var cfg in cfgs.Values)
        {
            Model.FightChats.TryAdd(cfg.Id, new FightChatData(cfg.Id));
        }
    }

    //检测触发
    public bool CheckNpcChat(int npcId, int order)
    {
        var level = GameManager.Instance.CurFightControl.LevelController.Model.LevelId;
        var ids = Config.GetConfig<Config_FightchatLibrary>().GetUnLockWorldToIds(npcId, level, order);
        foreach (var id in ids)
        {
            if (Model.FightChats.TryGetValue(npcId, out var fightChatData))
            {
                if (!fightChatData.historyStartIds.Exists(p => p == id))
                {
                    fightChatData.historyStartIds.Add(id);
                    fightChatData.curId = id;
                    Model.bInChat = true;
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 完成对局（胜利的时候）
    /// </summary>
    public void SetPrefsData()
    {
        foreach (var item in Model.FightChats.Values)
        {
            item.SetChatPrefsData();
        }
    }

    /// <summary>
    /// 获得当前聊天事件
    /// </summary>
    /// <param name="npcId"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public FightChatEventBase GetFightChatEven(int npcId)
    {
        if (Model.FightChats.TryGetValue(npcId, out var data))
        {
            if (data.curId == 0)
            {
                Debug.LogError("没有检测聊天，需要调用CheckNpcChat");
                return null;
            }
            else
            {
                var cfg = Config.GetConfig<Config_FightchatBase>().GetConfigById(data.curId);
                switch (cfg.DialogueType)
                {
                    case 1:
                        return new FightChatEventOne(npcId, cfg, data);
                    case 2:
                        return new FightChatEventTwo(npcId, cfg, data);
                    case 3:
                        return new FightChatEventThree(npcId, cfg, data);
                    case 4:
                        return new FightChatEventFour(npcId, cfg, data);
                    case 5:
                        return new FightChatEventFive(npcId, cfg, data);
                    default:
                        return new FightChatEventBase(npcId, cfg, data);
                }
            }
        }
        return null;
    }
    
}
