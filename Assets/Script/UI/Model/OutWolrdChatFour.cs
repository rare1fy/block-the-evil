using Pb;

public class OutWolrdChatFour : OutWolrdChatBase
{
    public OutWolrdChatFour(OutWord _outWord, string parameter = "", DialogueData data = null) : base(_outWord, parameter, data)
    {
    }

    public override bool IsNpc(out int npcId)
    {
        npcId = dialogueData.npcId;
        return true;
    }

    public override void OnClick(int idx)
    {
        Parameter = "true";
        //打开电话界面
    }

    public override bool HasOperation(out float time)
    {
        time = 0f;
        return true;
    }

    public void EndCall()
    {
        isOperationComplete = true;
    }

    /// <summary>
    /// 按钮类型 是否可领取
    /// </summary>
    /// <returns></returns>
    public override bool CanGet()
    {
        return Parameter == "true"; ;
    }
}
