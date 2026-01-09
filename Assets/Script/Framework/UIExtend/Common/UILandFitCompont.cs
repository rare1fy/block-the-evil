using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// 横屏UI适配脚本
/// </summary>
public class UILandFitCompont : MonoBehaviour, IUIAdaptation
{
    /// <summary>
    /// ipad pro 分辨率1.44 也按4:3处理
    /// </summary>
    public static float Judge4b3 = 1.45f;

    /// <summary>
    /// 适配类型
    /// </summary>
    public EUIAdaptationType adaptationType = EUIAdaptationType.Cutout;

    /// <summary>
    /// 刘海屏适配方向
    /// </summary>
    public EUIAdaptationOrientation adaptationOrientation = EUIAdaptationOrientation.Left;

    public UILandAdaptationScale UIScaleData;

    public UILandAdaptationOffset UIOffsetData;

    private Transform mTran;

    /// <summary>
    /// 上次偏移
    /// </summary>
    private Vector3 mLastOffset = Vector3.zero;

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

    public static bool Is4b3
    {
        get
        {
            return ScreenAspect.Aspect < Judge4b3;
        }
    }

    // TODO 初始化的时候初始化
    public static void InitData()
    {
        mCutoutRect = Util.GetPhoneCutout(); // 刘海信息
        mIsFringe = mCutoutRect != null && !mCutoutRect.IsZero();
        mLastHomeRight = Screen.orientation == ScreenOrientation.LandscapeLeft; // home在右边
    }

    // TODO 建议放到Update中去每帧执行
    public static void UpdateUIAdaptation()
    {
        if (!mIsFringe)
            return;
        if (Screen.orientation != ScreenOrientation.LandscapeLeft &&
            Screen.orientation != ScreenOrientation.LandscapeRight)
            return;
        bool homeRight = Screen.orientation == ScreenOrientation.LandscapeLeft;
        if (homeRight != mLastHomeRight)
        {
            mLastHomeRight = homeRight;
            foreach (var item in mHashSetUIAdaptation)
            {
                item.PhoneAdaptation();
            }
        }
    }

    private void Awake()
    {
        SetAdaptation();
    }

    /// <summary>
    /// 设置适配
    /// </summary>
    public void SetAdaptation()
    {
        switch (adaptationType)
        {
            case EUIAdaptationType.Scale:
                UIScaleAdaptation();
                break;
            case EUIAdaptationType.Cutout:
                _initCutoutAdaptation();
                break;
            case EUIAdaptationType.Offset:
                _initUIOffset();
                break;
            case EUIAdaptationType.ScaleCutout:
                UIScaleAdaptation();
                _initCutoutAdaptation();
                break;
            case EUIAdaptationType.ScaleOffset:
                UIScaleAdaptation();
                _initUIOffset();
                break;
            case EUIAdaptationType.CutoutOffset:
                _initCutoutAdaptation();
                _initUIOffset();
                break;
            case EUIAdaptationType.ScaleCutoutOffset:
                UIScaleAdaptation();
                _initCutoutAdaptation();
                _initUIOffset();
                break;
        }
    }

    private void OnDestroy()
    {
        if (adaptationType == EUIAdaptationType.Cutout)
            mHashSetUIAdaptation.Remove(this);
        mTran = null;
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
            if (Is4b3)
            {
                v = UIOffsetData.Offset4b3;
            }
            else if (ScreenAspect.Aspect >= 2)
            {
                v = UIOffsetData.Offset2b1;
            }
        }

        transform.localPosition = v;
    }

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
            v = Is4b3 ? UIScaleData.Scale4b3 : UIScaleData.ScaleUn4b3;
        }
        transform.localScale = v;
    }

    private void _initCutoutAdaptation()
    {
        mTran = transform;
        mHashSetUIAdaptation.Add(this);
        if (!mIsFringe) return;
        PhoneAdaptation();
    }

    public void PhoneAdaptation()
    {
        if (mCutoutRect == null) { return; }

        if (mTran == null)
        {
            return;
        }

        Vector3 curOffset = Vector3.zero;
        switch (adaptationOrientation)
        {
            case EUIAdaptationOrientation.Left:
                curOffset.x += mLastHomeRight ? mCutoutRect.top : mCutoutRect.bottom;
                break;
            case EUIAdaptationOrientation.Top:
                curOffset.y -= mLastHomeRight ? mCutoutRect.right : mCutoutRect.left;
                break;
            case EUIAdaptationOrientation.Right:
                curOffset.x -= mLastHomeRight ? mCutoutRect.bottom : mCutoutRect.top;
                break;
            case EUIAdaptationOrientation.Bottom:
                curOffset.y += mLastHomeRight ? mCutoutRect.left : mCutoutRect.right;
                break;
            case EUIAdaptationOrientation.LeftTop:
                curOffset.x += mLastHomeRight ? mCutoutRect.top : mCutoutRect.bottom;
                curOffset.y -= mLastHomeRight ? mCutoutRect.right : mCutoutRect.left;
                break;
            case EUIAdaptationOrientation.LeftBottom:
                curOffset.x += mLastHomeRight ? mCutoutRect.top : mCutoutRect.bottom;
                curOffset.y += mLastHomeRight ? mCutoutRect.left : mCutoutRect.right;
                break;
            case EUIAdaptationOrientation.RightTop:
                curOffset.x -= mLastHomeRight ? mCutoutRect.bottom : mCutoutRect.top;
                curOffset.y -= mLastHomeRight ? mCutoutRect.right : mCutoutRect.left;
                break;
            case EUIAdaptationOrientation.RightBottom:
                curOffset.x -= mLastHomeRight ? mCutoutRect.bottom : mCutoutRect.top;
                curOffset.y += mLastHomeRight ? mCutoutRect.left : mCutoutRect.right;
                break;
        }
        mTran.localPosition += -mLastOffset + curOffset;
        mLastOffset = curOffset;
    }

    /// <summary>
    /// 是否有刘海偏移，true则偏移
    /// </summary>
    private static bool mIsFringe = false;

    /// <summary>
    /// 刘海适配列表
    /// </summary>
    private static HashSet<IUIAdaptation> mHashSetUIAdaptation = new();

    /// <summary>
    /// 上一次的Home方向
    /// </summary>
    private static bool mLastHomeRight = false;

    /// <summary>
    /// 刘海偏移信息
    /// </summary>
    private static CutoutRect mCutoutRect = null;

    public enum EUIAdaptationOrientation // 横屏状态下
    {
        None = 0,
        /// <summary>
        /// 向右偏移
        /// </summary>
        Left,
        /// <summary>
        /// 向下偏移
        /// </summary>
        Top,
        /// <summary>
        /// 向左偏移
        /// </summary>
        Right,
        /// <summary>
        /// 向上偏移
        /// </summary>
        Bottom,
        /// <summary>
        /// 向右下偏移
        /// </summary>
        LeftTop,
        /// <summary>
        /// 向右上偏移
        /// </summary>
        LeftBottom,
        /// <summary>
        /// 向左下偏移
        /// </summary>
        RightTop,
        /// <summary>
        /// 向左上偏移
        /// </summary>
        RightBottom,
    }

    [Serializable]
    public struct UILandAdaptationScale
    {
        // 4:3 或者 5:4 缩放
        public Vector3 Scale4b3;
        // 其他 缩放
        public Vector3 ScaleUn4b3;
        // 精确分辨率配置缩放
        public List<UIAdaptationData> UIScaleDatas;
    }

    [Serializable]
    public struct UILandAdaptationOffset
    {
        public UIBase BaseParent;

        // 4:3 UI偏移适配
        public Vector3 Offset4b3;

        // >=2:1 UI偏移适配
        public Vector3 Offset2b1;

        // 其他分辨率UI偏移适配
        public Vector3 OffsetOther;

        // 精确分辨率配置偏移
        public List<UIAdaptationData> UIOffsetDatas;
    }

    #region editor 获取 GameView 的分辨率
#if UNITY_EDITOR
    public struct GameViewSize
    {
        public float width;
        public float height;
        public float aspect;

        public override bool Equals(object obj)
        {
            if (!(obj is GameViewSize))
            {
                return false;
            }

            var size = (GameViewSize)obj;
            return width == size.width &&
                   height == size.height &&
                   aspect == size.aspect;
        }

        public override int GetHashCode()
        {
            var hashCode = 529352562;
            hashCode = hashCode * -1521134295 + width.GetHashCode();
            hashCode = hashCode * -1521134295 + height.GetHashCode();
            hashCode = hashCode * -1521134295 + aspect.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(GameViewSize a, GameViewSize b)
        {
            return a.width == b.width && a.height == b.height && a.aspect == b.aspect;
        }

        public static bool operator !=(GameViewSize a, GameViewSize b)
        {
            return a.width != b.width || a.height != b.height || a.aspect != b.aspect;
        }
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
