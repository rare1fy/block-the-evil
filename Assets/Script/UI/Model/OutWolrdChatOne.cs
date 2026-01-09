using ICSharpCode.SharpZipLib;
using Pb;

public class OutWolrdChatOne : OutWolrdChatBase
{
    public OutWolrdChatOne(OutWord _outWord, string parameter = "", DialogueData data = null) : base(_outWord, parameter, data)
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

    public override bool IsNpc(out int npcId)
    {
        npcId = dialogueData.npcId;
        return true;
    }

    public override bool HasOperation(out float time)
    {
        time = 0f;
        return true;
    }
}
