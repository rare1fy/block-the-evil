using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicControl : BaseControl
{
    public int PickMusicId = 0;
    public int MainMusicId = 0;
    //public int ClearBlockCount = 0;
    public bool isMain = false;
    public bool isCd = false;

    public List<int> UnlockMusicIds = new List<int>();
    public Dictionary<int, int> MusicBlockDic = new Dictionary<int, int>();
    public List<int> blockIds = new List<int>() { 1,2,3,4,5};
    protected override void OnInitControl()
    {
        //ClearBlockCount = PlayerDataManager.instance.PlayerData.ClearBlockCount;
        LaunchManager.Instance.RegisterSystemWaitForInit();
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_LOAD_PLAYERDATA_FINISH, InitMusicData);
    }

    protected override void OnCloseControl()
    {
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_LOAD_PLAYERDATA_FINISH, InitMusicData);
        PlayerDataManager.instance.MusicSave();
    }

    private void InitMusicData(object param)
    {
        if (param is PlayerData playerData)
        {
            PickMusicId = playerData.PickMusicId;
            MainMusicId = playerData.MainMusicId;
            MusicBlockDic = playerData.MusicBlockDic;
            UnlockMusicIds = playerData.UnlockMusicIds;
            InitMusicBlock();
            
            LaunchManager.Instance.MaskSystemReady();
        }
    }

    private void InitMusicBlock()
    {
        if (MusicBlockDic.Count == 0)
        {
            MusicBlockDic.TryAdd(1, 0);
            MusicBlockDic.TryAdd(2, 0);
            MusicBlockDic.TryAdd(3, 0);
            MusicBlockDic.TryAdd(4, 0);
            MusicBlockDic.TryAdd(5, 0);
            PlayerDataManager.instance.MusicSave();
        }
    }

    /// <summary>
    /// 播放选择的背景音乐
    /// </summary>
    public void PlayPickMainBGM(bool isMain = false)
    {
        var lv = GameManager.Instance.PlayerControl.PlayerModel.Level;
        string bgmName;
        this.isMain = isMain;
        if (isMain)
        {
            if (MainMusicId == 0 || lv == 0)
            {
                bgmName = "default_bgm";
            }
            else
            {
                bgmName = Config.GetConfig<Config_MusicPlayer>().GetConfigById(MainMusicId).Music;
            }
        }
        else
        {
            if (PickMusicId == 0 || lv == 0)
            {
                bgmName = "default_bgm";
            }
            else
            {
                bgmName = Config.GetConfig<Config_MusicPlayer>().GetConfigById(PickMusicId).Music;
            }
        }
        
        AudioManagerNew.Instance.FadeStopMusic(() =>
        {
            AudioManagerNew.Instance.PlayMusic(bgmName);
        });
    }

    public void SelectBGM(int id,bool isOnce = false, bool isMain = false)
    {
        bool isPlay = false;
        this.isMain = isMain;
  
        if (isMain && (MainMusicId != id || isOnce))
        {
            MainMusicId = id;
            isPlay = true;
        }
        else if(PickMusicId != id || isOnce)
        {
            PickMusicId = id;
            isPlay = true;
        }

        if (isPlay)
        {
            isCd = true;
            DOVirtual.DelayedCall(1.5f, () =>
            {
                isCd = false;
            });
            var bgmName = Config.GetConfig<Config_MusicPlayer>().GetConfigById(id).Music;
            AudioManagerNew.Instance.FadeStopMusic(() =>
            {
                AudioManagerNew.Instance.PlayMusic(bgmName);
            });
            EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_MUSIC_SELECT);
            PlayerDataManager.instance.MusicSave();
        }
    }

    public void AddBlockCount(List<BlockData> blockList)
    {
        foreach (var block in blockList) 
        {
            if (MusicBlockDic.ContainsKey(block.ColorType))
            {
                MusicBlockDic[block.ColorType] += 1;
            }
        }
        EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_MUSIC_PROGRESS_REFRESH);
    }
    
    public bool CheckUnLock(int id)
    {
        return UnlockMusicIds.Contains(id);
    }

    public string GetConsumeText(int id)
    {
        var cfg = Config.GetConfig<Config_MusicPlayer>().GetConfigById(id);
        var consume = cfg.Consume.Split("#");
        int consumeId = int.Parse(consume[0]);
        int consumeNum = int.Parse(consume[1]);
        string color = CheckCanUnLockMusic(id) ? "94fc48" : "ffffff";

        return $"<color=#{color}>{Util.FormatNumber(MusicBlockDic[consumeId])}</color>/{Util.FormatNumber(consumeNum)}";
    }

    public bool CheckCanUnLockMusic(int id)
    {
        var cfg = Config.GetConfig<Config_MusicPlayer>().GetConfigById(id);
        var consume = cfg.Consume.Split("#");
        int consumeId = int.Parse(consume[0]);
        int consumeNum = int.Parse(consume[1]);
        if (CheckUnLock(id))
            return false;
        if (MusicBlockDic.ContainsKey(consumeId) && MusicBlockDic[consumeId] >= consumeNum)
            return true;
        return false;
    }

    public void UnLockMusic(int id, bool isMain = false)
    {
        var cfg = Config.GetConfig<Config_MusicPlayer>().GetConfigById(id);
        var consume = cfg.Consume.Split("#");
        int consumeId = int.Parse(consume[0]);
        int consumeNum = int.Parse(consume[1]);
        UnlockMusicIds.Add(cfg.Id);
        if (MusicBlockDic.ContainsKey(consumeId)) 
        {
            MusicBlockDic[consumeId] -= consumeNum;
        }
        SelectBGM(id,isMain:isMain);
        PlayerDataManager.instance.MusicSave();
        EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_MUSIC_PROGRESS_REFRESH);
    }
    
    /// <summary>
    /// 通过Id直接解锁（对话解锁）
    /// </summary>
    /// <param name="index"></param>
    public void UnlockById(int index)
    {
        var cfg = Config.GetConfig<Config_MusicPlayer>().GetConfigById(index);
        if (cfg != null)
        {
            var bgmName = cfg.Music;
            UnlockMusicIds.Add(cfg.Id);
            PickMusicId = cfg.Id;
            PlayerDataManager.instance.MusicSave();
            AudioManagerNew.Instance.FadeStopMusic(() =>
            {
                AudioManagerNew.Instance.PlayMusic(bgmName);
            });
            EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_MUSIC_SELECT);
            EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_MUSIC_PROGRESS_REFRESH);
        }
    }

    public bool HasUnlockable()
    {
        var cfgs = Config.GetConfig<Config_MusicPlayer>().m_MusicPlayerDic;
        foreach (var cfg in cfgs)
        {
            var consume = cfg.Value.Consume.Split("#");
            var id = int.Parse(consume[0]);
            var num = int.Parse(consume[1]);
            if (MusicBlockDic.ContainsKey(id) && MusicBlockDic[id] >= num && !UnlockMusicIds.Contains(cfg.Value.Id))
                return true;
        }
        return false;
    }
    
    
    
}
