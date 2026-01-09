using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class GuideMask : MonoBehaviour
{
    [Header("Shader参数")]
    [SerializeField] private Color maskColor = new Color(0, 0, 0, 0.6f); //遮罩颜色
    [SerializeField] private Vector2 padding = Vector2.zero; //长宽扩展

    private Material maskMaterial; // 动态材质实例
    private Image maskImage;
    private UIButtonExtension _target;
    private Action finishCB;
    
    public void Init()
    {
        maskImage = GetComponent<Image>();
        // 创建材质实例（避免修改原始材质）
        maskMaterial = new Material(maskImage.material);
        maskImage.material = maskMaterial;
        // 初始化遮罩颜色
        maskMaterial.SetColor("_MainColor", maskColor);

        GetComponent<UIButtonExtension>().onClick.AddListener(OnClickMask);
    }

    /// <summary>
    /// 更新挖孔位置和大小
    /// </summary>
    /// <param name="target">目标UI元素</param>
    /// <param name="callBack"></param>
    public void UpdateHolePosition(GameObject target, Action callBack)
    {
        if (target == null) return;

        finishCB = callBack;
        var rect = target.GetComponent<RectTransform>();
        _target = target.GetComponentInChildren<UIButtonExtension>();
        var uiCam = maskImage.canvas.worldCamera;
        // 获取目标UI的屏幕空间坐标
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        // 转换为屏幕坐标
        var min = RectTransformUtility.WorldToScreenPoint(uiCam, corners[0]);
        var max = RectTransformUtility.WorldToScreenPoint(uiCam, corners[2]);
        // 应用像素扩展
        min -= padding;
        max += padding;

        // 转换为UV坐标系（0-1范围）
        var uvMin = new Vector2(min.x / Screen.width, min.y / Screen.height);
        var uvMax = new Vector2(max.x / Screen.width, max.y / Screen.height);

        // 设置Shader参数
        maskMaterial.SetVector("_RectMin", uvMin);
        maskMaterial.SetVector("_RectMax", uvMax);
    }
    
    /// <summary>
    /// 设置点击穿透（挂载到遮罩的Button组件）
    /// </summary>
    public void OnClickMask()
    {
        var clickUV = new Vector2(
            Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height
        );

        Vector2 min = maskMaterial.GetVector("_RectMin");
        Vector2 max = maskMaterial.GetVector("_RectMax");

        //判断点击是否在挖孔区域内
        var isInside = clickUV.x >= min.x  && clickUV.x <= max.x &&
                       clickUV.y >= min.y && clickUV.y <= max.y ;

        if (isInside)
        {
            _target.Press();
            finishCB?.Invoke();
        }
    }
}