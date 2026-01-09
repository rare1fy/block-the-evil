using System;
using System.Collections.Generic;
using UnityEngine;

public interface IUIAdaptation
{
    void UIOffsetAdaptation();

    void PhoneAdaptation();
}

/// <summary>
/// 竖屏UI适配
/// </summary>
[DisallowMultipleComponent]
public class UIPortraitFitCompont : MonoBehaviour, IUIAdaptation
{
    /// <summary>
    /// 9:16分辨率宽高比(720比上1280的宽高比)
    /// </summary>
    public static float Judge9b16 = 0.5625f;

    /// <summary>
    /// 适配类型
    /// </summary>
    public EUIAdaptationType adaptationType = EUIAdaptationType.Scale;

    public UIPortraitAdaptationScale UIScaleData;

    public UIPortraitAdaptationOffset UIOffsetData;

    private static CustomAspect? _screenAspect;

    public static CustomAspect ScreenAspect
    {
        get
        {
#if UNITY_EDITOR
            var size = GetGameViewSize();
            _screenAspect = new CustomAspect((int)size.width, (int)size.height);
#else
            if (_screenAspect == null)
                _screenAspect = new CustomAspect(Screen.width, Screen.height);
#endif
            return _screenAspect.Value;
        }
    }

    public static bool Is9b16
    {
        get
        {
            return ScreenAspect.Aspect >= Judge9b16;
        }
    }

    private void Awake()
    {
        SetAdaptation();
    }

    /// <summary>
    /// 适配函数
    /// </summary>
    public void SetAdaptation()
    {
        switch (adaptationType)
        {
            case EUIAdaptationType.Scale:
                UIScaleAdaptation();
                break;
            case EUIAdaptationType.Offset:
                _initUIOffset();
                break;
            case EUIAdaptationType.ScaleOffset:
                UIScaleAdaptation();
                _initUIOffset();
                break;
        }
    }

    private void _initUIOffset()
    {
        if (UIOffsetData.BaseParent)
        {
            if (UIOffsetData.BaseParent.SetUIOffset == null)
            {
                UIOffsetData.BaseParent.SetUIOffset = new HashSet<IUIAdaptation>();
            }
            UIOffsetData.BaseParent.SetUIOffset.Add(this);
        }
        UIOffsetAdaptation();
    }

    /// <summary>
    /// 偏移适配
    /// </summary>
    public void UIOffsetAdaptation()
    {
        Vector3 v = UIOffsetData.OffsetOther;
        bool isFind = false;
        var datas = UIOffsetData.UIOffsetDatas;
        if (datas != null && datas.Count > 0)
        {
            for (int i = 0; i < datas.Count; i++)
            {
                var data = datas[i];
                if (data.Aspect == ScreenAspect || data.Aspect.Approximate(ScreenAspect))
                {
                    v = data.Adaptation;
                    isFind = true;
                    break;
                }
            }
        }
        if (!isFind)
        {
            v = UIOffsetData.OffsetOther;
            if (Is9b16)
            {
                v = UIOffsetData.Offset9b16;
            }
            else if (ScreenAspect.Aspect <= 0.5)
            {
                v = UIOffsetData.Offset1b2;
            }
        }
        transform.localPosition = v;
    }

    /// <summary>
    /// 缩放适配
    /// </summary>
    public void UIScaleAdaptation()
    {
        Vector3 v = Vector3.one;
        bool isFind = false;
        var datas = UIScaleData.UIScaleDatas;
        if (datas != null && datas.Count > 0)
        {
            for (int i = 0; i < datas.Count; i++)
            {
                var data = datas[i];
                if (data.Aspect == ScreenAspect || data.Aspect.Approximate(ScreenAspect))
                {
                    v = data.Adaptation;
                    isFind = true;
                    break;
                }
            }
        }

        if (!isFind)
        {
            v = Is9b16 ? UIScaleData.Scale9b16 : UIScaleData.ScaleUn9b16;
        }
        transform.localScale = v;
    }

    public void PhoneAdaptation()
    { }

    #region editor 获取 GameView 的分辨率
#if UNITY_EDITOR
    public struct GameViewSize
    {
        public float width;
        public float height;
        public float aspect;
    }

    static bool getGameViewSizeError = false;

    /// <summary>
    /// 获取 GameView 的分辨率
    /// </summary>
    /// <returns></returns>
    public static GameViewSize GetGameViewSize()
    {
        GameViewSize size = new();
        try
        {
            Type gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
            var GetSizeOfMainGameView = gameViewType.GetMethod("GetSizeOfMainGameView",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public 
                | System.Reflection.BindingFlags.NonPublic);

            Vector2 viewSize = (Vector2)GetSizeOfMainGameView.Invoke(null, null);

            size.width = viewSize.x; size.height = viewSize.y;
            size.aspect = size.width / size.height;
            return size;
        }
        catch (Exception e)
        {
            if (getGameViewSizeError == false)
            {
                Debug.LogError("GameCamera.GetGameViewSize - has a Unity update broken this?\nThis is not a fatal error !\n" + e.ToString());
                getGameViewSizeError = true;
            }
        }
        size.width = size.height = size.aspect = 0;
        return size;
    }
#endif
    #endregion
}

public enum EUIAdaptationType
{
    None = 0,
    /// <summary>
    /// UI缩放
    /// </summary>
    Scale,
    /// <summary>
    /// UI偏移
    /// </summary>
    Offset,
    /// <summary>
    /// 刘海适配
    /// </summary>
    Cutout,
    /// <summary>
    /// 缩放 + 刘海
    /// </summary>
    ScaleCutout,
    /// <summary>
    /// 偏移 + 缩放
    /// </summary>
    ScaleOffset,
    /// <summary>
    /// 刘海 + 偏移
    /// </summary>
    CutoutOffset,
    /// <summary>
    /// 缩放 + 刘海 + 偏移
    /// </summary>
    ScaleCutoutOffset,
}

[Serializable]
public struct UIAdaptationData
{
    [Header("分辨率")]
    public CustomAspect Aspect;
    [Header("精确参数")]
    public Vector3 Adaptation;
}

[Serializable]
public struct UIPortraitAdaptationScale
{
    [Header("9:16缩放(大于9:16的比率均被视作9:16)")]
    public Vector3 Scale9b16;

    [Header("其他 缩放")]
    public Vector3 ScaleUn9b16;

    [Header("精确分辨率配置缩放")]
    public List<UIAdaptationData> UIScaleDatas;
}

[Serializable]
public struct UIPortraitAdaptationOffset
{
    [Header("父物体UIBase节点")]
    public UIBase BaseParent;

    [Header("9:16 UI偏移适配(大于9:16的比率均被视作9:16)")]
    public Vector3 Offset9b16;

    [Header("小于等于1:2 UI偏移适配")]
    public Vector3 Offset1b2;

    [Header("其他分辨率UI偏移适配")]
    public Vector3 OffsetOther;

    [Header("精确分辨率配置偏移")]
    public List<UIAdaptationData> UIOffsetDatas;
}

[Serializable]
public struct CustomAspect
{
    [Header("屏幕宽高")]
    public Vector2 WH;

    public float Aspect
    {
        get
        {
            float t = WH.x / WH.y;
            if (t > 1.5f && t < 1.6f)
            {
                t = 1.5f;
            }
            return t;
        }
    }

    public CustomAspect(int w, int h)
    {
        WH.x = w;
        WH.y = h;
    }

    public static bool operator ==(CustomAspect a, CustomAspect b)
    {
        return a.WH.x * b.WH.y == b.WH.x * a.WH.y;
    }

    public static bool operator !=(CustomAspect a, CustomAspect b)
    {
        return a.WH.x * b.WH.y != b.WH.x * a.WH.y;
    }

    public bool Approximate(CustomAspect a)
    {
        return (Mathf.Abs(WH.x - a.WH.x) <= 2 && Mathf.Abs(WH.y - a.WH.y) <= 2)
            || Mathf.Abs(Aspect - a.Aspect) <= 0.02f;
    }
}

/// <summary>
/// 刘海数据
/// </summary>
[Serializable]
public class CutoutRect
{
    /// <summary>
    /// 是否获取到设备信息 -1 未获取到 0 非刘海 1刘海
    /// </summary>
    public int isCutout;

    /// <summary>
    /// 顶部状态栏的高度
    /// </summary>
    public int top;

    /// <summary>
    /// 底部状态栏的高度
    /// </summary>
    public int bottom;

    /// <summary>
    /// 左边状态栏的宽度
    /// </summary>
    public int left;

    /// <summary>
    /// 右边状态栏的宽度
    /// </summary>
    public int right;

    public CutoutRect()
    {
        isCutout = -1;
        top = 0;
        bottom = 0;
        left = 0;
        right = 0;
    }

    public CutoutRect(int c, int t, int b, int l, int r)
    {
        isCutout = c;
        top = Mathf.Clamp(t, 0, t);
        bottom = Mathf.Clamp(b, 0, b);
        left = Mathf.Clamp(l, 0, l);
        right = Mathf.Clamp(r, 0, r);
    }

    public bool IsZero()
    {
        return top + bottom + left + right == 0;
    }
}
