using AntNet;

public abstract class BaseControl
{
    protected WebSocketObject TcpNet = null;
    /// <summary>
    /// 初始化
    /// </summary>
    public void OnInitialize()
    {
        //TcpNet = GameManager.Instance.GetWebSocketObjectManager();
        OnInitControl();
    }

    protected abstract void OnInitControl();

    protected abstract void OnCloseControl();

    /// <summary>
    /// 清理
    /// </summary>
    public void OnClear(bool isClearCallBack = true)
    {
        OnCloseControl();
        if (isClearCallBack)
        {
            TcpNet.ClearAllCallBack();
            TcpNet = null;
        }
    }
}

