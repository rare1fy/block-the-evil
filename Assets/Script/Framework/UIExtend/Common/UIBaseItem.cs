public abstract class UIItemBase : UIBase
{
    protected override void Awake()
    {
        base.Awake();
        NodeBinder.BindNodes(this, this);
        InitItem();
    }

    protected override void OnInitUIBaseItem()
    {
        base.OnInitUIBaseItem();
        InitItem();
    }

    /// <summary>
    /// 初始化Item
    /// </summary>
    protected abstract void InitItem();
}