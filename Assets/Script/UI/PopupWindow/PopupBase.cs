using UnityEngine;

public class PopupBase
{
    public GameObject Prefab = null;
    public string PrefabName = string.Empty;
    public System.Action OnOpenCallback = null;
    public object Data = null;

    /// <summary>
    /// 是否能展示弹窗
    /// </summary>
    public bool IsCanPop = false;
}
