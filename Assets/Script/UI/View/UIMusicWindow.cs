using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework;
using Pb;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMusicWindow : UIBase
{
    [BindNode] private LoopListView2 _Obj_Scroll;
    [BindNode] private Image _Img_Item1;
    [BindNode] private Image _Img_Item2;
    [BindNode] private Image _Img_Item3;
    [BindNode] private Image _Img_Item4;
    [BindNode] private Image _Img_Item5;
    [BindNode] private CustomText _Txt_ItemCount1;
    [BindNode] private CustomText _Txt_ItemCount2;
    [BindNode] private CustomText _Txt_ItemCount3;
    [BindNode] private CustomText _Txt_ItemCount4;
    [BindNode] private CustomText _Txt_ItemCount5;
    List<Image> ImgItems = new();
    List<CustomText> TxtItems = new();

    //[BindNode] private Image _Img_Progress;
    //[BindNode] private Text _Txt_Progress;
    //[BindNode] private UIButtonExtension _Btn_GetReward;
    //[BindNode(true)] private GameObject _Obj_Red;

    [BindNode] private CustomText _Txt_Schedule;
    [BindNode] private Image _Img_Schedule;


    private Animator _animator;
    private bool isMainOpen = false;

    public override void InitOnce()
    {
        base.InitOnce();
        ImgItems = new() { _Img_Item1, _Img_Item2, _Img_Item3, _Img_Item4, _Img_Item5 };
        TxtItems = new() { _Txt_ItemCount1, _Txt_ItemCount2, _Txt_ItemCount3, _Txt_ItemCount4, _Txt_ItemCount5 };
        _animator = GetComponent<Animator>();
        _Obj_Scroll.InitListViewSingle(InitListView);
        AddListener(ui_listener_type.onClick, "_Btn_Close", OnClickClose);
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_MUSIC_PROGRESS_REFRESH, RefreshResource);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_MUSIC_PROGRESS_REFRESH, RefreshResource);
    }

    private List<MusicPlayer> _musicCfgList;
    private void InitListView(int index, GameObject go, int groupIndex)
    {
        var uiItem = go.GetComponent<UIMusicLine>();
        List<MusicPlayer> data;
        if ((index + 1) * 3 > _musicCfgList.Count)
        {
            data = _musicCfgList.GetRange(index * 3, _musicCfgList.Count - index * 3);
        }
        else
        {
            data = _musicCfgList.GetRange(index * 3, 3);
        }
        uiItem.RefreshItem(data, isMainOpen);
    }

    private Action closeCb = null;
    public override void OnOpen(object param = null)
    {
        Tuple<Action, bool> data = (Tuple<Action, bool>)param;
        closeCb = data.value1;
        isMainOpen = data.value2;
        _musicCfgList = Config.GetConfig<Config_MusicPlayer>().m_MusicPlayerDic.Values.ToList();
        RefreshList();
        RefreshResource();
        AudioManagerNew.Instance.SetPassFilter(true);
        AudioManagerNew.Instance.PlayAudio("UI_Window_InAndOut");
    }

    private void RefreshResource(object param = null)
    {
        var MusicControl = GameManager.Instance.MusicControl;
        var count = Config.GetConfig<Config_MusicPlayer>().m_MusicPlayerDic.Count;
        _Txt_Schedule.text = $"{MusicControl.UnlockMusicIds.Count}/{count}";
        _Img_Schedule.fillAmount = MusicControl.UnlockMusicIds.Count / (float)count;
    }

    private void RefreshList(object param = null)
    {
        var count = (int)Math.Ceiling(_musicCfgList.Count / 3.0f); 
        SortCfgList();
        _Obj_Scroll.ResetListView();
        _Obj_Scroll.FillSuperList(count);
    }

    private void SortCfgList()
    {
        var ctrl = GameManager.Instance.MusicControl;
        _musicCfgList.Sort((a, b) =>
        {
            var aUnlock = ctrl.CheckUnLock(a.Id);
            var bUnlock = ctrl.CheckUnLock(b.Id);
            if (aUnlock && bUnlock || (!aUnlock && !bUnlock))
            {
                return a.Id - b.Id;
            }
            else if (aUnlock)
            {
                return -1;
            }
            else if (bUnlock)
            {
                return 1;
            }
            return 1;
        });
    }

    private void OnClickClose(GameObject o, PointerEventData e)
    {
        _animator.Play("UIMusicWindow_End");
        AudioManagerNew.Instance.SetPassFilter(false);
        AudioManagerNew.Instance.PlayAudio("UI_Window_InAndOut");
    }
    
    /// <summary>
    /// 动画事件用
    /// </summary>
    public void AnimCloseWindow()
    {
        UIManager.Instance.HideUI(this);
        if(closeCb != null)
            closeCb.Invoke();
    }
}
 