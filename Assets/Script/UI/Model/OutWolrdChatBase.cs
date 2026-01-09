using Pb;


/// <summary>
/// 聊天事件基类
/// </summary>
public class OutWolrdChatBase
{
    public static int IdCount = 0;

    /// <summary>
    /// 配置表数据
    /// </summary>
    public OutWord outWord = null;
    /// <summary>
    /// 对话组
    /// </summary>
    protected DialogueData dialogueData = null;
    /// <summary>
    /// 额外参数
    /// </summary>
    public string Parameter = string.Empty;
    /// <summary>
    /// 对话唯一id（用于处理相同配置id的按钮等）
    /// </summary>
    public int sid;

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

    public OutWolrdChatBase(OutWord _outWord, string parameter = "", DialogueData dialogueData = null)
    {
        IdCount++;
        sid = IdCount + 10000 * dialogueData.npcId;
        outWord = _outWord;
        Parameter = parameter;
        this.dialogueData = dialogueData;
    }

    /// <summary>
    /// 是否为文本内容
    /// </summary>
    /// <param name="countent"></param>
    /// <returns></returns>
    public virtual bool IsTextCoutent(out string countent)
    {
        countent = Parameter;
        return false;
    }

    /// <summary>
    /// 是否为图片内容
    /// </summary>
    /// <param name="countent"></param>
    /// <returns></returns>
    public virtual bool IsImgCoutent(out string countent)
    {
        countent = Parameter;
        return false;
    }

    /// <summary>
    /// 是否为npc
    /// </summary>
    /// <param name="npcId"></param>
    /// <returns></returns>
    public virtual bool IsNpc(out int npcId)
    {
        npcId = dialogueData.npcId;
        return false;
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
    /// 按钮类型 是否可领取
    /// </summary>
    /// <returns></returns>
    public virtual bool CanGet()
    {
        return false;
    }

    /// <summary>
    /// 按钮1是否显示
    /// </summary>
    /// <returns></returns>
    public virtual bool isShowBtn1()
    {
        return outWord.Button1To != 0;
    }

    /// <summary>
    /// 按钮2是否显示
    /// </summary>
    /// <returns></returns>
    public virtual bool isShowBtn2()
    {
        return outWord.Button2To != 0;
    }

    /// <summary>
    /// 获得头像
    /// </summary>
    /// <returns></returns>
    public virtual string GetHead()
    {
        if (outWord.Typeid > 0)
        {
            var npcCfg = Config.GetConfig<Config_NpcBase>().GetConfigById(outWord.Typeid);
            return npcCfg.Img1;
        }
        else
        {
            return "Avatar_01";
        }
    }

    /// <summary>
    /// 点击按钮
    /// </summary>
    /// <param name="idx"></param>
    public virtual void OnClick(int idx)
    {
        OutWord cfg = Config.GetConfig<Config_OutWord>().GetConfigById(outWord.Next);
        switch (idx)
        {
            case 1:
                nextID = outWord.Button1To;
                break;
            case 2:
                nextID = outWord.Button2To;
                break;
            default:
                nextID = outWord.Button1To;
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
            var cfg = Config.GetConfig<Config_OutWord>().GetConfigById(nextID);
            dialogueData.NextChat(cfg);
            nextID = 0;
        }
        else
        {
            var cfg = Config.GetConfig<Config_OutWord>().GetConfigById(outWord.Next);
            dialogueData.NextChat(cfg);
        }
    }
}
