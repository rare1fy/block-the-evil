using Pb;

/// <summary>
/// 普通结束
/// </summary>
public class FightChatEventThree : FightChatEventBase
{
    public FightChatEventThree(int npcId, FightchatBase cfg, FightChatData data) : base(npcId, cfg, data)
    {
        IsEnd = true;
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
