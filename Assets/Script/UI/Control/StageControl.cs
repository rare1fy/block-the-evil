
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class StageControl : BaseControl
{
    /// <summary>
    /// 预加入
    /// </summary>
    public List<int> PreAddition = new List<int>();
    /// <summary>
    /// 普通npc
    /// </summary>
    public List<int> normalJoined = new List<int>();
    /// <summary>
    /// 特殊npc
    /// </summary>
    public List<int> specialJoined = new List<int>();
    /// <summary>
    /// 加入的全部npc
    /// </summary>
    public List<int> joined = new List<int>();

    protected override void OnInitControl()
    {
        LaunchManager.Instance.RegisterSystemWaitForInit();
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_LOAD_PLAYERDATA_FINISH, InitStageData);
    }
    
    protected override void OnCloseControl()
    {
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_LOAD_PLAYERDATA_FINISH, InitStageData);
    }


    private void InitStageData(object param)
    {
        if (param is PlayerData playerData)
        {
            PreAddition = playerData.StagePreAddition;
            specialJoined = playerData.StageSpecialJoined;
            normalJoined = playerData.StageNormalJoined;
            joined.Clear();
            joined.AddRange(specialJoined);
            joined.AddRange(normalJoined);
            
            LaunchManager.Instance.MaskSystemReady();
        }
    }


    public void AddPreAddition(List<int> npcIds)
    {
        PreAddition.AddRange(npcIds);
        PlayerDataManager.instance.StageSave();
    }

    public void JoinStage()
    {
        foreach (var item in PreAddition)
        {
            var npcCfg = Config.GetConfig<Config_NpcBase>().GetConfigById(item);
            if (npcCfg.Special == 0) 
            { 
                normalJoined.Add(item);
            }
            else
            {
                specialJoined.Insert(0, item);
            }
        }
        var add = PreAddition.Count;
        PreAddition.Clear();
        joined.Clear();
        joined.AddRange(specialJoined);
        joined.AddRange(normalJoined);
        PlayerDataManager.instance.StageSave();
        EventDispatchCenter.Instance.Dispatch(SDEvents.STAGE_NUM_REFRESH, add);
    }

    public int GetAllNpcCount()
    {
        return joined.Count + PreAddition.Count;
    }


    public bool CheckNpcOwned(int npcId)
    {
        if (PreAddition.Contains(npcId))
        {
            return true;
        }

        if (joined.Contains(npcId))
        {
            return true;
        }

        return false;
    }
}
