
using System;
using System.Linq;

public class PerformControl : BaseControl
{
    private PerformModel performModel;

    public PerformModel Model
    {
        get 
        {
            if (ReferenceEquals(performModel, null))
                performModel = new PerformModel();
            return performModel;
        }
        private set { performModel = value; }
    }

    protected override void OnInitControl()
    {
        Model.InitPerformModel();
        //EventDispatchCenter.Instance.Registry(SDEvents.PERFORM_OVER, PerformOver);

    }

    protected override void OnCloseControl()
    {
        //EventDispatchCenter.Instance.UnRegistry(SDEvents.PERFORM_OVER, PerformOver);

    }

    //private void PerformOver(object obj)
    //{
    //    UIManager.Instance.ShowUI("MainWindow");
    //}

    public bool CheckHasPerform()
    {
        var guildGroupCfgs = Config.GetConfig<Config_GuildGroup>().m_GuildGroupDic.Values.ToList();
        guildGroupCfgs.Sort((a, b) => { return a.Id - b.Id; });
        var level = GameManager.Instance.PlayerControl.PlayerModel.Level;

        foreach (var cfg in guildGroupCfgs)
        {
            if (Model.historyIds.Exists(p => cfg.Id == p))
                continue;
            if(level >= cfg.Level)
            {
                Model.performData = new PerformData(cfg.Id);
                Model.performData.Start();
                return true;
            }
        }
        return false;
    }

    public void SetAwaitEnd()
    {
        if(Model.performData!= null && Model.performData.curPerform != null)
            Model.performData.curPerform.awaitEnd = true;
    }
}

