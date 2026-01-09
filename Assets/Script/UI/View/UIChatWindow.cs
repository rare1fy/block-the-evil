using System.Collections.Generic;
using UnityEngine;

public class UIChatWindow : UIBase
{
    [BindNode]
    private UIChatItem _Obj_Item;
    [BindNode]
    private RectTransform _Obj_Content;
    [BindNode(true)]
    private GameObject _Obj_Empty;

    private List<UIChatItem> pool = new();
    public override void InitOnce()
    {
        AddListener(ui_listener_type.onClick, "_Btn_Close", OnBackClick);
    }

    public override void OnOpen(object param = null)
    {
        RecyclePool();
        var dialogueModel = GameManager.Instance.DialogueControl.Model;
        var index = 0;
        _Obj_Empty.SetActiveEx(dialogueModel.DialogueDatas.Count == 0);
        foreach (var data in dialogueModel.DialogueDatas) 
        {
            UIChatItem allChatItem = GetItem();
            allChatItem.gameObject.name = $"AllChatItem_{index}";
            allChatItem.SetUI(data.Value);
            index++;
        }
    }

    private void RecyclePool()
    {
        pool.Clear();
        for (int i = 0; i < _Obj_Content.childCount; i++)
        {
            var item = _Obj_Content.GetChild(i).GetComponent<UIChatItem>();
            item.gameObject.SetActiveEx(false);
            pool.Add(item);
        }
    }

    private UIChatItem GetItem()
    {
        UIChatItem item;
        if (pool.Count > 0)
        {
            item = pool[0];
            pool.RemoveAt(0);
        }
        else
        {
            item = Instantiate(_Obj_Item, _Obj_Content);
        }
        item.gameObject.SetActiveEx(true);
        return item;
    }
}
