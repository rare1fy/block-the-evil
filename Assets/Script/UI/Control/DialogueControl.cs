using UnityEngine;

public class DialogueControl : BaseControl
{
    private DialogueModel _model;
    public DialogueModel Model
    {
        get
        {
            if (_model == null)
                _model = new DialogueModel();
            return _model;
        }
    }

    protected override void OnInitControl()
    {
        EventDispatchCenter.Instance.Registry(SDEvents.CHANGE_NPC_STATE, OnNpcStateChange);
        EventDispatchCenter.Instance.Registry(SDEvents.CHANGE_LEAVL, RefreshDialogueDatas);
        EventDispatchCenter.Instance.Registry(SDEvents.CHAGE_NPC_FEEL, OnNpcFeelUp);

        var npcControl = GameManager.Instance.NpcControl;

        //需要显示的npc对话
        foreach (var id in npcControl.GetUnLockNpcId())
        {
            //Debug.LogError("解锁的NPCid=====" + id);
            if (!Model.DialogueDatas.ContainsKey(id))
            {
                //解析缓存 写入数据
                var data = new DialogueData(id);
                Model.DialogueDatas.Add(id, data);
            }
        }
    }

    protected override void OnCloseControl()
    {
        EventDispatchCenter.Instance.UnRegistry(SDEvents.CHANGE_NPC_STATE, OnNpcStateChange);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.CHANGE_LEAVL, RefreshDialogueDatas);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.CHAGE_NPC_FEEL, OnNpcFeelUp);


    }

    //npc解锁状态变更，开启对话列表
    public void OnNpcStateChange(object obj = null)
    {
        if (obj != null)
        {
            var npcId = (int)obj;
            //npc状态解锁且没有在对话列表中存在的时候才添加
            var nptCtrl = GameManager.Instance.NpcControl;
            var isUnlock = nptCtrl.CheckNpcUnLock(npcId);
            if (isUnlock) 
            {
                Model.DialogueDatas.TryAdd(npcId, new DialogueData(npcId));
            }
        }
    }

    /// <summary>
    /// 关卡变更（只处理解锁的）
    /// </summary>
    public void RefreshDialogueDatas(object obj = null)
    {
        var leavl = GameManager.Instance.PlayerControl.PlayerModel.Level;
        foreach (var data in Model.DialogueDatas.Values) 
        {
            data.RefreshInfo();
        }
    }

    /// <summary>
    /// npc好感提升（只处理解锁的）
    /// </summary>
    /// <param name="npcId"></param>
    public void OnNpcFeelUp(object obj)
    {
        var tmp = (Tuple<int, int>)obj;
        if (Model.DialogueDatas.TryGetValue(tmp.value1, out var data))
        {
            data.RefreshInfo();
        }
    }

    /// <summary>
    /// s所有对话中是否有新的
    /// </summary>
    /// <returns></returns>
    public bool CheckDialogueRed()
    {
        foreach (var item in Model.DialogueDatas.Values)
        {
            if (item.isNew)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 检查是否完成某对话
    /// </summary>
    /// <param name="chatId">对话id</param>
    /// <returns></returns>
    public bool CheckDialogue(int chatId)
    {
        if (chatId == 0)
        {
            return true;
        }
        var ret = false;
        foreach (var Dialogue in Model.DialogueDatas)
        {
            var check = Dialogue.Value.dialogueItems.Exists(p => p.outWord.Id == chatId);
            ret = check || ret;
        }
        return ret;
    }


}

