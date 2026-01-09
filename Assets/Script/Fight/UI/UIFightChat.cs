using DG.Tweening;
using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIFightChat : UIBase
{
    [BindNode]
    private CustomText _Txt_Content1;
    [BindNode]
    private CustomText _Txt_Content2;
    [BindNode]
    private CustomText _Txt_Content3;

    [BindNode(true)]
    private GameObject _Npc_Point1;
    [BindNode(true)]
    private GameObject _Npc_Point2;
    [BindNode(true)]
    private GameObject _Npc_Point3;

    [BindNode]
    private RawImage _Img_Npc1;
    [BindNode]
    private RawImage _Img_Npc2;
    [BindNode]
    private RawImage _Img_Npc3;
    [BindNode(true)]
    private GameObject _Btn_Select1;
    [BindNode]
    private CustomText _Text_Selcet1;
    [BindNode(true)]
    private GameObject _Btn_Select2;
    [BindNode]
    private CustomText _Text_Selcet2;
    [BindNode(true)]
    private GameObject _Obj_Select;
    private List<RawImage> _Img_Npcs;
    private List<GameObject> NpcPont;
    private List<CustomText> _Txt_Content;


    private int npcId;
    private int pos;
    private Action<int,int> closeAction;
    private FightChatEventBase curEvent;
    private Animator animator;

    public override void InitOnce()
    {
        animator = GetComponent<Animator>();
        AddListener(ui_listener_type.onClick, "_Btn_Select1",OnSelectlClick);
        AddListener(ui_listener_type.onClick, "_Btn_Select2", OnSelect2Click);
        _Img_Npcs = new List<RawImage>() { _Img_Npc1, _Img_Npc2, _Img_Npc3 };
        NpcPont = new List<GameObject>() { _Npc_Point1, _Npc_Point2, _Npc_Point3 };
        _Txt_Content = new() { _Txt_Content1 , _Txt_Content2 , _Txt_Content3};
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_FIGHT_CHAT_NEXT, FightChatNext);
    }

    public override void OnOpen(object param = null)
    {
        base.OnOpen(param);
        var data = param as FightChatViewData;
        npcId = data.npcId;
        pos = data.pos;
        closeAction = data.CloseAction;
        var npcCfg = Config.GetConfig<Config_NpcBase>().GetConfigById(npcId); 
        for (int i = 0; i < _Img_Npcs.Count; i++)
        {
            if (pos == i)
            {
                ResourceManagerNew.instance.LoadTextureAsset(npcCfg.Img2, _Img_Npcs[i]);
                NpcPont[i].SetActive(true);
            }
            else
            {
                NpcPont[i].SetActive(false);
            }
        }
        animator.Play("UIFightChat_Start",0,0);
        StartCoroutine(CreateFightChat(true));
    }

    protected override void OnDestroy()
    {
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_FIGHT_CHAT_NEXT, FightChatNext);
        base.OnDestroy();
    }

    private void FightChatNext(object obj)
    {
        StopCoroutine(CreateFightChat(false));
        StartCoroutine(CreateFightChat(false));
    }

    private void OnSelect2Click(GameObject _, PointerEventData __)
    {
        curEvent.OnClick(2);
    }

    private void OnSelectlClick(GameObject _, PointerEventData __)
    {
        curEvent.OnClick(1);
    }

    private IEnumerator CreateFightChat(bool isOpen)
    {
        curEvent = GameManager.Instance.CurFightControl.ChatController.GetFightChatEven(npcId);
        if (!string.IsNullOrEmpty(curEvent.cfg.Audio))
        {
            AudioManagerNew.Instance.PlayAudio(curEvent.cfg.Audio);
        }
        _Txt_Content[pos].text = curEvent.cfg.Txt1;
        if (isOpen) 
            yield return new WaitForSeconds(0.5f);
        if (curEvent.HasOperation(out var time))
        {
            SetSelectView(true);
            yield return new WaitUntil(() => { return curEvent.IsOperationComplete; });
            SetSelectView(false);
        }
        else
        {
            yield return new WaitForSeconds(time);
        }
        var tmpEvent = curEvent;
        curEvent = null;
        tmpEvent.Exit();
        if (tmpEvent.IsEnd)
        {
            OnBackClick();
            DOVirtual.DelayedCall(1f, () =>
            {
                var reward = tmpEvent.cfg.GoodfeelReward;
                if (!string.IsNullOrEmpty(reward)) 
                {
                    var strData = reward.Split("#");
                    closeAction?.Invoke(int.Parse(strData[0]), int.Parse(strData[1]));
                }
                else
                {
                    closeAction?.Invoke(0, 0);
                }
                StopCoroutine(CreateFightChat(false));
            });
        }
    }

    public void SetSelectView(bool isShow)
    {
        _Obj_Select.SetActiveEx(isShow);
        if (isShow)
        {
            var chatBase = curEvent;
            _Btn_Select1.SetActiveEx(chatBase.isShowBtn1());
            _Btn_Select2.SetActiveEx(chatBase.isShowBtn2());
            _Text_Selcet1.text = chatBase.cfg.Button1;
            _Text_Selcet2.text = chatBase.cfg.Button2;
        }
    }

    protected override void OnBackClick(GameObject go = null, PointerEventData eventData = null)
    {
        animator.enabled = false;
        animator.enabled = true;
        animator.Play("UIFightChat_End", 0, 0);
        DOVirtual.DelayedCall(1.5f, () => { base.OnBackClick(go, eventData); });
        
    }
}
