using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class NpcControl : BaseControl
{

    private NpcModel _model;
    public NpcModel Model
    {
        get
        {
            if (_model == null)
                _model = new NpcModel();
            return _model;
        }
    }

    protected override void OnInitControl() 
    {
        //EventDispatchCenter.Instance.Registry(SDEvents.CHANGE_LEAVL, RefreshAllNpcState);

        Model.NpcDic.Clear();
        var configs = Config.GetConfig<Config_NpcBase>().m_NpcBaseDic;
        foreach (var item in configs)
        {
            var cfgId = item.Value.Id;
            var data = new NpcData(cfgId);
            Model.NpcDic.TryAdd(cfgId, data);
        }
    }

    protected override void OnCloseControl() 
    {
        //EventDispatchCenter.Instance.UnRegistry(SDEvents.CHANGE_LEAVL, RefreshAllNpcState);

    }

    public NpcData GetNpcData(int id)
    {
        if (Model.NpcDic.TryGetValue(id, out var data))
        {
            return data;
        }
        return null;
    }

    /// <summary>
    /// 获得解锁的npc id
    /// </summary>
    /// <returns></returns>
    public List<int> GetUnLockNpcId()
    {
        var ret = new List<int>();
        foreach (var item in Model.NpcDic)
        {
            if (item.Value.NpcState == NpcState.UnLock)
            {
                ret.Add(item.Key);
            }
        }
        return ret;
    }

    public int GetMaxExp(int lv, out bool isMax)
    {
        var cfg = Config.GetConfig<Config_GoodfeelLv>().GetConfigById(lv);
        isMax = cfg.Max == 1;
        return cfg.Exp;
    }

    /// <summary>
    /// 获得npc的好感度等级
    /// </summary>
    /// <returns></returns>
    public int GetNpcFeelLevel(int npcId)
    {
        foreach (var item in Model.NpcDic.Values)
        {
            if (item.Id == npcId)
            {
                return item.FeelLv;
            }
        }
        return 0;
    }

    /// <summary>
    /// 获取所有npc
    /// </summary>
    /// <returns></returns>
    public List<NpcData> GetAllNpc()
    {
        return Model.NpcDic.Values.OrderBy(x => x.Id).ToList();
    }

    public bool CheckNpcUnLock(int npcId)
    {
        if (Model.NpcDic.TryGetValue(npcId, out var data))
        {
            return data.NpcState == NpcState.UnLock;
        }
        return false;
    }

    /// <summary>
    /// 是否存在可直接解锁的Npc
    /// </summary>
    /// <returns></returns>
    public bool CheckHasNpcUnlockable()
    {
        var npcDatas = GetAllNpc();
        foreach (var item in npcDatas) 
        {
            if (item.NpcState == NpcState.Openable)
                return true;
        }
        return false;
    }

    public void SetNpcFeel(int NpcId, int exp)
    {
        if (Model.NpcDic.TryGetValue(NpcId, out var data))
        {
            data.AddExp(exp);
        }
    }

    /// <summary>
    /// 刷新所有npc状态
    /// </summary>
    public void RefreshAllNpcState(object obj = null)
    {
        foreach (var item in Model.NpcDic)
        {
            item.Value.RefreshNpcType();
            item.Value.Serialize();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="npcAddDatas"> id, exp, order</param>
    public void ChangeNpcDatas(List<NpcInfo> npcInfoList)
    {
        foreach (var data in npcInfoList)
        {
            if (Model.NpcDic.TryGetValue(data.id, out var item))
            {
                int addExp = data.exp - item.FeelExp;
                int addOrder = data.order - item.Order;
                item.AddExp(addExp);
                item.AddNpcOrder(addOrder);
                item.SetPrefData();
            }
        }
    }

    /// <summary>
    /// 解锁npc
    /// </summary>
    /// <param name="id"></param>
    public void UnLockingNpc(int id)
    {
        if (Model.NpcDic.TryGetValue(id, out var data))
        {
            data.ChangeNpcType(NpcState.UnLock);
            data.SetPrefData();
        }
        else
        {
            Debug.LogError($"未找到id ={id}的npc");
        }
    }
}

