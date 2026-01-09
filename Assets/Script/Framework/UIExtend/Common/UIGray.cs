using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIGray : MonoBehaviour
{
    [Header("字体灰度值")]
    public float m_fontGrayValue = 0.8f;
    [Header("图片灰度值")]
    public float m_textrueGrayValue = 0.5f;
    [Header("点击区域宽高,如果不填写就用当前节点的")]
    public Vector2 m_sizeData = Vector2.zero;
    [Header("是否开始置灰")]
    public bool isStarGray = false;
    [Header("置灰点击是否显示暂未开启")]
    public bool isAddClick = false;
    [Header("是否使用新版灰化")]
    public bool useGrayNew = false;

    [Header("不改变grayMaskColor")]
    private bool isChangeGrayMaskColor = false;

    private static Color tempColor = new (0, 0, 0, 255);
    private static Color newGrayColor = new (0, 0, 1, 255);
    private static Color newGrayTextColor = new (255, 255, 255, 200);
    private static Color newGrayShadowColor = new (50, 50, 50, 255);
    private static Color grayMaskColor = new (1, 1, 1, 0.01f);

    private Dictionary<Text, Color> initColorDic = new ();
    private Dictionary<Shadow, Color> initCustomDic = new ();
    private Dictionary<MaskableGraphic, Color> initImgColorDic = new ();
    /// <summary>
    /// 是否更新所有颜色
    /// </summary>
    private bool m_IsUpdateColor = false;

    void Awake()
    {
        UpdateAllColor();
        if (isStarGray)
        {
            DoGray(true, isAddClick);
        }
    }

    void Start()
    {

    }

    void OnDestroy()
    {
        initColorDic.Clear();
        initColorDic = null;
        initCustomDic.Clear();
        initCustomDic = null;
        initImgColorDic.Clear();
        initImgColorDic = null;
    }

    [ContextMenu("灰化")]
    void Gray()
    {
        DoGray(true, false);
    }

    [ContextMenu("灰化并添加Mask")]
    void GrayAddMask()
    {
        DoGray(true, true);
    }

    [ContextMenu("恢复不置灰")]
    void UnGray()
    {
        DoGray(false);
    }

    /// <summary>
    /// 灰化
    /// </summary>
    /// <param name="isDark">是否变暗</param>
    /// <param name="isAddMask">是否添加mask</param>
    public void DoGray(bool isDark, bool isAddMask = false)
    {
        m_IsUpdateColor = isDark;
        var enuColor = initColorDic.GetEnumerator();
        while (enuColor.MoveNext())
        {
            if (isDark)
            {
                if (useGrayNew)
                {
                    enuColor.Current.Key.color = newGrayTextColor;
                }
                else
                {
                    enuColor.Current.Key.color = enuColor.Current.Value * m_fontGrayValue;
                }
            }
            else
            {
                enuColor.Current.Key.color = enuColor.Current.Value;
            }
        }

        var customColor = initCustomDic.GetEnumerator();
        while (customColor.MoveNext())
        {
            if (isDark)
            {
                if (useGrayNew)
                {
                    customColor.Current.Key.effectColor = newGrayShadowColor;
                }
                else
                {
                    customColor.Current.Key.effectColor = customColor.Current.Value * m_fontGrayValue;
                }
            }
            else
            {
                customColor.Current.Key.effectColor = customColor.Current.Value;
            }
        }

        var enuImgColor = initImgColorDic.GetEnumerator();
        while (enuImgColor.MoveNext())
        {
            if (isDark)
            {
                if (useGrayNew)
                {
                    enuImgColor.Current.Key.color = newGrayColor;
                }
                else
                {
                    tempColor.r = enuImgColor.Current.Value.r * m_textrueGrayValue;
                    tempColor.g = enuImgColor.Current.Value.g * m_textrueGrayValue;
                    tempColor.b = enuImgColor.Current.Value.b * m_textrueGrayValue;
                    tempColor.a = enuImgColor.Current.Value.a;
                    enuImgColor.Current.Key.color = tempColor;
                }
            }
            else
            {
                enuImgColor.Current.Key.color = enuImgColor.Current.Value;
            }
        }

        RectTransform rt = gameObject.transform as RectTransform;
        GameObject maskObj = null;
        Transform maskTrans = rt.Find("graymask");
        if (maskTrans)
        {
            maskObj = maskTrans.gameObject;
        }
        if (isDark)
        {
            if (isAddMask)
            {
                if (null == maskObj)
                {
                    maskObj = new GameObject("graymask");
                    Image mskImg = maskObj.AddComponent<Image>();
                    mskImg.color = grayMaskColor;
                    maskObj.transform.SetParent(rt);
                    RectTransform rtMask = maskObj.transform as RectTransform;
                    if (m_sizeData.x > 0 && m_sizeData.y > 0)
                        rtMask.sizeDelta = m_sizeData;
                    else
                        rtMask.sizeDelta = rt.sizeDelta;
                    rtMask.anchorMax = rt.anchorMax;
                    rtMask.anchorMin = rt.anchorMin;
                    rtMask.pivot = rt.pivot;
                    rtMask.localPosition = Vector3.zero;
                    rtMask.localScale = Vector3.one;

                    EventTrigger trigger = maskObj.AddComponent<EventTrigger>();
                    EventTrigger.Entry en = new EventTrigger.Entry();
                    en.eventID = EventTriggerType.PointerClick;
                    UnityAction<BaseEventData> pointerClick = new UnityAction<BaseEventData>(OnPointerClick);
                    en.callback.AddListener(pointerClick);
                    trigger.triggers.Add(en);
                }
                if (isChangeGrayMaskColor)
                {
                    Image temp = maskObj.GetComponent<Image>();
                    temp.color = grayMaskColor;
                }
                maskObj.SetActive(true);
            }
            else
            {
                if (maskObj)
                {
                    maskObj.SetActive(false);
                }
            }
        }
        else
        {
            if (maskObj)
            {
                maskObj.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 更新所有颜色；【在置灰前如果通过代码修改过颜色，需要调用此方法重新覆盖初始颜色，取消置灰时才能正常表现】
    /// </summary>
    public void UpdateAllColor()
    {
        if(m_IsUpdateColor)  return;

        initColorDic.Clear();
        initCustomDic.Clear();
        initImgColorDic.Clear();

        Text[] texts = this.gameObject.GetComponentsInChildren<CustomText>(true);
        foreach (var txt in texts)
        {
            initColorDic.Add(txt, new Color(txt.color.r, txt.color.g, txt.color.b, txt.color.a));
        }
        Shadow[] cls = this.gameObject.GetComponentsInChildren<Shadow>(true);
        foreach (var cl in cls)
        {
            initCustomDic.Add(cl, new Color(cl.effectColor.r, cl.effectColor.g, cl.effectColor.b, cl.effectColor.a));
        }

        Image[] imgs = this.gameObject.GetComponentsInChildren<Image>(true);
        foreach (var img in imgs)
        {
            initImgColorDic.Add(img, new Color(img.color.r, img.color.g, img.color.b, img.color.a));
        }

        RawImage[] rImgs = this.gameObject.GetComponentsInChildren<RawImage>(true);
        foreach (var rImg in rImgs)
        {
            initImgColorDic.Add(rImg, new Color(rImg.color.r, rImg.color.g, rImg.color.b, rImg.color.a));
        }
    }

    void OnPointerClick(BaseEventData baseEventData)
    {
        UIManager.Instance.ShowPromptWindow("[LID:696]");
    }
}
