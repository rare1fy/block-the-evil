using Pb;

public class OutWolrdChatSix : OutWolrdChatBase
{
    public OutWolrdChatSix(OutWord _outWord, string parameter = "", DialogueData data = null) : base(_outWord, parameter, data)
    {
    }

    public override bool IsNpc(out int npcId)
    {
        npcId = dialogueData.npcId;
        return true;
    }

    public override bool IsTextCoutent(out string countent)
    {
        countent = Parameter;
        return true;
    }
}
