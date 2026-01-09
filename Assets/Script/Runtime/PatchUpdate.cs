using YooAsset;

public class PatchUpdate : GameAsyncOperation
{
    private enum ESteps
    {
        None,
        Update,
        Done,
    }

    private ESteps _steps = ESteps.None;

    private Login _login = null;

    public PatchUpdate(string packageName, EPlayMode playMode)
    {
        //进入初始化资源包状态
        _login = new(packageName, playMode);
        RegistryListener();
    }

    protected override void OnStart()
    {
        _steps = ESteps.Update;
    }

    protected override void OnUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if(_steps == ESteps.Update)
        {
            _login.Update();
            if (_login.Current is UpdaterSuccess)
            {
                RemoveAllListener();
                Status = EOperationStatus.Succeed;
                _steps = ESteps.Done;
            }
        }
    }

    private void RegistryListener()
    {
        //// 注册监听事件
        EventDispatchCenter.Instance.Registry(SDEvents.S2C_USER_TRY_INITIALIZE, UserTryInitialize);
        EventDispatchCenter.Instance.Registry(SDEvents.S2C_USER_BEGIN_DOWNLOAD_WEBFILES, UserBeginDownloadWebFiles);
        EventDispatchCenter.Instance.Registry(SDEvents.S2C_USER_TRY_UPDATE_PACKAGE_VERSION, UserTryUpdatePackageVersion);
        EventDispatchCenter.Instance.Registry(SDEvents.S2C_USER_TRY_UPDATE_PATCH_MANIFEST, UserTryUpdatePatchManifest);
        EventDispatchCenter.Instance.Registry(SDEvents.S2C_USER_TRY_DOWNLOAD_WEBFILES, UserTryDownloadWebFiles);
    }

    private void RemoveAllListener()
    {
        EventDispatchCenter.Instance.UnRegistry(SDEvents.S2C_USER_TRY_INITIALIZE, UserTryInitialize);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.S2C_USER_BEGIN_DOWNLOAD_WEBFILES, UserBeginDownloadWebFiles);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.S2C_USER_TRY_UPDATE_PACKAGE_VERSION, UserTryUpdatePackageVersion);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.S2C_USER_TRY_UPDATE_PATCH_MANIFEST, UserTryUpdatePatchManifest);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.S2C_USER_TRY_DOWNLOAD_WEBFILES, UserTryDownloadWebFiles);
    }

    protected override void OnAbort()
    {}

    /// <summary>
    /// 用户尝试再次初始化资源包
    /// </summary>
    /// <param name="param"></param>
    private void UserTryInitialize(object param)
    {
        _login.Goto<InitPackageData>();
    }

    /// <summary>
    /// 用户开始下载网络文件
    /// </summary>
    /// <param name="param"></param>
    private void UserBeginDownloadWebFiles(object param)
    {
        _login.Goto<DownloadUpdateFiles>();
    }

    /// <summary>
    /// 用户尝试再次更新静态版本
    /// </summary>
    /// <param name="param"></param>
    private void UserTryUpdatePackageVersion(object param)
    {
        _login.Goto<UpdateAssetVersion>();
    }

    /// <summary>
    /// 用户尝试再次更新补丁清单
    /// </summary>
    /// <param name="param"></param>
    private void UserTryUpdatePatchManifest(object param)
    {
        _login.Goto<UpdateAssetManifest>();
    }

    /// <summary>
    /// 用户尝试再次下载网络文件
    /// </summary>
    /// <param name="param"></param>
    private void UserTryDownloadWebFiles(object param)
    {
        _login.Goto<CreateFileDownloader>();
    }
}