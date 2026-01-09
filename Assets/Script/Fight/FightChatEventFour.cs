using Pb;

/// <summary>
/// 选项结束
/// </summary>
public class FightChatEventFour : FightChatEventBase
{
    public FightChatEventFour(int npcId, FightchatBase cfg, FightChatData data) : base(npcId, cfg, data)
    {
        IsEnd = true;
    }

    public override bool HasOperation(out float time)
    {
        time = 0f;
        return true;
    }

    public override bool isShowBtn1()
    {
        return !string.IsNullOrEmpty(cfg.Button1);
    }
    public override bool isShowBtn2()
    {
        return !string.IsNullOrEmpty(cfg.Button2);
    }

    public override void OnClick(int idx)
    {
        IsOperationComplete = true;
    }

    public override void Exit()
    {
        fightChat.Next(0);
        if (GameManager.Instance.CurFightControl.ChatController != null)
        {
            GameManager.Instance.CurFightControl.ChatController.Model.bInChat = false;
        }

    }
}
