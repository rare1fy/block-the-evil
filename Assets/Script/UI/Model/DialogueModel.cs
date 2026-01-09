using Pb;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

/// <summary>
/// Npc数据
/// </summary>
public class DialogueModel : BaseModel
{
    /// <summary>
    /// NPC执行过的聊天数据存储
    /// </summary>
    public const string DialogSaveKey = "TetrisBarClient_SaveKey_";

    /// <summary>
    /// npc对话数据
    /// </summary>
    public Dictionary<int, DialogueData> DialogueDatas = new();
}

/// <summary>
/// npc对话信息
/// </summary>
public class DialogueData
{
    /// <summary>
    /// npcId
    /// </summary>
    public int npcId;

    /// <summary>
    /// 聊天列表（包括当前聊天）
    /// </summary>
    public List<OutWolrdChatBase> dialogueItems = new();

    /// <summary>
    /// 当前聊天事件数据
    /// </summary>
    public OutWolrdChatBase CurEventChatBase = null;

    /// <summary>
    /// 是否存在新聊天
    /// </summary>
    public bool isNew = false;

    public DialogueData(int id)
    {
        npcId = id;
        //解析历史聊天
        //dialogueItems = ReferenceEquals(dialogueItems, null) ? new(count) : dialogueItems;
        if (PlayerPrefs.HasKey(DialogueModel.DialogSaveKey + npcId))
        {
            var chatList = GetHistoryChatData(PlayerPrefs.GetString(DialogueModel.DialogSaveKey + npcId));
            if (chatList.Count > 0)
            {
                foreach (var item in chatList)
                {
                    var config = Config.GetConfig<Config_OutWord>().GetConfigById(item.value1);
                    var sign = GetCurWorldType(config);
                    dialogueItems.Add(sign);
                }
                CurEventChatBase = dialogueItems.Last();
            }
        }
        //刷新状态和当前聊天事件
        RefreshInfo();
        ////好感等级
        //var npcControl = GameManager.Instance.NpcControl; 
        //FeelLv = npcControl.GetNpcFeelLevel(npcId);
        //if (dialogueItems.Count == 0)
        //{
        //    isNew = true;
        //    //关卡等级
        //    var playerModel = GameManager.Instance.PlayerControl.PlayerModel;
        //    var Leavl = playerModel.Leavl;
        //    var outWorldId = Config.GetConfig<Config_OutworldLibrary>().GetOutWorldIdByConfig(npcId, Leavl, FeelLv);
        //    Debug.LogError("NPCID====" + npcId + " Leavl====" + Leavl + " 好感度等级===" + FeelLv);
        //    if (outWorldId == 0)
        //    {
        //        Debug.LogError($"outworld_library配置表配置错误，npcId为{npcId},请查证！！！");
        //        return;
        //    }

        //    var config = Config.GetConfig<Config_OutWord>().GetConfigById(outWorldId);
        //    var curEventChatBase = GetCurWorldType(config);

        //    Debug.LogError("当前的配置表ID========" + outWorldId + " 配置ID=====" + config.DialogueType
        //        + " 聊天数量===" + count + " 聊天数量===" + dialogueItems.Count);

        //    CurEventChatBase = curEventChatBase;
        //}
        //else
        //{
        //    isNew = false;
        //    CurEventChatBase = dialogueItems.Last();

        //    Debug.LogError("AAAAAA当前的配置表ID========" + CurEventChatBase.outWord.Id 
        //        + " 聊天数量===" + dialogueItems.Count);
        //}
    }

    /// <summary>
    /// 解析本地缓存
    /// </summary>
    /// <param name="strDatas"></param>
    /// <returns></returns>
    private List<Tuple<int,string>> GetHistoryChatData(string strDatas)
    {
        if (string.IsNullOrEmpty(strDatas))
        {
            return null;
        }
        var stringSplit = strDatas.Split(';');
        var Length = stringSplit.Length;
        var chatList = new List<Tuple<int, string>>(Length);
        for (var i = 0; i < Length; i++)
        {
            var signSplit = stringSplit[i].Split('#');
            if (signSplit.Length >= 2)
            {
                Tuple<int, string> cahtClass = new (int.Parse(signSplit[0]), signSplit[1]);
                chatList.Add(cahtClass);
            }
        }
        return chatList;
    }

    public void SetHistoryChatData()
    {
        string strData = "";
        foreach (var item in dialogueItems)
        {
            strData += $"{item.outWord.Id}#";
            strData += $"{item.Parameter}";
            strData += ";";
        }
        PlayerPrefs.SetString(DialogueModel.DialogSaveKey + npcId, strData);
    }

    public void SetCurrChatData()
    {
        string strData = "";
        strData += $"{CurEventChatBase.outWord.Id}#";
        strData += $"{CurEventChatBase.Parameter}";
        strData += ";";

        PlayerPrefs.SetString(DialogueModel.DialogSaveKey + npcId, strData);
    }

    public void RefreshInfo()
    {
        var feel = GameManager.Instance.NpcControl.GetNpcFeelLevel(npcId);
        var leavl = GameManager.Instance.PlayerControl.PlayerModel.Level;
        var outWordList = Config.GetConfig<Config_OutworldLibrary>().GetOutWorldList(npcId, leavl, feel);
        isNew = false;

        //当前有对话未完成
        if (CurEventChatBase != null 
            && CurEventChatBase.outWord.DialogueType != 8 
            && CurEventChatBase.outWord.DialogueType != 9
            && CurEventChatBase.outWord.DialogueType != 11)
        {
            isNew = true;
            Debug.LogError($" isNew 1 {CurEventChatBase.outWord.DialogueType}");
            return;
        }

        foreach (var id in outWordList)
        {
            if (dialogueItems.Exists(p=>p.outWord.Id == id))
            {
                continue;
            }
            isNew = true;
            var cfg = Config.GetConfig<Config_OutWord>().GetConfigById(id);
            CurEventChatBase = GetCurWorldType(cfg);
            dialogueItems.Add(CurEventChatBase);
            Debug.LogError($" isNew 2 {CurEventChatBase.outWord.DialogueType}");
            //EventDispatchCenter.Instance.Dispatch(SDEvents.DIALOGUE_RED_REFRESH, npcId);
            return;
        }
        if(dialogueItems.Count > 0)
            CurEventChatBase = dialogueItems.Last();
    }

    public void NextChat(OutWord nextCfg = null)
    {
        if (nextCfg != null)
        {
            CurEventChatBase = GetCurWorldType(nextCfg);
            dialogueItems.Add(CurEventChatBase);
            EventDispatchCenter.Instance.Dispatch(SDEvents.DIALOGUE_NEXT, npcId);
        }
        else
        {
            RefreshInfo();
            EventDispatchCenter.Instance.Dispatch(SDEvents.DIALOGUE_END, npcId);
            //EventDispatchCenter.Instance.Dispatch(SDEvents.DIALOGUE_RED_REFRESH, npcId);
        }
        SetHistoryChatData();
    }

    /// <summary>
    /// 获取当前世界类型
    /// </summary>
    /// <param name="dialogueType"></param>
    /// <param name="outWorldId"></param>
    /// <returns></returns>
    public OutWolrdChatBase GetCurWorldType(OutWord config) 
    {
        OutWolrdChatBase curEventChatBase = null;
        switch (config.DialogueType)
        {
            case 1://npc对话
                curEventChatBase = new OutWolrdChatOne(config, config.Txt1, this);
                break;
            case 2://玩家对话
                curEventChatBase = new OutWolrdChatTwo(config, config.Txt1, this);
                break;
            case 3://npc按钮
                curEventChatBase = new OutWolrdChatThree(config, "flase", this);
                break;
            case 4://npc来电
                curEventChatBase = new OutWolrdChatFour(config, config.Txt1, this);
                break;
            case 5://礼物
                curEventChatBase = new OutWolrdChatFive(config , config.Txt1, this);
                break;
            case 6://npc连续
                curEventChatBase = new OutWolrdChatSix(config, config.Txt1, this);
                break;
            case 7://玩家连续
                curEventChatBase = new OutWolrdChatSeven(config , config.Txt1, this);
                break;
            case 8://npc结束
                curEventChatBase = new OutWolrdChatEight(config , config.Txt1, this);
                break;
            case 9://通话时长只存储秒数
                curEventChatBase = new OutWolrdChatNine(config, string.Empty, this);
                break;
            case 10://npc图片
                curEventChatBase = new OutWolrdChatTen(config, config.Txt1, this);
                break;
            case 11://玩家结束
                curEventChatBase = new OutWolrdChatEleven(config, config.Txt1, this);
                break;
            default:
                curEventChatBase = new OutWolrdChatOne(config, config.Txt1, this);
                break;
        }
        return curEventChatBase;
    }
}