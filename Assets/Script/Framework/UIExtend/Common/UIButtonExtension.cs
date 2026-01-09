using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEditor;
using Framework;

public class UIButtonExtension : Selectable,IPointerClickHandler,ISubmitHandler,IBeginDragHandler,IEndDragHandler,IDragHandler
{
    [Header("点击缩放")]
    public float scaleRate = 0.98f;
    private float oldScale;

    public bool m_originSelect = false;

    [Header("普通状态")]
    public GameObject m_normalObj;
    [Header("选中状态")]
    public GameObject m_selectObj;
    [Header("音效ID")]
    public string m_audio = "buttonAudio";
    [Header("音效强度")]
    public float m_audioSound = 1f;
    
    [Header("长按等待时间")]
    public float onLongWaitTime = 1.5f;

    [Header("不断的产生长按事件")]
    public bool onLongContinue = false;

    [Header("是否开启长按,长按会忽略点击")]
    public bool isLongClicked = false;

    [Header("忽略自动生成的EventArea")]
    public bool ignoreEventArea = true;

    [Header("是否开启按钮冷却")]
    public bool isOpenCold = false;

    [Header("冷却时间")]
    public float ColdTime = 1f;

    [System.Serializable]
    public class ButtonClickedEvent : UnityEvent { }

    private ButtonClickedEvent m_OnClick = new ButtonClickedEvent();
    
    private ButtonClickedEvent m_OnLongClick = new ButtonClickedEvent();
    
    private ButtonClickedEvent m_OnDown = new ButtonClickedEvent();
    
    private ButtonClickedEvent m_OnUp = new ButtonClickedEvent();
    
    private ButtonClickedEvent m_OnEnter = new ButtonClickedEvent();
    
    private ButtonClickedEvent m_OnExit = new ButtonClickedEvent();

    private GameObject m_objEvent;

    private ScrollRect scrollRect;
    private bool isNeedLoadParentScroll = true;

    //带参数是为了方便取得绑定了UI事件的对象    
    public delegate void PointerEventDelegate(GameObject go, PointerEventData eventData);

    public PointerEventDelegate onButtonPointerUp;
    public PointerEventDelegate onButtonPointerDown;

    public PointerEventDelegate onButtonBeginDrag;
    public PointerEventDelegate onButtonEndDrag;
    public PointerEventDelegate onButtonDrag;
    protected UIButtonExtension() : base() {}
    protected override void  Awake() {
        SetSelect(m_originSelect);
        this.transition = Transition.None;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        interactable = true;
    }

    private ScrollRect parentScroll
    {
        get
        {
            if (!isNeedLoadParentScroll) return scrollRect;

            if (transform.parent != null)
            {
                isNeedLoadParentScroll = false;
                scrollRect = transform.GetComponentInParent<ScrollRect>();
            }

            return scrollRect;
        }
    }

    /// <summary>  
    /// 文本值  
    /// </summary>  
    public string text
    {
        get
        {
            Text v = getText();
            if (v != null)
                return v.text;
            return null;
        }
        set
        {
            Text v = getText();
            if (v != null)
                v.text = value;
        }
    }

    private bool isPointerDown = false;
    private bool isPointerInside = false;
    private bool doLongClick = false;
    private bool isDragging = false;

    /// <summary>  
    /// 是否被按下  
    /// </summary>  
    public bool isDown
    {
        get
        {
            return isPointerDown;
        }
    }

    /// <summary>  
    /// 是否进入  
    /// </summary>  
    public bool isEnter
    {
        get
        {
            return isPointerInside;
        }
    }

    /// <summary>  
    /// 点击事件  
    /// </summary>  
    public ButtonClickedEvent onClick
    {
        get { return m_OnClick; }
        set { m_OnClick = value; }
    }

    /// <summary>  
    /// 长按事件  
    /// </summary>  
    public ButtonClickedEvent onLongClick
    {
        get { return m_OnLongClick; }
        set { m_OnLongClick = value; }
    }

    /// <summary>  
    /// 按下事件  
    /// </summary>  
    public ButtonClickedEvent onDown
    {
        get { return m_OnDown; }
        set { m_OnDown = value; }
    }

    /// <summary>  
    /// 松开事件  
    /// </summary>  
    public ButtonClickedEvent onUp
    {
        get { return m_OnUp; }
        set { m_OnUp = value; }
    }

    /// <summary>  
    /// 进入事件  
    /// </summary>  
    public ButtonClickedEvent onEnter
    {
        get { return m_OnEnter; }
        set { m_OnEnter = value; }
    }

    /// <summary>  
    /// 离开事件  
    /// </summary>  
    public ButtonClickedEvent onExit
    {
        get { return m_OnExit; }
        set { m_OnExit = value; }
    }

    private void SetScale(float scale)
    {
        Vector3 vScale = Vector3.one;
        vScale.x = scale;
        vScale.y = scale;
        vScale.z = scale;

        this.gameObject.transform.localScale = vScale;
    }

    public void SetSelect(bool bSelect)
    {
        if (bSelect)
        {
            SetSelectActive(m_normalObj, false);
            SetSelectActive(m_selectObj, true);
        }
        else
        { 
            SetSelectActive(m_normalObj, true);
            SetSelectActive(m_selectObj, false);
        }
    }

    private void SetSelectActive(GameObject go, bool s)
    {
        if (go)
        {
            go.SetActive(s);
        }
    }

    private Text getText()
    {
        Transform f = transform.Find("Text");
        Text v = null;
        if (f != null)
        {
            v = f.gameObject.GetComponent<CustomText>();
        }
        if (v == null && transform.childCount > 0)
        {
            GameObject obj = transform.GetChild(0).gameObject;
            v = obj.GetComponent<CustomText>();
        }
        return v;
    }

    public void Press(bool bAudio = true)
    {
        if (!IsActive() || !IsInteractable())
            return;

        // 长按忽略按下事件
        if (isLongClicked) {
            isLongClicked = false;
            return;
        }

        if (interactable)
        {
            m_OnClick.Invoke();
            if (isOpenCold)
            {
                interactable = false;
                if (gameObject.activeSelf)
                    StartCoroutine(clickTimer());
            }
        }

        if (bAudio && !string.IsNullOrEmpty(m_audio))// 播放按钮音效       
        {
            AudioManagerNew.Instance.PlayAudio(m_audio, m_audioSound);
        }
    }

    private float clickDownTime = 0f;

    private IEnumerator clickTimer()
    {
        clickDownTime = Time.time;
        while (!interactable)
        {
            if (Time.time - clickDownTime >= ColdTime)
            {
                clickDownTime = Time.time;
                interactable = true;
                yield break;
            }
            else
                yield return null;
        }
    }

    private void Down(PointerEventData eventData)
    {
        if (!IsActive() || !IsInteractable())
            return;

        oldScale = gameObject.transform.localScale.x;
        SetScale(scaleRate);

        if (!ignoreEventArea)
        {
            if (m_objEvent == null)
            {
                m_objEvent = new GameObject("EventArea");
                RectTransform rt = m_objEvent.AddComponent<RectTransform>();
                var rtOld = transform as RectTransform;
                m_objEvent.transform.SetParent(this.transform);
                rt.anchorMax = rtOld.anchorMax;
                rt.anchorMin = rtOld.anchorMin;
                rt.pivot = rtOld.pivot;
                rt.localPosition = Vector3.zero;
                rt.localScale = Vector3.one;
                rt.sizeDelta = rtOld.sizeDelta;
                m_objEvent.AddComponent<CanvasRenderer>();
                m_objEvent.AddComponent<EmptyImage4Touch>();
            }

            Vector3 vScale = Vector3.one;
            float s = oldScale / scaleRate;
            vScale.x = s;
            vScale.y = s;
            vScale.z = s;

            m_objEvent.transform.localScale = vScale;
        }

        m_OnDown.Invoke();
        doLongClick = true;
        StartCoroutine(grow());
        if (onButtonPointerDown != null) onButtonPointerDown(gameObject, eventData);
    }

    private void Up(PointerEventData eventData)
    {
        if (!IsActive() || !IsInteractable() || !isDown)
            return;

        SetScale(oldScale);

        if (!ignoreEventArea)
        {
            m_objEvent.transform.localScale = Vector3.one;
        }

        m_OnUp.Invoke();
        if (onButtonPointerUp != null) onButtonPointerUp(gameObject, eventData);
    }

    private void Enter()
    {
        if (!IsActive())
            return;
        m_OnEnter.Invoke();
    }

    private void Exit()
    {
        if (!IsActive() || !isEnter)
            return;
        m_OnExit.Invoke();
    }

    private void LongClick()
    {
        if (!IsActive() || !isDown)
            return;

        isLongClicked = true;
        m_OnLongClick.Invoke();
    }

    private float downTime = 0f;
    private IEnumerator grow()
    {
        downTime = Time.time;
        while (isDown && doLongClick)
        {
            if (Time.time - downTime > onLongWaitTime)
            {
                LongClick();
                if (onLongContinue)
                    downTime = Time.time;
                else
                    break;
            }
            else
                yield return null;
        }
    }

    protected override void OnDisable()
    {
        isPointerDown = false;
        isPointerInside = false;
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (isDragging)
            return;

        Press();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        isPointerDown = true;
        Down(eventData);
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        Up(eventData);
        isPointerDown = false;
        base.OnPointerUp(eventData);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        isPointerInside = true;
        Enter();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        Exit();
        isPointerInside = false;
        base.OnPointerExit(eventData);
        if (isLongClicked)
        {
            isLongClicked = false;
        }
    }

    public virtual void OnSubmit(BaseEventData eventData)
    {
        Press();

        if (!IsActive() || !IsInteractable())
            return;

        DoStateTransition(SelectionState.Pressed, false);
        StartCoroutine(OnFinishSubmit());
    }

    private IEnumerator OnFinishSubmit()
    {
        var fadeTime = colors.fadeDuration;
        var elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        DoStateTransition(currentSelectionState, false);
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (!isDragging)
        {
            isDragging = true;
        }
        if (doLongClick)
        {
            doLongClick = false;
        }
        if (onButtonBeginDrag != null)
        {
            onButtonBeginDrag(gameObject, eventData);
        }
        if (parentScroll != null)
        {
            parentScroll.OnBeginDrag(eventData);
        }
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (isDragging)
        {
            isDragging = false;
        }
        if (onButtonEndDrag != null)
        {
            onButtonEndDrag(gameObject, eventData);
        }
        if (parentScroll != null)
        {
            parentScroll.OnEndDrag(eventData);
        }
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (onButtonDrag != null)
        {
            onButtonDrag(gameObject, eventData);
        }
        if (parentScroll != null)
        {
            parentScroll.OnDrag(eventData);
        }
    }

    public static GameObject CreateUIElementRoot(string name, Vector2 size)
    {
        GameObject go = new GameObject(name);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        return go;
    }
    public static GameObject CreateUIElementRoot(string name, float w, float h)
    {
        return CreateUIElementRoot(name, new Vector2(w, h));
    }

    public static GameObject CreateUIText(string name, string text, GameObject parent)
    {
        GameObject childText = CreateUIObject(name, parent);
        Text v = childText.AddComponent<CustomText>();
        v.text = text;
        v.alignment = TextAnchor.MiddleCenter;
        SetDefaultTextValues(v);

        RectTransform r = childText.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.sizeDelta = Vector2.zero;

        return childText;
    }

    public static GameObject CreateUIObject(string name, GameObject parent)
    {
        GameObject go = new GameObject(name);
        go.AddComponent<RectTransform>();
        SetParentAndAlign(go, parent);
        return go;
    }

    public static void SetDefaultTextValues(Text lbl)
    {
        lbl.color = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);
    }

    public static void SetParentAndAlign(GameObject child, GameObject parent)
    {
        if (parent == null)
            return;

        child.transform.SetParent(parent.transform, false);
        SetLayerRecursively(child, parent.layer);
    }
    public static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        Transform t = go.transform;
        for (int i = 0; i < t.childCount; i++)
            SetLayerRecursively(t.GetChild(i).gameObject, layer);
    }

    public static T findRes<T>(string name) where T : Object
    {
        T[] objs = Resources.FindObjectsOfTypeAll<T>();
        if (objs != null && objs.Length > 0)
        {
            foreach (Object obj in objs)
            {
                if (obj.name == name)
                    return obj as T;
            }
        }
        objs = AssetBundle.FindObjectsOfType<T>();
        if (objs != null && objs.Length > 0)
        {
            foreach (Object obj in objs)
            {
                if (obj.name == name)
                    return obj as T;
            }
        }
        return default(T);
    }

#if UNITY_EDITOR
    private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
    {
        GameObject parent = menuCommand.context as GameObject;
        if (parent == null || parent.GetComponentInParent<Canvas>() == null)
        {
            parent = GetOrCreateCanvasGameObject();
        }

        string uniqueName = GameObjectUtility.GetUniqueNameForSibling(parent.transform, element.name);
        element.name = uniqueName;
        Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);
        Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
        GameObjectUtility.SetParentAndAlign(element, parent);
        if (parent != menuCommand.context) // not a context click, so center in sceneview  
            SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());

        Selection.activeGameObject = element;

    }
#endif
    public static GameObject GetOrCreateCanvasGameObject()
    {
#if UNITY_EDITOR  
        GameObject selectedGo = Selection.activeGameObject;

        Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
        if (canvas != null && canvas.gameObject.activeInHierarchy)
            return canvas.gameObject;

        canvas = Object.FindObjectOfType(typeof(Canvas)) as Canvas;
        if (canvas != null && canvas.gameObject.activeInHierarchy)
            return canvas.gameObject;

        return CreateNewUI();
#else
        return null;
#endif
    }

#if UNITY_EDITOR  
    private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null && SceneView.sceneViews.Count > 0)
            sceneView = SceneView.sceneViews[0] as SceneView;

        if (sceneView == null || sceneView.camera == null)
            return;

        Vector2 localPlanePosition;
        Camera camera = sceneView.camera;
        Vector3 position = Vector3.zero;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPlanePosition))
        {
            // Adjust for canvas pivot  
            localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
            localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;

            localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
            localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);

            // Adjust for anchoring  
            position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
            position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;

            Vector3 minLocalPosition;
            minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
            minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;

            Vector3 maxLocalPosition;
            maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
            maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;

            position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
            position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
        }

        itemTransform.anchoredPosition = position;
        itemTransform.localRotation = Quaternion.identity;
        itemTransform.localScale = Vector3.one;
    }
#endif

    public static GameObject CreateNewUI()
    {
        var root = new GameObject("Canvas");
        root.layer = LayerMask.NameToLayer("UI");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        root.AddComponent<CanvasScaler>();
        root.AddComponent<GraphicRaycaster>();
#if UNITY_EDITOR  
        Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);
#endif
        CreateEventSystem(false, null);
        return root;
    }

    public static void CreateEventSystem(bool select, GameObject parent)
    {
#if UNITY_EDITOR  
        var esys = Object.FindObjectOfType<EventSystem>();
        if (esys == null)
        {
            var eventSystem = new GameObject("EventSystem");
            GameObjectUtility.SetParentAndAlign(eventSystem, parent);
            esys = eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
        }

        if (select && esys != null)
        {
            Selection.activeGameObject = esys.gameObject;
        }
#endif
    }


#if UNITY_EDITOR
    [MenuItem("GameObject/UI/ButtonEx")]
    static void CreateButtonEx(MenuCommand menuCmd)
    {
        // 创建游戏对象  
        float w = 160f;
        float h = 30f;
        GameObject btnRoot = CreateUIElementRoot("ButtonEx", w, h);

        // 创建Text对象  
        CreateUIText("Text", "Button", btnRoot);

        // 添加脚本  
        btnRoot.AddComponent<CanvasRenderer>();
        Image img = btnRoot.AddComponent<Image>();
        img.color = Color.white;
        img.fillCenter = true;
        img.raycastTarget = true;
        img.sprite = findRes<Sprite>("UISprite");
        if (img.sprite != null)
            img.type = Image.Type.Sliced;

        btnRoot.AddComponent<UIButtonExtension>();
        btnRoot.GetComponent<Selectable>().image = img;

        PlaceUIElementRoot(btnRoot, menuCmd);
    }
#endif


}