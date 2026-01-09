using DG.Tweening;
using Framework;
using Pb;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PerformWindow : UIBase
{
    [BindNode(true, nodeName: "_Btn_UI")]
    private GameObject _Obj_UI;
    [BindNode]
    private UIButtonExtension _Btn_UI;
    [BindNode(true,nodeName: "_Img_NpcLeft")]
    private GameObject _Obj_NpcLeft;
    [BindNode]
    private RawImage _Img_NpcLeft;
    [BindNode(true, nodeName: "_Img_NpcRight")]
    private GameObject _Obj_NpcRight;
    [BindNode]
    private RawImage _Img_NpcRight;
    [BindNode]
    private CustomText _Txt_Name1;
    [BindNode]
    private CustomText _Txt_Name2;
    [BindNode]
    private CustomText _Txt_Desc;

    private RawImage[] Img_Npcs;

    private Animator animator;

    public override void InitOnce()
    {
        EventDispatchCenter.Instance.Registry(SDEvents.PERFORM_OVER, PerformOver);
        AddListener(ui_listener_type.onClick, "_Btn_UI", OnClickNext);
        Img_Npcs = new RawImage[2] { _Img_NpcLeft, _Img_NpcRight };
        animator = GetComponent<Animator>();
    }

    public override void OnOpen(object param = null)
    {
        var guildPerform = param as GuildPerform;
        var cfg = Config.GetConfig<Config_GuildWord>().GetConfigById(int.Parse(guildPerform.Num));
        AudioManagerNew.Instance.PlayAudio(cfg.Audio);
        _Btn_UI.enabled = false;
        if (cfg.Talkside - 1 == 0) 
        {
            animator.Play("PerformWindow_Left");
        }
        else
        {
            animator.Play("PerformWindow_Right");
        }
        for (int i = 0; i < Img_Npcs.Length; i++)
        {
            ResourceManagerNew.instance.LoadTextureAsset(cfg.Img, Img_Npcs[i]);
        }
        _Txt_Name1.text = cfg.Npcid;
        _Txt_Name2.text = cfg.Npcid;
        _Txt_Desc.text = cfg.Txt1;
        DOVirtual.DelayedCall(0.5f, () =>
        {
            _Btn_UI.enabled = true;
        });
    }

    protected override void OnDestroy()
    {
        EventDispatchCenter.Instance.UnRegistry(SDEvents.PERFORM_OVER, PerformOver);
        base.OnDestroy();
    }
    private void PerformOver(object obj)
    {
        base.OnBackClick();
    }

    private void OnClickNext(GameObject _, PointerEventData __)
    {
        base.OnBackClick();
        GameManager.Instance.PerformControl.SetAwaitEnd();
    }
}
