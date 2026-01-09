using Pb;

public class OutWolrdChatThree : OutWolrdChatBase
{
    public OutWolrdChatThree(OutWord _outWord, string parameter = "", DialogueData data = null) : base(_outWord, parameter, data)
    {
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

    public override void OnClick(int idx)
    {
        if (idx == 1 && Parameter != "true")
        {
            //奖励在这里
            Parameter = "true";
            EventDispatchCenter.Instance.Dispatch("DIALOGUE_CLICK_BTN", sid);

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
        base.OnClick(idx);
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
