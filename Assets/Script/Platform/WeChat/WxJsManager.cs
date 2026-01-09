#if WEIXINMINIGAME
using System;
using UnityEngine;
using System.Runtime.InteropServices;
using Framework;

public class WxJsManager : MonoSingleton<WxJsManager>
{
    [DllImport("__Internal")]
    private static extern void VibrateShortLight();
    
    [DllImport("__Internal")]
    private static extern void VibrateShortMedium();
    
    [DllImport("__Internal")]
    private static extern void VibrateShortHeavy();

    [DllImport("__Internal")]
    private static extern void VibrateLong();

    [DllImport("__Internal")]
    private static extern void CustomWXWriteFile(string fileKey, string dataJson);

    [DllImport("__Internal")]
    private static extern void CustomWXReadFile(string fileKey);

    [DllImport("__Internal")]
    private static extern void RegisterOnHideCallback();

    [DllImport("__Internal")]
    private static extern void RegisterOnShowCallback();

    #region 震动接口

    public void TriggerShortVibration(int type)
    {
        if(!PlayerDataManager.instance.PlayerData.BShake)
            return;
        switch (type)
        {
            case 2:
                VibrateShortMedium();
                break;
            case 3:
                VibrateShortHeavy();
                break;
            default:
                VibrateShortLight();
                break;
        }
    }

    public void TriggerLongVibration()
    {
        VibrateLong();
    }

    #endregion

    #region 文件接口

    public void SaveDataToJson(string fileKey, string jsonData)
    {
        try
        {
            CustomWXWriteFile(fileKey, jsonData);
            Debug.Log($"数据保存成功: {fileKey}");
        }
        catch (Exception e)
        {
            Debug.LogError($"保存失败: {e.Message}");
        }
    }

    public void LoadDataFromJson(string fileKey, Action<string> onSuccess)
    {
        readFileCallback = onSuccess;
        CustomWXReadFile(fileKey);
    }

    #endregion


    /// <summary>
    /// 注册微信事件回调
    /// </summary>
    public void RegisterWeChatCallbacks()
    {
        RegisterOnHideCallback();
        RegisterOnShowCallback();
    }

#region js直接调用的函数

    public static Action<string> readFileCallback;

    public void OnReadFileFromJs(string jsonData)
    {
        if (readFileCallback == null)
        {
            Debug.LogWarning("OnReadFileFromJs 调用时没有注册回调，忽略。数据=" + jsonData);
            return;
        }

        if (string.IsNullOrEmpty(jsonData))
        {
            readFileCallback?.Invoke(null);
        }
        else
        {
            readFileCallback?.Invoke(jsonData);
        }
        readFileCallback = null;
    }

    public void _OnWxShow(string msg)
    {
        EventDispatchCenter.Instance.Dispatch(SDEvents.WX_ON_SHOW);
        AudioManagerNew.Instance.IsInBackground = false;
        AudioManagerNew.Instance.ResumeMusic();
    }

    public void _OnWxHide(string msg)
    {
        AudioManagerNew.Instance.IsInBackground = true;
        AudioManagerNew.Instance.HideAudio();
        PlayerDataManager.instance.MusicSave();
    }
    
#endregion

}

#endif