using System.Collections;
using YooAsset;

/// <summary>
/// 下载更新文件
/// </summary>
public class DownloadUpdateFiles : LoginState
{
    public override void OnEnter()
    {
        base.OnEnter();
        EventDispatchCenter.Instance.Dispatch(SDEvents.S2C_PATCH_STATES_CHANGE, "开始下载补丁文件！");
        LaunchManager.Instance.StartCoroutine(BeginDownload());
    }

    private IEnumerator BeginDownload()
    {
        var login = Owner as Login;

        var downloader = login.Downloader;
        downloader.DownloadErrorCallback = SendWebFileDownloadFailed;
        downloader.DownloadUpdateCallback = SendDownloadProgressUpdate;
        downloader.BeginDownload();
        yield return downloader;

        // 检测下载结果
        if (downloader.Status != EOperationStatus.Succeed)
            yield break;

        login.Goto<DownloadComplete>();
    }

    private void SendWebFileDownloadFailed(DownloadErrorData errorData)
    {
        string fileName = errorData.FileName;
        string error = errorData.ErrorInfo;
        var msg = new Tuple<string, string>(fileName, error);
        EventDispatchCenter.Instance.Dispatch(SDEvents.S2C_WEB_FILE_DOWNLOAD_FAILED, msg);
    }

    private void SendDownloadProgressUpdate(DownloadUpdateData updateData)
    {
        var totalDownloadCount = updateData.TotalDownloadCount;
        var currentDownloadCount = updateData.CurrentDownloadCount;
        var totalDownloadSizeBytes = updateData.TotalDownloadBytes;
        var currentDownloadSizeBytes = updateData.CurrentDownloadBytes;
        var msg = new Tuple<int, int,long,long>(totalDownloadCount, currentDownloadCount,
            totalDownloadSizeBytes, currentDownloadSizeBytes);
        EventDispatchCenter.Instance.Dispatch(SDEvents.S2C_DOWN_LOAD_PROGRESS_UPDATE, msg);
    }
}