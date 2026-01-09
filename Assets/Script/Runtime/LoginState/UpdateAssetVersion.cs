using System.Collections;
using UnityEngine;
using YooAsset;

/// <summary>
/// 更新资源版本号
/// </summary>
internal class UpdateAssetVersion : LoginState
{
    public override void OnEnter()
    {
        base.OnEnter();
        EventDispatchCenter.Instance.Dispatch(SDEvents.S2C_PATCH_STATES_CHANGE, "获取最新的资源版本！");
        LaunchManager.Instance.StartCoroutine(UpdatePackageVersion());
    }

    private IEnumerator UpdatePackageVersion()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        var login = Owner as Login;
        var packageName = login.PackageName;
        var package = YooAssets.GetPackage(packageName);
        var operation = package.RequestPackageVersionAsync();
        yield return operation;

        if (operation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning(operation.Error);
            EventDispatchCenter.Instance.Dispatch(SDEvents.S2C_PACKAGE_VERSION_UPDATE_FAILED);
        }
        else
        {
            Debug.Log($"Request package version : {operation.PackageVersion}");
            login.PackageVersion = operation.PackageVersion;
            login.Goto<UpdateAssetManifest>();
        }
    }
}