using Pb;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 按钮选项事件
/// </summary>
public class FightChatEventTwo : FightChatEventBase
{
    public FightChatEventTwo(int npcId, FightchatBase cfg, FightChatData data) : base(npcId, cfg, data)
    {

    }

    /// <summary>
    /// 是否存在操作（没有操作，等待时间后往下走）
    /// </summary>
    /// <returns></returns>
    public override bool HasOperation(out float time)
    {
        time = 0f;
        return true;
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
    /// 没有点击事件，自动进入下一句
    /// </summary>
    public virtual void Exit()
    {
        if (nextID != 0)
        {
            var lastCfg = Config.GetConfig<Config_OutWord>().GetConfigById(nextID);
            nextID = 0;
        }
        else
        {
            var lastCfg = Config.GetConfig<Config_OutWord>().GetConfigById(cfg.Next);
        }
    }

}
