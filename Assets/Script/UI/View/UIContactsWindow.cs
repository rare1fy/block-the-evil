using UnityEngine.EventSystems;
using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;
using System;

public class UIContactsWindow : UIBase
{
    [BindNode(true)]
    private GameObject _Obj_Item;
    [BindNode]
    private RectTransform _Obj_Content;
    [BindNode]
    private CustomText _Txt_Schedule;

    List<GameObject> pool = new List<GameObject>(); 

    public override void InitOnce()
    {
        EventDispatchCenter.Instance.Registry(SDEvents.CHANGE_NPC_STATE, OnChangeNpcType);
        AddListener(ui_listener_type.onClick, "_Btn_Close", OnClickClose);
        AddListener(ui_listener_type.onClick, "_Btn_Mask", OnClickClose);
    }

    private void OnChangeNpcType(object obj)
    {
        var npcDatas = GameManager.Instance.NpcControl.GetAllNpc();
        _Txt_Schedule.text = $"{GameManager.Instance.NpcControl.GetUnLockNpcId().Count}/{npcDatas.Count}";
    }

    public void OnClickClose(GameObject go, PointerEventData eventData)
    {
        for (int i = 0; i < _Obj_Content.childCount; i++)
        {
            var obj = _Obj_Content.GetChild(i).gameObject;
            obj.SetActiveEx(false);
            pool.Add(obj);
        }
        base.OnBackClick(go, eventData);
    }

    public override void OnOpen(object param = null)
    {
        var npcDatas = GameManager.Instance.NpcControl.GetAllNpc();
        foreach (var data in npcDatas) 
        {
            GameObject obj;
            if (pool.Count > 0)
            {
                obj =pool[0];
                obj.SetActiveEx(true);
                pool.RemoveAt(0);
            }
            else
            {
                obj = Instantiate(_Obj_Item, _Obj_Content);
                obj.SetActiveEx(true);
            }
            UIContactNpcItem item = obj.GetComponent<UIContactNpcItem>();
            item.SetUI(data);
        }
        _Txt_Schedule.text = $" {GameManager.Instance.NpcControl.GetUnLockNpcId().Count}/{npcDatas.Count}";
    }

    protected override void OnDestroy()
    { 
        EventDispatchCenter.Instance.UnRegistry(SDEvents.CHANGE_NPC_STATE, OnChangeNpcType);

    }
}
