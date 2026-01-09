using Pb;

public class OutWolrdChatEleven : OutWolrdChatBase
{
    public OutWolrdChatEleven(OutWord _outWord, string parameter = "", DialogueData data = null) : base(_outWord, parameter, data)
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

    public override void Exit()
    {
        if (dialogueData.isNew)
        {
            //奖励
            if (!string.IsNullOrEmpty(outWord.Reward1))
            {
                var reword = Util.AnalysisItem(outWord.Reward1);
                GameManager.Instance.GameBagControl.UpdateItems(reword.x, reword.y);
                
                var itemCfg = Config.GetConfig<Config_ItemBase>().GetConfigById(reword.x);
                UIManager.Instance.ShowPromptWindow($"获得{itemCfg.ItemName} * {reword.y}");
            }
            if (!string.IsNullOrEmpty(outWord.GoodfeelReward))
            {
                var reword = Util.AnalysisItem(outWord.GoodfeelReward);
                GameManager.Instance.NpcControl.SetNpcFeel(reword.x, reword.y);
            }
        }
        base.Exit();
    }
}
