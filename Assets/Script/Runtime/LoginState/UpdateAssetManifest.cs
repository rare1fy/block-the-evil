using System.Collections;
using UnityEngine;
using YooAsset;

/// <summary>
/// 更新资源清单
/// </summary>
public class UpdateAssetManifest : LoginState
{
    public override void OnEnter()
    {
        base.OnEnter();
        EventDispatchCenter.Instance.Dispatch(SDEvents.S2C_PATCH_STATES_CHANGE, "更新资源清单！");
        LaunchManager.Instance.StartCoroutine(UpdateManifest());
    }

    private IEnumerator UpdateManifest()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        var login = Owner as Login;
        var packageName = login.PackageName;
        var packageVersion = login.PackageVersion;
        var package = YooAssets.GetPackage(packageName);
        var operation = package.UpdatePackageManifestAsync(packageVersion);
        yield return operation;

        if (operation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning(operation.Error);
            EventDispatchCenter.Instance.Dispatch(SDEvents.S2C_PATCH_MANIFEST_UPDATE_FAILED);
            yield break;
        }
        else
        {
            login.Goto<CreateFileDownloader>();
        }
    }
}