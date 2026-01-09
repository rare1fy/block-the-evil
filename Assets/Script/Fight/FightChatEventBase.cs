using Pb;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightChatEventBase
{
    public int npcId;
    public FightchatBase cfg;
    public FightChatData fightChat;

    /// <summary>
    /// 是否已完成操作
    /// </summary>
    protected bool isOperationComplete = false;
    /// <summary>
    /// 是否已完成操作
    /// </summary>
    public bool IsOperationComplete
    {
        get { return isOperationComplete; }
        protected set { isOperationComplete = value; }
    }
    protected int nextID = 0;

    public bool IsEnd = false;

    public FightChatEventBase(int npcId, FightchatBase cfg, FightChatData fightChat)
    {
        this.npcId = npcId;
        this.cfg = cfg;
        this.fightChat = fightChat;
    }

    /// <summary>
    /// 是否存在操作（没有操作，等待时间后往下走）
    /// </summary>
    /// <returns></returns>
    public virtual bool HasOperation(out float time)
    {
        time = 1f;
        return false;
    }

    /// <summary>
    /// 点击按钮
    /// </summary>
    /// <param name="idx"></param>
    public virtual void OnClick(int idx)
    {
        switch (idx)
        {
            case 1:
                nextID = cfg.Button1To;
                break;
            case 2:
                nextID = cfg.Button2To;
                break;
            default:
                nextID = cfg.Button1To;
                break;
        }
        IsOperationComplete = true;
    }

    /// <summary>
    /// 按钮1是否显示
    /// </summary>
    /// <returns></returns>
    public virtual bool isShowBtn1()
    {
        return cfg.Button1To != 0;
    }

    /// <summary>
    /// 按钮2是否显示
    /// </summary>
    /// <returns></returns>
    public virtual bool isShowBtn2()
    {
        return cfg.Button2To != 0;
    }

    /// <summary>
    /// 没有点击事件，自动进入下一句
    /// </summary>
    /// <returns>是否结束对话</returns>
    public virtual void Exit()
    {
        if (nextID != 0)
        {
            fightChat.Next(nextID);
            nextID = 0;
        }
        else
        {
            fightChat.Next(cfg.Next);
        }
    }

}
