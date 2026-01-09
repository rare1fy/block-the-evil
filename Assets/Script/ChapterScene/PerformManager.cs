using DG.Tweening;
using Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PerformManager : MonoBehaviour
{
    public static PerformManager _Instance;
  
    public void Awake()
    {
        _Instance = this;
        EventDispatchCenter.Instance.Registry(SDEvents.PERFORM_NEXT, NaxtPerform);
        EventDispatchCenter.Instance.Registry(SDEvents.PERFORM_START, StartPerform);
        EventDispatchCenter.Instance.Registry(SDEvents.PERFORM_OVER, PerformOver);
        EventDispatchCenter.Instance.Registry(SDEvents.CHANGE_LEAVL, CheckPerform);
    }

    private void CheckPerform(object obj)
    {

    }

    private void PerformOver(object obj)
    {
        int guildId = (int)obj;
        var cfg = Config.GetConfig<Config_GuildGroup>().GetConfigById(guildId);
        if (UIManager.Instance.GetTopUI() == null || UIManager.Instance.GetTopUI().UiName != "UIFightMain")
        {
            AudioManagerNew.Instance.FadeStopMusic(() =>
            {
                UIManager.Instance.HideUI("PerformWindow");
                UIManager.Instance.ShowUI("MainWindow");
            });
        }
        else
        {
            UIManager.Instance.HideUI("PerformWindow");
        }
        
        ChapterSceneManager._Instance.PerformOver();
    }

    public void StartPerform(object obj = null) 
    {
        int guildId = (int)obj;
        var cfg = Config.GetConfig<Config_GuildGroup>().GetConfigById(guildId);
        AudioManagerNew.Instance.PlayMusic(cfg.Music);
        var curPerform = GameManager.Instance.PerformControl.Model.performData.curPerform;
        StopAllCoroutines();
        StartCoroutine(curPerform.Operation());
    }

    public void NaxtPerform(object obj = null)
    {
        var curPerform = GameManager.Instance.PerformControl.Model.performData.curPerform;
        StopAllCoroutines();
        StartCoroutine(curPerform.Operation());
    }

    public void OnEnable()
    {
    }

    public void OnDestroy()
    {
        EventDispatchCenter.Instance.UnRegistry(SDEvents.PERFORM_NEXT, NaxtPerform);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.PERFORM_START, StartPerform);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.PERFORM_OVER, PerformOver);
    }
}
