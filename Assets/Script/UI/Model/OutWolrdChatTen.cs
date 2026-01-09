using Pb;

public class OutWolrdChatTen : OutWolrdChatBase
{
    public OutWolrdChatTen(OutWord _outWord, string parameter = "", DialogueData data = null) : base(_outWord, parameter, data)
    {
    }

    public override bool IsNpc(out int npcId)
    {
        npcId = dialogueData.npcId;
        return true;
    }

    public override bool IsImgCoutent(out string countent)
    {
        countent = Parameter;
        return true;
    }
}
