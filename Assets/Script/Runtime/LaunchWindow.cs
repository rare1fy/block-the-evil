using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LaunchWindow : UIBase
{
    [BindNode(true)] private GameObject _Obj_MessgeBox;
    [BindNode] private Slider _Obj_Slider;
    [BindNode] private CustomText _Txt_Tips;
    [BindNode] private CustomText _Txt_Content;
    //[BindNode] private Image _Img_Green_Rabbit;
    [BindNode] private CustomText _Txt_Version;
    
    private Action _clickOK;
    protected override void Awake()
    {
        base.Awake();
        NodeBinder.BindNodes(this, this);
        AddListener(ui_listener_type.onClick, "_Btn_OK", OnClickYes);
        EventDispatchCenter.Instance.Registry(SDEvents.S2C_INIT_FAILED, InitializeFailed);
        EventDispatchCenter.Instance.Registry(SDEvents.S2C_PATCH_STATES_CHANGE, PatchStatesChange);
        EventDispatchCenter.Instance.Registry(SDEvents.S2C_FOUND_UPDATE_FILES, FoundUpdateFiles);
        EventDispatchCenter.Instance.Registry(SDEvents.S2C_DOWN_LOAD_PROGRESS_UPDATE, DownloadProgressUpdate);
        EventDispatchCenter.Instance.Registry(SDEvents.S2C_PACKAGE_VERSION_UPDATE_FAILED, PackageVersionUpdateFailed);
        EventDispatchCenter.Instance.Registry(SDEvents.S2C_PATCH_MANIFEST_UPDATE_FAILED, PatchManifestUpdateFailed);
        EventDispatchCenter.Instance.Registry(SDEvents.S2C_WEB_FILE_DOWNLOAD_FAILED, WebFileDownloadFailed);
        _Obj_MessgeBox.SetActiveEx(false);
        _Obj_Slider.gameObject.SetActiveEx(false);
        _Txt_Tips.text = "[LID:705]";
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventDispatchCenter.Instance.UnRegistry(SDEvents.S2C_INIT_FAILED, InitializeFailed);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.S2C_PATCH_STATES_CHANGE, PatchStatesChange);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.S2C_FOUND_UPDATE_FILES, FoundUpdateFiles);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.S2C_DOWN_LOAD_PROGRESS_UPDATE, DownloadProgressUpdate);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.S2C_PACKAGE_VERSION_UPDATE_FAILED, PackageVersionUpdateFailed);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.S2C_PATCH_MANIFEST_UPDATE_FAILED, PatchManifestUpdateFailed);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.S2C_WEB_FILE_DOWNLOAD_FAILED, WebFileDownloadFailed);
    }

    /// <summary>
    /// 设置进度条的值
    /// </summary>
    /// <param name="value"></param>
    public void SetSliderValue(float value)
    {
        //_Img_Green_Rabbit.fillAmount = value / 100f;
    }

    public void SetVersion(string versionStr)
    {
        _Txt_Version.text = versionStr;
    }

    private void OnClickYes(GameObject go, PointerEventData pointerEventData)
    {
        _clickOK?.Invoke();
        _Obj_MessgeBox.SetActive(false);
    }

    /// <summary>
    /// 补丁包初始化失败
    /// </summary>
    /// <param name="param"></param>
    private void InitializeFailed(object param)
    {
        Action callback = () => { EventDispatchCenter.Instance.Dispatch(SDEvents.S2C_USER_TRY_INITIALIZE); };
        ShowMessageBox("Failed to initialize package !", callback);
    }

    /// <summary>
    /// 补丁流程步骤改变
    /// </summary>
    /// <param name="param"></param>
    private void PatchStatesChange(object param)
    {
        _Txt_Tips.text = (string)param;
    }

    /// <summary>
    /// 发现更新文件
    /// </summary>
    /// <param name="param"></param>
    private void FoundUpdateFiles(object param)
    {
        var msg = (Tuple<int, long>)param;
        float sizeMB = msg.value2 / 1048576f;
        sizeMB = Mathf.Clamp(sizeMB, 0.1f, float.MaxValue);
        var totalSizeMB = sizeMB.ToString("f1");
        EventDispatchCenter.Instance.Dispatch(SDEvents.S2C_USER_BEGIN_DOWNLOAD_WEBFILES);
    }

    /// <summary>
    /// 下载进度更新
    /// </summary>
    /// <param name="param"></param>
    private void DownloadProgressUpdate(object param)
    {
        var msg = (Tuple<int, int, long, long>)param;
        _Obj_Slider.value = (float)msg.value2 / msg.value1;
        var currentSize = msg.value4 / 1048576f;
        var currentSizeMB = currentSize.ToString("f1");
        var totalSize = msg.value3 / 1048576f;
        var totalSizeMB = totalSize.ToString("f1");
        var des = $"{msg.value2}/{msg.value1} {currentSizeMB}MB/{totalSizeMB}MB";
        _Txt_Tips.text = des;
    }

    /// <summary>
    /// 资源版本号更新失败
    /// </summary>
    /// <param name="param"></param>
    private void PackageVersionUpdateFailed(object param)
    {
        Action callback = () =>
        {
            EventDispatchCenter.Instance.Dispatch(SDEvents.S2C_USER_TRY_UPDATE_PACKAGE_VERSION);
        };
        ShowMessageBox($"Failed to update static version, please check the network status.", callback);
    }

    /// <summary>
    /// 补丁清单更新失败
    /// </summary>
    /// <param name="param"></param>
    private void PatchManifestUpdateFailed(object param)
    {
        Action callback = () => { EventDispatchCenter.Instance.Dispatch(SDEvents.S2C_USER_TRY_UPDATE_PATCH_MANIFEST); };
        ShowMessageBox($"Failed to update patch manifest, please check the network status.", callback);
    }

    /// <summary>
    /// 网络文件下载失败
    /// </summary>
    /// <param name="param"></param>
    private void WebFileDownloadFailed(object param)
    {
        var msg = (string)param;
        Action callback = () => { EventDispatchCenter.Instance.Dispatch(SDEvents.S2C_USER_TRY_DOWNLOAD_WEBFILES); };
        ShowMessageBox($"Failed to download file : {msg}", callback);
    }

    /// <summary>
    /// 显示对话框
    /// </summary>
    private void ShowMessageBox(string content, Action ok = null)
    {
        Debug.LogError(content);
        _Txt_Content.text = content;
        _Obj_MessgeBox.SetActiveEx(true);
        _clickOK = ok;
    }

    public void LaunchAudio()
    {
        Debug.LogError("yasialei");
    }
}