/// <summary>
/// 下载完毕
/// </summary>
internal class DownloadComplete : LoginState
{
    public override void OnEnter()
    {
        base.OnEnter();
        var login = Owner as Login;
        login.Goto<ClearNotUseCache>();
    }
}