using AntNet;
using DG.Tweening;
using Framework;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoSingleton<GameManager>
{
    /// <summary>
    /// 配置是否加载完成
    /// </summary>
    public bool IsConfigLoad  = false;

    private WebSocketObject _webSocketObjectManager;
    public WebSocketObject GetWebSocketObjectManager()
    {
        var manager = Instance._webSocketObjectManager;
        Util.CheckAndLogError(manager, "[GameManager] WebSocketObject == null");
        return manager;
    }
    
    private HeartBeatManager _heartBeatManager;
    public HeartBeatManager GetHeartBeatManager() {
        var manager = Instance._heartBeatManager;
        Util.CheckAndLogError(manager, "[GameManager] HeartBeatManager == null");
        return manager;
    }

    private LogManager _logManager;
    public LogManager LogManager
    {
        get
        {
            if (ReferenceEquals(_logManager, null))
                _logManager = new LogManager();
            return _logManager;
        }
    }
    
    public void LoadConfig()
    {
        var configCoroutine = Config.instance.LoadConfigStep(() =>
        {
            IsConfigLoad = true;
        });
        StopCoroutine(configCoroutine);
        StartCoroutine(configCoroutine);
    }
    
    public async UniTask LoadConfigAsync()
    {
        IsConfigLoad = false;
        await Config.instance.LoadConfigStepAsync();
        IsConfigLoad = true;
    }

    private void OnHide()
    {
        Time.timeScale = 0; // 暂停游戏时间
        // 同时暂停所有音频
        AudioListener.pause = true;
    }
    private void OnShow()
    {
        Time.timeScale = 1; // 恢复游戏时间
        // 同时恢复所有音频
        AudioListener.pause = false;
    }
    
    /// <summary>
    /// 调用登录
    /// </summary>
    public void Launch()
    {
        InitGameControl();
    }

    public void PreloadFightStart(int levelId)
    {
        StartCoroutine(CurFightControl.PreloadFight(levelId));
    }

    
    public void FightStart(int levelId, int levelType)
    {
        string dungeonName;
        if (levelType == 2)
        {
            dungeonName = "无尽挑战";
            StartCoroutine(CurFightControl.FightEndlessStart());
        }
        else
        {
            dungeonName = "普通关卡";
            StartCoroutine(CurFightControl.FightStart(levelId));
        }

#if  WEIXINMINIGAME && !UNITY_EDITOR
        PlatformManager.Instance.GetPlatformAdapter<WeChatAdapter>().ReportCustomEvent("dungeon", new Dictionary<string, object>()
        {
            { "dungeon_Id", levelType },
            { "dungeon_name", dungeonName },
            { "check_id", levelId },
            { "check_name", $"第{levelId}关" },
            { "oper", 1 },
            { "start_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            { "game_time", 0 },
            { "map_id", 0 },
            { "progress", 0 },
        });
#endif
        
        return;
        // PlayerDataManager.instance.FightTempLoad(data =>
        // {
        //     if (data != null)
        //     {
        //         var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        //         if (!Util.IsCrossDayInEast8(data.SaveTime, unixTimestamp))
        //         {
        //             return;
        //         }   
        //     }
        //
        //     if (isEndless)
        //     {
        //         StartCoroutine(CurFightControl.FightEndlessStart());
        //     }
        //     else
        //     {
        //         StartCoroutine(CurFightControl.FightStart(levelId));
        //     }
        // });
    }

    public void Update()
    {
   
    }

    public void FixedUpdate()
    {
        TimerManager.instance.FixedUpdate();
        CurFightControl?.FixedUpdate();
    }

    #region 游戏控制器


    private FightController _curFightControl;
    public FightController CurFightControl
    {
        get
        {
            if (ReferenceEquals(_curFightControl, null))
            {
                _curFightControl = new FightController();
            }
            return _curFightControl;
        }
    }
    
    private EndlessControl _endlessControl;
    public EndlessControl EndlessControl
    {
        get
        {
            if (ReferenceEquals(_endlessControl, null))
            {
                _endlessControl = new EndlessControl();
            }
            return _endlessControl;
        }
    }
    
    /// <summary>
    /// 玩家信息
    /// </summary>
    private PlayerControl _playerControl = null;

    public PlayerControl PlayerControl
    {
        get
        {
            if (ReferenceEquals(_playerControl, null))
                _playerControl = new PlayerControl();
            return _playerControl;
        }
    }

    /// <summary>
    /// npc控制器
    /// </summary>
    private NpcControl _npcControl = null;

    public NpcControl NpcControl 
    {
        get { 
            if(ReferenceEquals(_npcControl, null))
                _npcControl = new NpcControl();
            return _npcControl;
            }
    }

    /// <summary>
    /// 章节控制器
    /// </summary>
    private ChapterControl chapterControl = null;
    public ChapterControl ChapterControl 
    {
        get 
        {
            if (ReferenceEquals(chapterControl, null))
                chapterControl = new ChapterControl();
            return chapterControl;
        }
    }

    /// <summary>
    /// npc外围对话控制器
    /// </summary>
    private DialogueControl _dialogueControl = null;

    public DialogueControl DialogueControl
    {
        get
        {
            if (ReferenceEquals(_dialogueControl, null))
                _dialogueControl = new DialogueControl();
            return _dialogueControl;
        }
    }

    /// <summary>
    /// 背包控制器
    /// </summary>
    private GameBagControl _gameBagControl = null;

    public GameBagControl GameBagControl
    {
        get
        {
            if (ReferenceEquals(_gameBagControl, null))
                _gameBagControl = new GameBagControl();
            return _gameBagControl;
        }
    }

    private PerformControl performControl = null;

    public PerformControl PerformControl
    {
        get {
            if (ReferenceEquals(performControl, null))
                performControl = new PerformControl();
            return performControl;
        }
    }
    
    private MusicControl _musicControl = null;
    public MusicControl MusicControl
    {
        
        get
        {
            if (ReferenceEquals(_musicControl, null))
                _musicControl = new MusicControl();
            return _musicControl;
        }
    }

    private StageControl _stageControl = null;
    public StageControl StageControl
    {

        get
        {
            if (ReferenceEquals(_stageControl, null))
                _stageControl = new StageControl();
            return _stageControl;
        }
    }

    /// <summary>
    /// 游戏控制器
    /// </summary>
    private readonly List<BaseControl> _gameControl = new();

    /// <summary>
    /// 初始化游戏控制器
    /// </summary>
    public void InitGameControl()
    {
        _gameControl.Add(PlayerControl);
        _gameControl.Add(NpcControl);
        _gameControl.Add(ChapterControl);
        _gameControl.Add(DialogueControl);
        _gameControl.Add(CurFightControl);
        _gameControl.Add(GameBagControl);
        _gameControl.Add(PerformControl);
        _gameControl.Add(MusicControl);
        _gameControl.Add(StageControl);
        _gameControl.Add(EndlessControl);
        _gameControl.ForEach((control) =>
        {
            control.OnInitialize();
        });
    }
    #endregion

    private void OnDestroy()
    {
        ResetControl();
        EventDispatchCenter.Instance.ClearAll(true);
        Config.Release();
        ResourceManagerNew.Release();
        TimerManager.Release();
    }

    public void ResetControl()
    {
        _gameControl.ForEach((control) =>
        {
            control.OnClear(false);
        });
    }
}