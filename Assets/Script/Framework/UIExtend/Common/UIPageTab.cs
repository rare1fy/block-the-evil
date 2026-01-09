using Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


[DisallowMultipleComponent]
public class UIPageTab : MonoBehaviour
{
    /// <summary>
    /// 点击回调
    /// </summary>
    public Action<GameObject, int> TabChangeEvent;
    /// <summary>
    /// 页签组
    /// </summary>
    public List<PageTabContent> ListTab;

    [Serializable]
    public class PageTabContent
    {
        [Serializable]
        public class SpecialState
        {
            public GameObject SpecialButton;
            [Header("普通状态时候图片")]
            public string NormalIcon;
            [Header("选中时候图片")]
            public string SelectIcon;
        }

        [Header("页签按钮组名字")]
        public string Name;
        [Header("选中页签缩放参数")]
        public float Scale = 1f;
        [Header("普通状态时候图片")]
        public string NormalIcon;
        [Header("选中时候图片")]
        public string SelectIcon;
        /// <summary>
        /// 未选中颜色
        /// </summary>
        public Color normalColor = new Color(0.6f, 0.6f, 0.6f);
        /// <summary>
        /// 选中颜色
        /// </summary>
        public Color selectColor = Color.white;
        /// <summary>
        /// 是否自动改变文本颜色
        /// </summary>
        public bool autoTxtColor;
        /// <summary>
        /// 当前选中页签
        /// </summary>
        [NonSerialized, HideInInspector]
        public GameObject CurrTabbutton;
        /// <summary>
        /// 页签物体链表
        /// </summary>
        public List<GameObject> PageTabList;
    }

    /// <summary>
    /// 监听Tab事件
    /// </summary>
    /// <param name="tabName"></param>
    /// <param name="onChange"></param>
    public void AddTabChangeEvent(string tabName, Action<GameObject, int> onChange)
    {
        var tab = FindTabContent(tabName);
        if (!ReferenceEquals(tab,null))
        {
            SetTabChangeEvent(tab, onChange);
        }
    }

    /// <summary>
    /// 获取单个按钮组数据
    /// </summary>
    /// <param name="tabName"></param>
    /// <returns></returns>
    private PageTabContent FindTabContent(string tabName)
    {
        if (!ReferenceEquals(ListTab,null))
        {
            foreach (var tabContent in ListTab)
            {
                if (tabContent.Name == tabName)
                {
                    return tabContent;
                }
            }
        }
        Debug.LogErrorFormat("called FindTabContent but does not exist,PageTab name:{0}", tabName);
        return null;
    }

    private void SetTabChangeEvent(PageTabContent tab, Action<GameObject, int> onChange)
    {
        for (var index = 0; index < tab.PageTabList.Count; index++)
        {
            var idx = index;
            var go = tab.PageTabList[index];
            var btn = go.GetComponent<Button>();
            if (!ReferenceEquals(btn,null))
            {
                btn.onClick.RemoveAllListeners();
                UnityEngine.Events.UnityAction action = () =>
                {
                    if (!ReferenceEquals(tab.CurrTabbutton,btn.gameObject))
                    {
                        onChange?.Invoke(btn.gameObject, idx);
                        tab.CurrTabbutton = btn.gameObject;
                        SetTabState(tab, btn.gameObject, idx);
                    }
                };
                btn.onClick.AddListener(action);
            }
            else
            {
                var btnEx = go.GetComponent<UIButtonExtension>();
                if (!ReferenceEquals(btnEx, null))
                {
                    btnEx.onClick.RemoveAllListeners();
                    UnityEngine.Events.UnityAction action = () =>
                    {
                        if (!ReferenceEquals(tab.CurrTabbutton,btnEx.gameObject))
                        {
                            onChange?.Invoke(btnEx.gameObject, idx);
                            tab.CurrTabbutton = btnEx.gameObject;
                            SetTabState(tab, btnEx.gameObject, idx);
                        }
                    };
                    btnEx.onClick.AddListener(action);
                }
                else
                {
                    var tog = go.GetComponent<Toggle>();
                    if (!ReferenceEquals(tog, null))
                    {
                        tog.onValueChanged.RemoveAllListeners();
                        UnityEngine.Events.UnityAction<bool> action = (isOn) =>
                        {
                            if (isOn && !ReferenceEquals(tog.gameObject,tab.CurrTabbutton))
                            {
                                tab.CurrTabbutton = tog.gameObject;
                                onChange(tog.gameObject, idx);
                            }
                        };
                        tog.onValueChanged.AddListener(action);
                    }
                    else
                    {
                        var listener = EventTriggerListener.Get(go);
                        EventTriggerListener.PointerEventDelegate action = (obj, eventData) =>
                        {
                            if (!ReferenceEquals(tab.CurrTabbutton,obj))
                            {
                                onChange?.Invoke(btn.gameObject, idx);
                                tab.CurrTabbutton = btn.gameObject;
                                SetTabState(tab, btn.gameObject, idx);
                            }
                        };
                        listener.onPointerClick += action;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 非toggle 自己设置选中状态
    /// </summary>
    /// <param name="tabContent"></param>
    /// <param name="go"></param>
    /// <param name="index"></param>
    private void SetTabState(PageTabContent tabContent, GameObject go, int index)
    {
        if (!ReferenceEquals(tabContent.PageTabList,null))
        {
            foreach (var tab in tabContent.PageTabList)
            {
                var NormalIcon = tabContent.NormalIcon;
                var SelectIcon = tabContent.SelectIcon;
                var ex = tab.GetComponent<UIButtonExtension>();
                var imgRes = String.Empty;
                var bSelect = false;
                var trans = tab.transform;
                if (ReferenceEquals(tab,go))
                {
                    // 被选中
                    trans.localScale = Vector3.one * tabContent.Scale;
                    imgRes = SelectIcon;
                    bSelect = true;
                    var txt = trans.GetComponentInChildren<CustomText>();
                    if (!ReferenceEquals(txt,null) && tabContent.autoTxtColor)
                    {
                        txt.color = tabContent.selectColor;
                    }
                }
                else
                {
                    // 没被选中
                    trans.localScale = Vector3.one;
                    imgRes = NormalIcon;
                    var txt = trans.GetComponentInChildren<CustomText>();
                    if (!ReferenceEquals(txt,null) && tabContent.autoTxtColor)
                    {
                        txt.color = tabContent.normalColor;
                    }
                }
                if (!ReferenceEquals(ex,null))
                {
                    ex.SetSelect(bSelect);
                }


                if (!string.IsNullOrEmpty(imgRes))
                {
                    var img = tab.GetComponent<Image>();
                    if (!ReferenceEquals(img,null))
                    {
                        ResourceManagerNew.instance.LoadAssetAsync(imgRes, delegate (Sprite sprite)
                        {
                            img.sprite = sprite;
                            img.SetNativeSize();
                        });
                    }
                }
            }
        }
    }

    /// <summary>
    /// 选中具体某个下标
    /// </summary>
    /// <param name="tabName"></param>
    /// <param name="index"></param>
    public void OnTab(string tabName, int index)
    {
        ResetCurrTabbutton(tabName);
        var tab = FindTabContent(tabName);
        SelectPageTab(index, tab);
    }

    private void SelectPageTab(int index, PageTabContent tab)
    {
        GameObject go = null;
        for (var idx = 0; idx < tab.PageTabList.Count; idx++)
        {
            go = tab.PageTabList[idx];
            var t = go.GetComponent<Toggle>();
            if (t)
            {
                t.isOn = index == idx;
            }
            else
            {
                go.transform.localScale = index == idx ? Vector3.one * tab.Scale : Vector3.one;
                var b = go.GetComponent<Button>();
                if (b)
                {
                    if (index == idx)
                    {
                        b.OnPointerClick(new PointerEventData(EventSystem.current));
                    }
                }
                else
                {
                    var ex = go.GetComponent<UIButtonExtension>();
                    if (ex)
                    {
                        if (index == idx)
                        {
                            ex.Press(false);
                        }
                    }
                    else
                    {
                        if (index == idx)
                        {
                            var l = go.GetComponent<EventTriggerListener>();
                            l.OnPointerClick(new PointerEventData(EventSystem.current));
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 重置当前按钮状态
    /// </summary>
    /// <param name="tabName"></param>
    public void ResetCurrTabbutton(string tabName)
    {
        if (ListTab != null)
        {
            foreach (PageTabContent tabContent in ListTab)
            {
                if (tabContent.Name == tabName)
                {
                    tabContent.CurrTabbutton = null;
                }
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var tab in ListTab)
        {
            tab.CurrTabbutton = null;
            if (tab.PageTabList != null)
            {
                tab.PageTabList.Clear();
                tab.PageTabList = null;
            }
        }
        ListTab.Clear();
        ListTab = null;
    }
}
