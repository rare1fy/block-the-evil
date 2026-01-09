public class PopupFirstRecharge : PopupBase
{
    public PopupFirstRecharge(object data = null)
    {
        Data = data;
        PrefabName = "UIFirstRecharge";
        OnOpenCallback = OpenWindow;
        IsCanPop = true;
    }

    private void OpenWindow()
    {
        UIManager.Instance.ShowUI(PrefabName, (obj) =>
        {
            Prefab = obj;
        }, Data);
    }
}
