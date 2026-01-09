using Pb;

public class OutWolrdChatFive : OutWolrdChatBase
{
    public OutWolrdChatFive(OutWord _outWord, string parameter = "", DialogueData data = null) : base(_outWord, parameter, data)
    {
    }

    public override bool IsImgCoutent(out string countent)
    {
        countent = Parameter;
        return true;
    }
}
