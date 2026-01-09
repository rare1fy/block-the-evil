using Pb;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Npc数据
/// </summary>
public class NpcModel : BaseModel
{
    public Dictionary<int, NpcData> NpcDic = new();
}

public enum NpcState
{
    Lock,//未解锁
    Openable,//可开启
    UnLock,//已解锁
}

public class NpcData
{
    private int id;
    public int Id 
    { 
        get { return id; }
        private set { }
    }
    //好感等级
    private int feelLv;
    public int FeelLv
    {
        get { return feelLv; }
        private set { feelLv = value; }
    }

    private int feelExp;
    //好感经验
    public int FeelExp
    {
        get { return feelExp; }
        private set { feelExp = value; }
    }

    private NpcState npcsState;
    public NpcState NpcState
    {
        get { return npcsState; }
        private set { npcsState = value; }
    }
    /// <summary>
    /// 订单数
    /// </summary>
    private int order;
    public int Order
    {
        get { return order; }
        private set { order = value; }
    }

    public NpcData(int id)
    {
        this.id = id;
        this.feelLv = 1;
        this.feelExp = 0;
        this.npcsState = NpcState.Lock;
        this.order = 0;
        AnalysisPrefData();
    }

    public void SetPrefData()
    {
        string strData = "";
        strData += $"{feelLv};";
        strData += $"{feelExp};";
        strData += $"{NpcState};";
        strData += $"{order};";
        PlayerPrefs.SetString("TetrisBarClient_NpcData_" + Id, strData);
    }

    private void AnalysisPrefData()
    {
        if (PlayerPrefs.HasKey("TetrisBarClient_NpcData_" + id))
        {
            var strData = PlayerPrefs.GetString("TetrisBarClient_NpcData_" + id).Split(";");
            feelLv = int.Parse(strData[0]);
            feelExp = int.Parse(strData[1]);
            NpcState = Enum.Parse<NpcState>(strData[2]);
            order = int.Parse(strData[3]);
        }
        else
        {
            RefreshNpcType();
            SetPrefData();
        }
        //Debug.LogError($"npcId = {id}; order = {order}");
    }

    /// <summary>
    /// 增加经验
    /// </summary>
    /// <param name="exp"></param>
    /// <param name="bStore">立即缓存</param>
    public void AddExp(int exp)
    {
        feelExp += exp;
        while (true)
        {
            var cfg = Config.GetConfig<Config_GoodfeelLv>().GetConfigById(feelLv);
            if (feelExp >= cfg.Exp)
            {
                if (cfg.Max == 1)
                {
                    feelExp = cfg.Exp;
                    break;
                }
                feelLv++;
                feelExp -= cfg.Exp;
            }
            else
            {
                break;
            }
        }
        EventDispatchCenter.Instance.Dispatch(SDEvents.CHAGE_NPC_FEEL, new Tuple<int,int>(id, exp));
    }

    public void RefreshNpcType()
    {
        var lastType = npcsState;
        var playerModel = GameManager.Instance.PlayerControl.PlayerModel;
        var cfg = Config.GetConfig<Config_NpcBase>().GetConfigById(id);
        //var bOrder = order >= cfg.Unlockorderneed;
        //var bLevel = playerModel.Level >= cfg.Unlocklevelneed;

        if (NpcState == NpcState.UnLock)
        {
            NpcState = NpcState.UnLock;

        }
        else
        {
            NpcState = NpcState.Lock;

            //if (bOrder && bLevel)
            //{
            //    NpcState = NpcState.Openable;
            //}
            //else
            //{
            //    NpcState = NpcState.Lock;
            //}
        }
        EventDispatchCenter.Instance.Dispatch(SDEvents.CHANGE_NPC_STATE, id);
    }

    public void ChangeNpcType(NpcState npcType)
    {
        this.npcsState = npcType;

        EventDispatchCenter.Instance.Dispatch(SDEvents.CHANGE_NPC_STATE, id);
    }

    public void AddNpcOrder(int order)
    {
        this.order += order;

        RefreshNpcType();
    }
}

/// <summary>
/// 用于战斗结束返回外围数据
/// </summary>
public class NpcInfo
{
    public int id;
    public int exp;
    public int order;
}

