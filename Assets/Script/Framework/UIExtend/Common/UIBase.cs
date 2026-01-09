using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UILayers;

[DisallowMultipleComponent]
public class UIBase : MonoBehaviour, IUIDepthDataEntry
{
    [NonSerialized]
    public string UiName = string.Empty;

    [NonSerialized]
    public UIType CurUIType = UIType.None;

    [Header("需要操作的GO列表")]
    public UIBaseItem[] TargetContainer;

    private readonly Dictionary<string, GameObject> _mDicNameToGameObject = new();

    private bool _mIsInit = false;

    public HashSet<IUIAdaptation> SetUIOffset;

    /// <summary>
    /// 是否需要全面屏便宜
    /// </summary>
    [HideInInspector]
    public bool IsNeedFullScreenSkew = false;

    /// <summary>
    /// 偏移的屏幕坐标
    /// </summary>
    private Vector2 _skewVector2 = new (0, -50);

    /// <summary>
    /// 是否开启界面虚化效果
    /// </summary>
    [HideInInspector]
    public bool IsOpenBlur = false;

    protected virtual void Awake()
    {
        Init();
    }

    protected virtual void Start() 
    {}

    private void Init()
    {
        if (_mIsInit)
            return;
        _mIsInit = true;
        if (!ReferenceEquals(TargetContainer,null) && TargetContainer.Length > 0)
        {
            for (int i = 0, iLength = TargetContainer.Length; i < iLength; i++)
            {
                var baseItem = TargetContainer[i];
                _mDicNameToGameObject[baseItem.goName] = baseItem.goNode;
                if (ReferenceEquals(baseItem.goNode, null))
                {
                    Debug.LogErrorFormat("{0}, {1}  goNode is null ", transform.name, baseItem.goName);
                }
            }
        }
    }

    public void BindItemNodeAndInit()
    {
        NodeBinder.BindNodes(this, this);
        InitOnce();
    }

    /// <summary>
    /// 首次创建UI完毕后调用，做界面绑定等操作
    /// </summary>
    public virtual void InitOnce() {}


    /// <summary>
    /// 预加载界面时调用
    /// </summary>
    /// <param name="param"></param>
    public virtual void OnPreload(object param = null) { }

    /// <summary>
    /// 初始化界面等操作,每次打开均调用
    /// </summary>
    public virtual void OnOpen(object param = null) { }

    /// <summary>
    /// 只关闭不销毁时调用
    /// </summary>
    public virtual void OnClose() 
    {}

    protected virtual void OnInitUIBaseItem() 
    {
        NodeBinder.BindNodes(this, this);
    }

    /// <summary>
    /// 关闭界面的方法
    /// </summary>
    /// <param name="go"></param>
    /// <param name="eventData"></param>
    protected virtual void OnBackClick(GameObject go = null, PointerEventData eventData = null)
    {
        UIManager.Instance.HideUI(this);
    }

    /// <summary>
    /// 销毁时调用
    /// </summary>
    protected virtual void OnDestroy()
    {
        TargetContainer = null;
        if (SetUIOffset != null)
        {
            SetUIOffset.Clear();
            SetUIOffset = null;
        }
    }

    public void AddUIBaseItem(List<GameObject> items)
    {
        if (ReferenceEquals(TargetContainer,null))
        {
            TargetContainer = new UIBaseItem[items.Count];
        }
        for (var i = 0; i < items.Count; i++)
        {
            TargetContainer[i] = new UIBaseItem
            {
                goName = items[i].name,
                goNode = items[i]
            };
        }
        OnInitUIBaseItem();
    }

    public T GetNodeByName<T>(string nodeName, bool isDebug = true) where T : Component
    {
        if (_mDicNameToGameObject.TryGetValue(nodeName, out var target))
        {
            var t = target.GetComponent<T>();
            if (!ReferenceEquals(t, null))
            {
                return t;
            }
            if (isDebug)
                Debug.LogError($"节点名字不存在!!!!! {nodeName}");
            return default;
        }
        if (isDebug)
            Debug.LogError($"节点名字不存在!!!!! {nodeName}");
        return default;
    }

    public GameObject GetNodeByName(string nodeName, bool isDebug = true)
    {
        if (_mDicNameToGameObject.TryGetValue(nodeName, out var target))
        {
            if (!ReferenceEquals(target, null))
            {
                return target;
            }
            if (isDebug)
                Debug.LogError($"节点名字不存在!!!!! {nodeName}");
            return null;
        }
        if (isDebug)
            Debug.LogError($"节点名字不存在!!!!! {nodeName}");
        return null;
    }

    /// <summary>
    /// 监听事件基类
    /// </summary>
    /// <param name="type">监听类型</param>
    /// <param name="nodeName">节点名字</param>
    /// <param name="call">回调</param>
    protected void AddListener(ui_listener_type type, string nodeName, Action<GameObject, PointerEventData> call)
    {
        var gameObject = GetNodeByName(nodeName);
        if (ReferenceEquals(gameObject,null)) 
        {
            Debug.LogError($"节点名为：{nodeName}未找到，请查证！！！");
            return;
        }
        switch (type)
        {
            case ui_listener_type.onClick:
                AddClickListener(gameObject, call);
                break;
            case ui_listener_type.onButtonPointerDown:
                AddButtonPointerDownClickListener(gameObject, call);
                break;
            case ui_listener_type.onButtonPointerUp:
                AddButtonPointerUpClickListener(gameObject, call);
                break;
            case ui_listener_type.onBeginDrag:
                AddBeginDragListener(gameObject, call);
                break;
            case ui_listener_type.onEndDrag:
                AddEndDragListener(gameObject, call);
                break;
            case ui_listener_type.onDrop:
                AddOnDragListener(gameObject, call);
                break;
            case ui_listener_type.onButtonBeginDrag:
                AddButtonBeginDragListener(gameObject, call);
                break;
            case ui_listener_type.onButtonEndDrag:
                AddButtonEndDragListener(gameObject, call);
                break;
            case ui_listener_type.onButtonDrag:
                AddButtonOnDragListener(gameObject, call);
                break;
            case ui_listener_type.onLongClick:
                AddButtonLongClickListener(gameObject, call);
                break;
        }
    }

    #region 事件监听
    protected enum ui_listener_type
    {
        /// <summary>
        /// 点击监听
        /// </summary>
        onClick,
        /// <summary>
        /// 按钮按下
        /// </summary>
        onButtonPointerDown,
        /// <summary>
        /// 按钮松开
        /// </summary>
        onButtonPointerUp,
        /// <summary>
        /// 物体开始拖拽（可以不带ButtonEx组件）
        /// </summary>
        onBeginDrag,
        /// <summary>
        /// 结束拖拽（可以不带ButtonEx组件）
        /// </summary>
        onEndDrag,
        /// <summary>
        /// 拖动中
        /// </summary>
        onDrop,
        /// <summary>
        /// 按钮开始拖拽（必须带ButtonEx组件）
        /// </summary>
        onButtonBeginDrag,
        /// <summary>
        /// 按钮结束拖拽（必须带ButtonEx组件）
        /// </summary>
        onButtonEndDrag,
        /// <summary>
        /// 按钮拖拽中（必须带ButtonEx组件）
        /// </summary>
        onButtonDrag,
        /// <summary>
        /// 长按
        /// </summary>
        onLongClick,
    }

    private void AddClickListener(GameObject curGo, Action<GameObject, PointerEventData> call)
    {
        var newBtn = curGo.GetComponent<UIButtonExtension>();

        void listenerFunc()
        {
            call?.Invoke(curGo,null);
        }

        if (!ReferenceEquals(newBtn, null)) {
            newBtn.onClick.RemoveAllListeners();
            newBtn.onClick.AddListener(listenerFunc);
        }
        else
        {
            var oldBtn = curGo.GetOrAddComponent<UIButtonExtension>();
            oldBtn.onClick.RemoveAllListeners();
            oldBtn.onClick.AddListener(listenerFunc);
        }
    }

    private void AddButtonPointerDownClickListener(GameObject curGo, Action<GameObject, PointerEventData> call)
    {
        var btn = curGo.GetComponent<UIButtonExtension>();
        if (!ReferenceEquals(btn, null))
        {
            btn.onButtonPointerDown = (go, pointerEventData) =>
            {
                call?.Invoke(go, pointerEventData);
            };
        }
        else
        {
            var listener = EventTriggerListener.Get(curGo);
            listener.onPointerDown += (go, pointerEventData) =>
            {
                call?.Invoke(go, pointerEventData);
            };
        }
    }

    private void AddButtonPointerUpClickListener(GameObject curGo, Action<GameObject, PointerEventData> call)
    {
        var btn = curGo.GetComponent<UIButtonExtension>();
        if (!ReferenceEquals(btn, null))
        {
            btn.onButtonPointerUp = (go, pointerEventData) =>
            {
                call?.Invoke(go, pointerEventData);
            };
        }
        else
        {
            var listener = EventTriggerListener.Get(curGo);
            listener.onPointerUp += (go, pointerEventData) =>
            {
                call?.Invoke(go, pointerEventData);
            };
        }
    }

    private void AddBeginDragListener(GameObject curGo, Action<GameObject, PointerEventData> call)
    {
        var listener = EventTriggerListener.Get(curGo);
        listener.onBeginDrag += (go, pointerEventData) =>
        {
            call?.Invoke(go, pointerEventData);
        };
    }

    private void AddEndDragListener(GameObject curGo, Action<GameObject, PointerEventData> call)
    {
        var listener = EventTriggerListener.Get(curGo);
        listener.onEndDrag += (go, pointerEventData) =>
        {
            call?.Invoke(go, pointerEventData);
        };
    }

    private void AddOnDragListener(GameObject curGo, Action<GameObject, PointerEventData> call)
    {
        var listener = EventTriggerListener.Get(curGo);
        listener.onDrag += (go, pointerEventData) =>
        {
            call?.Invoke(go, pointerEventData);
        };
    }

    private void AddButtonBeginDragListener(GameObject curGo, Action<GameObject, PointerEventData> call)
    {
        var btn = curGo.GetComponent<UIButtonExtension>();
        if (!ReferenceEquals(btn, null))
        {
            btn.onButtonBeginDrag = (go, pointerEventData) =>
            {
                call?.Invoke(go, pointerEventData);
            };
        }
    }

    private void AddButtonEndDragListener(GameObject curGo, Action<GameObject, PointerEventData> call)
    {
        var btn = curGo.GetComponent<UIButtonExtension>();
        if (!ReferenceEquals(btn, null))
        {
            btn.onButtonEndDrag = (go, pointerEventData) =>
            {
                call?.Invoke(go, pointerEventData);
            };
        }
    }

    private void AddButtonOnDragListener(GameObject curGo, Action<GameObject, PointerEventData> call)
    {
        var btn = curGo.GetComponent<UIButtonExtension>();
        if (!ReferenceEquals(btn, null))
        {
            btn.onButtonDrag = (go, pointerEventData) =>
            {
                call?.Invoke(go, pointerEventData);
            };
        }
    }

    private void AddButtonLongClickListener(GameObject curGo, Action<GameObject, PointerEventData> call)
    {
        var btn = curGo.GetComponent<UIButtonExtension>();
        if (!ReferenceEquals(btn, null))
        {
            void listenerFunc()
            {
                call?.Invoke(curGo, null);
            }
            btn.onLongClick.RemoveAllListeners();
            btn.onLongClick.AddListener(listenerFunc);
        }
    }
    #endregion

    #region 页签相关

    /// <summary>
    /// 添加页签监听事件
    /// </summary>
    /// <param name="parentPath">UIPageTab组件节点</param>
    /// <param name="tabName">页签名</param>
    /// <param name="call">点击回调</param>
    protected void AddPageTabChangeEvent(string parentPath, string tabName, Action<GameObject, int> call)
    {
        var uiPageTap = GetNodeByName<UIPageTab>(parentPath);
        if (ReferenceEquals(uiPageTap, null))
        {
            Debug.LogError($"节点名为：{parentPath}未找到UIPageTab组件，请查证！！！");
            return;
        }
        uiPageTap.AddTabChangeEvent(tabName, call);
    }

    /// <summary>
    /// 选中页签
    /// </summary>
    /// <param name="parentPath">UIPageTab组件节点</param>
    /// <param name="tabName">页签名</param>
    /// <param name="call">选中的下标</param>
    protected void SelectPageTable(string parentPath, string tabName, int index)
    {
        var uiPageTap = GetNodeByName<UIPageTab>(parentPath);
        if (ReferenceEquals(uiPageTap, null))
        {
            Debug.LogError($"节点名为：{parentPath}未找到UIPageTab组件，请查证！！！");
            return;
        }
        uiPageTap.OnTab(tabName, index);
    }

    #endregion

    [NonSerialized]
    public Canvas mCanvas;
    [NonSerialized]
    public bool mHasCanvas;

    public int mBaseSortingOrder { get; set; }
    public int mMaxSortingOrder { get; set; }

    public void SetUILayer(UILayer uiLayer)
    {
        if (!ReferenceEquals(uiLayer, null))
            uiLayer.mIsUpdate = true;
        var rTrans = transform as RectTransform;
        if (rTrans)
        {
            rTrans.anchorMin = Vector2.zero; // 左下角锚点
            rTrans.anchorMax = Vector2.one; // 右上角锚点
            rTrans.localEulerAngles = Vector3.zero;
            rTrans.localScale = Vector3.one;
            rTrans.offsetMin = Vector2.zero;
            if(IsNeedFullScreenSkew)
/*            if (GameManager.Instance.PLATFROM == Platfrom.Telegram 
                && GameManager.Instance.IsFullScreen && IsNeedFullScreenSkew)*/
            {
                rTrans.offsetMax = _skewVector2;
            }
            else
            {
                rTrans.offsetMax = Vector2.zero;
            }
        }
        //为了重置层级
        rTrans.SetAsLastSibling();

        if (SetUIOffset != null)
        {
            foreach (var item in SetUIOffset)
            {
                item.UIOffsetAdaptation();
            }
        }
    }
}

[Serializable]
public class UIBaseItem
{
    public string goName;
    public GameObject goNode;
}