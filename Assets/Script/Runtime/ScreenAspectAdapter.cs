using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class ScreenAspectAdapter : MonoBehaviour
{
    [Header("设计分辨率 (宽 × 高)")]
    public float designWidth = 720f;
    public float designHeight = 1559f;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        UpdateCameraViewport();
    }

    void Update()
    {
        UpdateCameraViewport();
    }

    void UpdateCameraViewport()
    {
        if (cam == null) return;

        float designRatio = designWidth / designHeight;
        float currentRatio = (float)Screen.width / Screen.height;

        // 如果当前屏幕更宽（横向拉伸）
        if (currentRatio > designRatio)
        {
            // 加左右黑边（pillarbox）
            float scale = designRatio / currentRatio;
            float margin = (1f - scale) / 2f;
            cam.rect = new Rect(margin, 0, scale, 1);
        }
        else
        {
            // 屏幕更高（或等比例）→ 保持全屏
            cam.rect = new Rect(0, 0, 1, 1);
        }
    }
}