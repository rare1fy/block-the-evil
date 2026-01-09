
using System.Collections.Generic;
using WeChatWASM;

public class LogManager
{
    public void Log_AD(int adType, bool isFinish)
    {
        var f = isFinish ? 1 : 0;
        
#if WEIXINMINIGAME && !UNITY_EDITOR
        PlatformManager.Instance.GetPlatformAdapter<WeChatAdapter>().WxSDK.SetEvent("ad", new Dictionary<string, string>()
        {
            {"adtype", adType.ToString()},
            {"ad_watch", f.ToString()}
        });
#endif
    }

    /// <summary>
    /// 登录
    /// </summary>
    public void Log_Login()
    {
        var moneyCount = GameManager.Instance.GameBagControl.GetItemNumberById(GameBagModel.GOLD);
        var npcCount = GameManager.Instance.StageControl.GetAllNpcCount();
        var endlessLeveId = GameManager.Instance.EndlessControl.PassEndlessLevelId;
        var stamina = GameManager.Instance.PlayerControl.PlayerModel.Stamina;
#if WEIXINMINIGAME && !UNITY_EDITOR
        PlatformManager.Instance.GetPlatformAdapter<WeChatAdapter>().WxSDK.SetEvent("login", new Dictionary<string, string>()
        {
            {"npccount", npcCount.ToString()},
            {"money", moneyCount.ToString()},
            {"endless_levelid", endlessLeveId.ToString()},
            {"stamina", stamina.ToString()}
        });
#endif
    }

    public void Log_GameStart(int levelId, bool isEndless)
    {
        var modelId = isEndless ? 1 : 0;
#if WEIXINMINIGAME && !UNITY_EDITOR
        PlatformManager.Instance.GetPlatformAdapter<WeChatAdapter>().WxSDK.SetEvent("game_start", new Dictionary<string, string>()
        {
            {"levelid", levelId.ToString()},
            {"modelid", modelId.ToString()}
        });
#endif
    }

    public void Log_GameDead(int levelId, bool isEndless)
    {
        var modelId = isEndless ? 1 : 0;
#if WEIXINMINIGAME && !UNITY_EDITOR
        PlatformManager.Instance.GetPlatformAdapter<WeChatAdapter>().WxSDK.SetEvent("game_dead", new Dictionary<string, string>()
        {
            {"levelid", levelId.ToString()},
            {"modelid", modelId.ToString()}
        });
#endif
    }


    public void Log_GameRevive(int levelId, int reviveTimes, bool isEndless)
    {
        var modelId = isEndless ? 1 : 0;
#if WEIXINMINIGAME && !UNITY_EDITOR
        PlatformManager.Instance.GetPlatformAdapter<WeChatAdapter>().WxSDK.SetEvent("game_revive", new Dictionary<string, string>()
        {
            {"levelid", levelId.ToString()},
            {"modelid", modelId.ToString()},
            {"round_revivetims", reviveTimes.ToString()}
        });
#endif
    }

    public void Log_GameBuyProp(int levelId, int propId, int buyType, bool isEndless)
    {
        var modelId = isEndless ? 1 : 0;
#if WEIXINMINIGAME && !UNITY_EDITOR
        PlatformManager.Instance.GetPlatformAdapter<WeChatAdapter>().WxSDK.SetEvent("game_buyprop", new Dictionary<string, string>()
        {
            {"levelid", levelId.ToString()},
            {"buytype", buyType.ToString()},
            {"modelid", modelId.ToString()},
            {"propid", propId.ToString()}
        });
#endif
    }
    
    
    public void Log_GameUseProp(int levelId, int propId, bool isEndless)
    {
        var modelId = isEndless ? 1 : 0;
#if WEIXINMINIGAME && !UNITY_EDITOR
        PlatformManager.Instance.GetPlatformAdapter<WeChatAdapter>().WxSDK.SetEvent("game_useprop", new Dictionary<string, string>()
        {
            {"levelid", levelId.ToString()},
            {"modelid", modelId.ToString()},
            {"propid", propId.ToString()}
        });
#endif
    }


    public void Log_GameFinish(int levelId, int score, bool isWin, bool isEndless)
    {
        var finishType = isWin ? 1 : 0;
        var modelId = isEndless ? 1 : 0;
        var npcCount = GameManager.Instance.StageControl.GetAllNpcCount();
        var endlessLeveId = GameManager.Instance.EndlessControl.PassEndlessLevelId;
        var stamina = GameManager.Instance.PlayerControl.PlayerModel.Stamina;
#if WEIXINMINIGAME && !UNITY_EDITOR
        PlatformManager.Instance.GetPlatformAdapter<WeChatAdapter>().WxSDK.SetEvent("game_finish", new Dictionary<string, string>()
        {
            {"levelid", levelId.ToString()},
            {"modelid", modelId.ToString()},
            {"fight_win", finishType.ToString()},
            {"fight_score", score.ToString()},
            {"npccount", npcCount.ToString()},
            {"endless_levelid", endlessLeveId.ToString()},
            {"stamina", stamina.ToString()}
        });
#endif
    }
}
