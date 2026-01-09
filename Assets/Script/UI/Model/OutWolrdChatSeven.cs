using Pb;

public class OutWolrdChatSeven : OutWolrdChatBase
{
    public OutWolrdChatSeven(OutWord _outWord, string parameter = "", DialogueData data = null) : base(_outWord, parameter, data)
    {
    }

    /// <summary>
    /// 是否为文本内容
    /// </summary>
    /// <param name="countent"></param>
    /// <returns></returns>
    public override bool IsTextCoutent(out string countent)
    {
        countent = Parameter;
        return true;
    }
}
