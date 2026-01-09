using UnityEngine;
using YooAsset;

public static class YooAssetManager
{
    public const string DefaultPackageName = "DefaultPackage";
    public static ResourcePackage DefaultPackage { get; private set; }
    public static string PackageVersion { get; set; }

    public static InitializationOperation Initialize(EPlayMode playMode)
    {
        // 初始化资源系统
        YooAssets.Initialize();
        // 创建默认的资源包
        DefaultPackage = YooAssets.CreatePackage(DefaultPackageName);
        // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
        YooAssets.SetDefaultPackage(DefaultPackage);

        // 编辑器下的模拟模式
        InitializationOperation initializationOperation = null;
        if (playMode == EPlayMode.EditorSimulateMode)
        {
            var buildResult = EditorSimulateModeHelper.SimulateBuild(DefaultPackageName);
            var packageRoot = buildResult.PackageRootDirectory;
            var createParameters = new EditorSimulateModeParameters();
            createParameters.EditorFileSystemParameters =
                FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            initializationOperation = DefaultPackage.InitializeAsync(createParameters);
        }

        // 单机运行模式
        if (playMode == EPlayMode.OfflinePlayMode)
        {
            var createParameters = new OfflinePlayModeParameters();
            createParameters.BuildinFileSystemParameters =
                FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            initializationOperation = DefaultPackage.InitializeAsync(createParameters);
        }

        // 联机运行模式
        if (playMode == EPlayMode.HostPlayMode)
        {
            string defaultHostServer = GetHostServerURL();
            string fallbackHostServer = GetHostServerURL();
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            var createParameters = new HostPlayModeParameters();
            createParameters.BuildinFileSystemParameters =
                FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            createParameters.CacheFileSystemParameters =
                FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
            initializationOperation = DefaultPackage.InitializeAsync(createParameters);
        }

        // WebGL运行模式
        if (playMode == EPlayMode.WebPlayMode)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var createParameters = new WebPlayModeParameters();
            string defaultHostServer = GetHostServerURL();
            Debug.LogError($"服务器地址：{defaultHostServer}");
            string fallbackHostServer = GetHostServerURL();
            string packageRoot =
                $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE"; //注意：如果有子目录，请修改此处！
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            createParameters.WebServerFileSystemParameters =
                WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices);
            initializationOperation = DefaultPackage.InitializeAsync(createParameters);
#else
            var createParameters = new WebPlayModeParameters();
            createParameters.WebServerFileSystemParameters =
                FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
            initializationOperation = DefaultPackage.InitializeAsync(createParameters);
#endif
        }

        return initializationOperation;
    }


    public static string AppVersion;
    /// <summary>
    /// 获取资源服务器地址
    /// </summary>
    private static string GetHostServerURL()
    {
        string hostServerIP = "https://fkyqgwx-res.chuxinhd.com";
        AppVersion = "1.0.0";
        
#if UNITY_EDITOR
        if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
            return $"{hostServerIP}/Android/{AppVersion}";
        else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
            return $"{hostServerIP}/IPhone/{AppVersion}";
        else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
            return $"{hostServerIP}/WebGL/{AppVersion}";
        else
            return $"{hostServerIP}/PC/{AppVersion}";
#else
        if (Application.platform == RuntimePlatform.Android)
            return $"{hostServerIP}/Android/{AppVersion}";
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
            return $"{hostServerIP}/IPhone/{AppVersion}";
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
            return $"{hostServerIP}/WebGL/{AppVersion}";
        else
            return $"{hostServerIP}/PC/{AppVersion}";
#endif
    }

    public static RequestPackageVersionOperation RequestPackageVersionAsync()
    {
        var operation = DefaultPackage.RequestPackageVersionAsync();
        return operation;
    }

    public static UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion)
    {
        var operation = DefaultPackage.UpdatePackageManifestAsync(packageVersion);
        return operation;
    }

    /// <summary>
    /// 用于下载更新当前资源版本所有的资源包文件
    /// </summary>
    public static ResourceDownloaderOperation DownloadAllAssetBundles(int downloadingMaxNum = 10,
        int failedTryAgain = 3, int timeout = 60)
    {
        var downloader = DefaultPackage.CreateResourceDownloader(downloadingMaxNum, failedTryAgain, timeout);

        // 没有需要下载的资源
        if (downloader.TotalDownloadCount == 0)
        {
            Debug.Log("Not found any download files !");
            return downloader;
        }

        // 发现新更新文件后，挂起流程系统
        // 注意：开发者需要在下载前检测磁盘空间不足
        int totalDownloadCount = downloader.TotalDownloadCount;
        long totalDownloadBytes = downloader.TotalDownloadBytes;

        downloader.DownloadErrorCallback = data => { };
        downloader.DownloadUpdateCallback = data => { };
        downloader.DownloadFinishCallback = data => { };

        // 开启下载
        downloader.BeginDownload();
        return downloader;
    }

    /// <summary>
    /// 用于下载更新指定的资源列表依赖的资源包文件 location只需要填写资源包里的任意资源地址
    /// </summary>
    public static ResourceDownloaderOperation DownloadBundle(string location, int failedTryAgain = 3,
        int timeout = 60)
    {
        int downloadingMaxNum = 10;
        var downloader =
            DefaultPackage.CreateBundleDownloader(location, downloadingMaxNum, failedTryAgain, timeout);

        // 没有需要下载的资源
        if (downloader.TotalDownloadCount == 0)
        {
            return downloader;
        }

        // 开启下载
        downloader.BeginDownload();
        return downloader;
    }
}

/// <summary>
/// 远端资源地址查询服务类
/// </summary>
public class RemoteServices : IRemoteServices
{
    private readonly string _defaultHostServer;
    private readonly string _fallbackHostServer;

    public RemoteServices(string defaultHostServer, string fallbackHostServer)
    {
        _defaultHostServer = defaultHostServer;
        _fallbackHostServer = fallbackHostServer;
    }

    string IRemoteServices.GetRemoteMainURL(string fileName)
    {
        return $"{_defaultHostServer}/{fileName}";
    }

    string IRemoteServices.GetRemoteFallbackURL(string fileName)
    {
        return $"{_fallbackHostServer}/{fileName}";
    }
}