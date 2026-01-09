using System.Collections;
using UnityEngine;
using YooAsset;

/// <summary>
/// 创建文件下载器
/// </summary>
public class CreateFileDownloader : LoginState
{
    public override void OnEnter()
    {
        base.OnEnter();
        EventDispatchCenter.Instance.Dispatch(SDEvents.S2C_PATCH_STATES_CHANGE, "创建补丁下载器！");
        LaunchManager.Instance.StartCoroutine(CreateDownloader());
    }

    IEnumerator CreateDownloader()
    {
        yield return new WaitForSecondsRealtime(0.25f);
        var login = Owner as Login;
        var packageName = login.PackageName;
        var package = YooAssets.GetPackage(packageName);
        int downloadingMaxNum = 10;
        int failedTryAgain = 3;
        var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
        login.Downloader = downloader;

        if (downloader.TotalDownloadCount == 0)
        {
            Debug.Log("Not found any download files !");
            login.Goto<UpdaterSuccess>();
        }
        else
        {
            // TODO 发现新更新文件后，挂起流程系统
            // 注意：开发者需要在下载前检测磁盘空间不足
            int totalDownloadCount = downloader.TotalDownloadCount;
            long totalDownloadBytes = downloader.TotalDownloadBytes;
            var msg = new Tuple<int, long>(totalDownloadCount, totalDownloadBytes);
            EventDispatchCenter.Instance.Dispatch(SDEvents.S2C_FOUND_UPDATE_FILES, msg);
        }
    }
}