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
        this.npcsState = NpcState.Lock;
        this.order = 0;
        AnalysisPrefData();
    }

    public void SetPrefData()
    {
        string strData = "";
        strData += $"{NpcState};";
        strData += $"{order};";
        PlayerPrefs.SetString("TetrisBarClient_NpcData_" + Id, strData);
    }

    private void AnalysisPrefData()
    {
        if (PlayerPrefs.HasKey("TetrisBarClient_NpcData_" + id))
        {
            var strData = PlayerPrefs.GetString("TetrisBarClient_NpcData_" + id).Split(";");
            NpcState = Enum.Parse<NpcState>(strData[0]);
            order = int.Parse(strData[1]);
        }
        else
        {
            RefreshNpcType();
            SetPrefData();
        }
    }

    public void RefreshNpcType()
    {
        if (NpcState == NpcState.UnLock)
        {
            NpcState = NpcState.UnLock;
        }
        else
        {
            NpcState = NpcState.Lock;
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
    public int order;
}

