using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : Singleton<PopupManager>
{
    private List<PopupBase> _popupQueue = new(2)
    {
        new PopupFirstRecharge(),
    };

    /// <summary>
    /// 是否展示弹窗
    /// </summary>
    private bool isShowingPopup = false;

    public override void Init()
    {

    }

    public void TryShowNextPopup()
    {
        if (!isShowingPopup && _popupQueue.Count > 0)
        {
            isShowingPopup = true;
            GameManager.Instance.StopCoroutine(ShowPopup());
            GameManager.Instance.StartCoroutine(ShowPopup());
        }
    }

    private IEnumerator ShowPopup()
    {
        foreach (var popup in _popupQueue)
        {
            if (popup.IsCanPop)
            {
                popup.OnOpenCallback?.Invoke();
                while (ReferenceEquals(popup.Prefab, null) || popup.Prefab.activeSelf)
                {
                    yield return null;
                }
            }
        }
    }

    public override void Dispose()
    {
    }
}
