using Pb;

public class OutWolrdChatTwo : OutWolrdChatBase
{
    public OutWolrdChatTwo(OutWord _outWord, string parameter = "", DialogueData data = null) : base(_outWord, parameter, data)
    {
    }

    public override bool IsTextCoutent(out string countent)
    {
        countent = Parameter;
        return true;
    }

    public override bool HasOperation(out float time)
    {
        time = 1;
        return outWord.Button1To != 0;
    }
}
