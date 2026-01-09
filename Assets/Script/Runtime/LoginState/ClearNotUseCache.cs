using YooAsset;

/// <summary>
/// 清理未使用的缓存文件
/// </summary>
internal class ClearNotUseCache : LoginState
{
    public override void OnEnter()
    {
        base.OnEnter();
        EventDispatchCenter.Instance.Dispatch(SDEvents.S2C_PATCH_STATES_CHANGE, "清理未使用的缓存文件！");
        var login = Owner as Login;

        var packageName = login.PackageName;
        var package = YooAssets.GetPackage(packageName);
        var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
        operation.Completed += delegate(AsyncOperationBase obj) 
        {
            login.Goto<UpdaterSuccess>();
        };
    }
}