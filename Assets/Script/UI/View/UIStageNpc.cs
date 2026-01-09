using Framework;
using Pb;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStageNpc : UIItemBase
{
    [BindNode] private RawImage _Obj_npc;

    private Animator animator;

    NpcBase npcCfg;

    protected override void InitItem()
    {
        animator = GetComponent<Animator>();
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_MUSIC_SELECT, ChangeAnimtor);
    }



    protected override void OnDestroy()
    {
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_MUSIC_SELECT, ChangeAnimtor);
    }

    public void SetUI(object o = null)
    {
        npcCfg = (NpcBase)o;
        ResourceManagerNew.instance.LoadTextureAsset(npcCfg.Img2, _Obj_npc);
        ChangeAnimtor();
    }

    private void ChangeAnimtor(object obj = null)
    {
        var musicId = GameManager.Instance.MusicControl.MainMusicId;
        musicId = musicId == 0 ? 1 : musicId;
        var musicCfg = Config.GetConfig<Config_MusicPlayer>().GetConfigById(musicId);
        if (npcCfg.Special == 1)
        {
            animator.Play(musicCfg.BeatsAniMainSp);
        }
        else
        {
            animator.Play(musicCfg.BeatsAniMainKala);
        }
    }
}
