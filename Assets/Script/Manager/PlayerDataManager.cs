using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 玩家数据结构
/// </summary>
[Serializable]
public class PlayerData
{
    public int HighestLevel;

    public int PickMusicId = 0;
    public int MainMusicId = 0;
    public List<int> UnlockMusicIds = new List<int>();
    public Dictionary<int, int> MusicBlockDic = new();

    #region 舞台数据

    public List<int> StagePreAddition = new();
    public List<int> StageNormalJoined = new();
    public List<int> StageSpecialJoined = new();

    #endregion

    #region 体力

    public int Stamina;
    public long LastStaminaTime;
    public long OpenTime;
    public int DailyStaminaClaimCount;
    public int LastStaminaClaimPeriodKey;

    #endregion

    #region 设置相关

    public float VolumeBlend = 1;
    public bool BShake = true;

    #endregion
}

/// <summary>
/// 玩家数据管理器
/// </summary>
public class PlayerDataManager : Singleton<PlayerDataManager>
{
    public PlayerData PlayerData
    {
        get
        {
            if (_playerData == null)
            {
                _playerData = new PlayerData();
                Debug.LogError("PlayerData为空!!!");
            }
            return _playerData;
        }
    }
    private PlayerData _playerData;

    private string filePath;

    public override void Init() { }

    #region 游戏内保存接口

    public void MusicSave()
    {
        _playerData.PickMusicId = GameManager.Instance.MusicControl.PickMusicId;
        _playerData.MainMusicId = GameManager.Instance.MusicControl.MainMusicId;
        _playerData.MusicBlockDic = GameManager.Instance.MusicControl.MusicBlockDic;
        _playerData.UnlockMusicIds = GameManager.Instance.MusicControl.UnlockMusicIds;
        SaveData();
    }

    public void StageSave()
    {
        _playerData.StageSpecialJoined = GameManager.Instance.StageControl.specialJoined;
        _playerData.StagePreAddition = GameManager.Instance.StageControl.PreAddition;
        _playerData.StageNormalJoined = GameManager.Instance.StageControl.normalJoined;
        SaveData();
    }

    public void StaminaSave()
    {
        _playerData.Stamina = GameManager.Instance.PlayerControl.PlayerModel.Stamina;
        _playerData.LastStaminaTime = GameManager.Instance.PlayerControl.PlayerModel.LastStaminaTime;
        _playerData.OpenTime = GameManager.Instance.PlayerControl.PlayerModel.OpenTime;
        _playerData.DailyStaminaClaimCount = GameManager.Instance.PlayerControl.PlayerModel.DailyStaminaClaimCount;
        _playerData.LastStaminaClaimPeriodKey = GameManager.Instance.PlayerControl.PlayerModel.LastStaminaClaimPeriodKey;
        SaveData();
    }

    public void SettingSave()
    {
        SaveData();
    }
    

    #endregion

    #region 存档通用方法

    private void SaveData()
    {
        if (_playerData == null)
        {
            Debug.LogWarning("尝试保存 null 对象，忽略");
            return;
        }
        PlatformManager.Instance.SaveData(PlayerData);
    }

    public void LoadData()
    {
        PlatformManager.Instance.LoadData((PlayerData data) =>
        {
            _playerData = data ?? new PlayerData();
            EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_LOAD_PLAYERDATA_FINISH, _playerData, false);
        });
    }
    
    
    #endregion

#if UNITY_EDITOR
    public void Clear()
    {
        var path = Path.Combine(Application.dataPath, "Res/PlayerData", "Data.json");
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("存档已清除: " + path);
        }
    }
#endif

    public override void Dispose()
    {
        SaveData();
    }
}
