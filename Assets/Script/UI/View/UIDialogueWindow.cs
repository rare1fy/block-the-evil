using DG.Tweening;
using Pb;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDialogueWindow : UIBase
{
    #region ui
    #region 对话item
    [BindNode]
    private RectTransform _Obj_Content;
    [BindNode]
    private UIDiaLogueItem _Item_NpcChat;
    [BindNode]
    private UIDiaLogueItem _Item_PlayerChat;
    [BindNode]
    private UIDiaLogueItem _Item_NpcBtn;
    [BindNode]
    private UIDiaLogueItem _Item_PlayerBtn; 
    [BindNode]
    private UIDiaLogueItem _Item_NpcImg;
    [BindNode]
    private UIDiaLogueItem _Item_PlayerImg;
    #endregion
    [BindNode]
    private RectTransform _Obj_List;
    [BindNode(true)]
    private GameObject _Obj_Select;
    [BindNode(true)]
    private GameObject _Btn_Select1;
    [BindNode(true)]
    private GameObject _Btn_Select2;
    [BindNode]
    private CustomText _Text_Selcet1;
    [BindNode]
    private CustomText _Text_Selcet2;
    [BindNode]
    private RectTransform _Obj_Bottom;
    [BindNode(true)]
    private GameObject _Obj_Gift;
    [BindNode]
    private FeelView _Obj_FeelView;
    #endregion

    DialogueData data;

    float offsetMinY = 0;

    public override void InitOnce()
    {
        EventDispatchCenter.Instance.Registry(SDEvents.DIALOGUE_NEXT, OnDialogueNext);
        EventDispatchCenter.Instance.Registry(SDEvents.DIALOGUE_END, OnDialogueEnd);
        EventDispatchCenter.Instance.Registry(SDEvents.CHAGE_NPC_FEEL, OnFeelLvChange);

        AddListener(ui_listener_type.onClick, "_Btn_Close", OnClickClose);
        AddListener(ui_listener_type.onClick, "_Btn_Select1", OnClickSelect1);
        AddListener(ui_listener_type.onClick, "_Btn_Select2", OnClickSelect2);
        AddListener(ui_listener_type.onClick, "_Btn_Gift", OnClickEmpty);
        AddListener(ui_listener_type.onClick, "_Btn_Call", OnClickEmpty);

        offsetMinY = _Obj_List.offsetMin.y;
    }

    private void OnClickEmpty(GameObject @object, PointerEventData data)
    {
        UIManager.Instance.ShowPromptWindow("功能暂未开放");
    }

    private void OnFeelLvChange(object obj)
    {
        var tmp = (Tuple<int, int>)obj;
        if (data.npcId == tmp.value1)
        {
            _Obj_FeelView.ShowView(tmp.value2);
        }
    }

    private void OnDialogueNext(object obj)
    {
        StopCoroutine(CreateBulletScreen());
        StartCoroutine(CreateBulletScreen());
    }

    private void OnDialogueEnd(object obj)
    {
        StopCoroutine(CreateBulletScreen());
        ShowGift(true);
    }

    public override void OnOpen(object param = null)
    {
        data = (DialogueData)param;
        //清除对话子节点
        for (int i = _Obj_Content.childCount - 1; i >= 0; i--)
        {
            Destroy(_Obj_Content.GetChild(i).gameObject);
        }
        var count = Config.GetConfig<Config_GdConstant>().GetConfigById(2).Num;

        _Obj_List.offsetMin = new Vector2(_Obj_List.offsetMin.x, offsetMinY);
        SetSelectView(false);
        SetFeel();
        //处理历史对话（只显示20条历史对话）
        var history = data.dialogueItems;
        if (history.Count - 1 > count)
        {
            for (int i = history.Count - 1 - count; i < history.Count - 1; i++)
            {
                var item = GetItem(history[i]);
                item.gameObject.name = history[i].outWord.Id.ToString();
                item.SetUI(history[i]);
            }
        }
        else
        {
            for (int i = 0; i < history.Count - 1 ; i++)
            {
                var item = GetItem(history[i]);
                item.gameObject.name = history[i].outWord.Id.ToString();
                 item.SetUI(history[i]);
            }
        }
        _Obj_List.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
        if (data.CurEventChatBase != null)
        {
            StopCoroutine(CreateBulletScreen());
            StartCoroutine(CreateBulletScreen());
        }
    }

    private IEnumerator CreateBulletScreen()
    {
        var chatBase = data.CurEventChatBase;
        var item = GetItem(chatBase);
        float waitTiem = 0;
        item.gameObject.name = chatBase.outWord.Id.ToString();
        if (chatBase.outWord.Typeid > 0)
        {
            AudioManagerNew.Instance.PlayAudio("UI_Bo");
        }
        item.SetUI(chatBase);
        LayoutRebuilder.ForceRebuildLayoutImmediate(item.transform as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_Obj_Content);
        _Obj_List.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
        if (!chatBase.HasOperation(out waitTiem))
        {
            yield return new WaitForSeconds(waitTiem);
        }
        else
        {
            SetSelectView(true);
            _Obj_List.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
            yield return new WaitUntil(()=> chatBase.IsOperationComplete);
            SetSelectView(false);
        }
            chatBase.Exit();
    }


    public void SetSelectView(bool isShow)
    {
        _Obj_Select.SetActiveEx(isShow);
        if (isShow)
        {
            var chatBase = data.CurEventChatBase;
            _Btn_Select1.SetActiveEx(chatBase.isShowBtn1());
            _Btn_Select2.SetActiveEx(chatBase.isShowBtn2());
            _Text_Selcet1.text = chatBase.outWord.Button1;
            _Text_Selcet2.text = chatBase.outWord.Button2;
        }
        else
        {
            ShowGift(!data.isNew);
        }
    }

    public void ShowGift(bool show)
    {
        _Obj_Gift.SetActiveEx(show);
        if (show)
        {
            _Obj_Bottom.anchoredPosition = new Vector2(0, -38);
            _Obj_List.offsetMin = new Vector2(_Obj_List.offsetMin.x, offsetMinY - 210);
        }
        else
        {
            _Obj_Bottom.anchoredPosition = new Vector2(0, 174);
            _Obj_List.offsetMin = new Vector2(_Obj_List.offsetMin.x, offsetMinY);
        }
    }

    public void SetFeel()
    {
        var ctrl = GameManager.Instance.NpcControl;
        var npcData = ctrl.GetNpcData(data.npcId);
        _Obj_FeelView.SetInitialFeelView(npcData.FeelLv, npcData.FeelExp,npcData.Id);
    }

    private void OnClickClose(GameObject go, PointerEventData eventData)
    {
        base.OnBackClick(go, eventData);
        StopCoroutine(CreateBulletScreen());
    }

    private void OnClickSelect1(GameObject go, PointerEventData eventData)
    {
        _Obj_Select.SetActiveEx(false);
        data.CurEventChatBase.OnClick(1);
    }
    private void OnClickSelect2(GameObject go, PointerEventData eventData)
    {
        _Obj_Select.SetActiveEx(false);
        data.CurEventChatBase.OnClick(2);
    }

    private UIDiaLogueItem GetItem(OutWolrdChatBase outWolrd)
    {
        switch (outWolrd.outWord.DialogueType)
        {
            case 1:
                return Instantiate(_Item_NpcChat, _Obj_Content);
            case 2:
            case 11:
                return Instantiate(_Item_PlayerChat, _Obj_Content);
            case 3:
                return Instantiate(_Item_NpcBtn, _Obj_Content);
            case 4:
                return Instantiate(_Item_NpcBtn, _Obj_Content);
            case 5:
                return Instantiate(_Item_PlayerImg, _Obj_Content);
            case 6:
                return Instantiate(_Item_NpcChat, _Obj_Content);
            case 7:
                return Instantiate(_Item_PlayerChat, _Obj_Content);
            case 8:
                return Instantiate(_Item_NpcChat, _Obj_Content);
            case 9:
                return Instantiate(_Item_PlayerChat, _Obj_Content);
            case 10:
                return Instantiate(_Item_NpcImg, _Obj_Content);
            default:
                return Instantiate(_Item_PlayerChat, _Obj_Content);
        }
    }

    protected override void OnDestroy()
    { 
        EventDispatchCenter.Instance.UnRegistry(SDEvents.DIALOGUE_NEXT, OnDialogueNext);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.DIALOGUE_END, OnDialogueEnd);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.CHAGE_NPC_FEEL, OnFeelLvChange);

    }
}
