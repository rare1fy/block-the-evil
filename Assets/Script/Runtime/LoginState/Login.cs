using YooAsset;

public class Login: StateMachine<LoginState>
{
    /// <summary>
    /// 默认包名
    /// </summary>
    public string PackageName;

    /// <summary>
    /// 运行模式
    /// </summary>
    public EPlayMode ePlayMode;

    /// <summary>
    /// 包版本号
    /// </summary>
    public string PackageVersion;

    /// <summary>
    /// 下载器
    /// </summary>
    public ResourceDownloaderOperation Downloader;

    public Login(string packageName, EPlayMode playMode)
    {
        PackageName = packageName;
        ePlayMode = playMode;
        Goto<InitPackageData>();
    }
}
