using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIParticleMask : Mask
{
    [SerializeField] private int updateRectClipFrame = 30;

    #region Fields

    private Renderer[] _cachedRenders;
    private int waitFrameCount;
    private Vector3[] corners;

    #endregion

    #region public

    protected override void Awake()
    {
        base.Awake();
        //InitMaskImag();
        InitComponent();
    }

    protected override void Start()
    {
        base.Start();
        corners = new Vector3[4];
        UpdateClipRect();
    }


    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        UpdateClipRect();
    }

    public void InitComponent()
    {
        _cachedRenders = transform.GetComponentsInChildren<Renderer>(true);
    }

    #endregion

    #region private

    public void UpdateClipRect()
    {
        InitComponent();
        if (_cachedRenders == null || _cachedRenders.Length == 0)
            return;
        if (corners == null)
        {
            corners = new Vector3[4];
        }

        this.rectTransform.GetWorldCorners(corners);
        float minX = corners[0].x;
        float minY = corners[0].y;
        float maxX = corners[2].x;
        float maxY = corners[2].y;
        for (int i = 0; i < _cachedRenders.Length; i++)
        {
            var r = _cachedRenders[i];
            if (r.material && r.material.shader.name.StartsWith("Particles"))
            {
                r.material.SetFloat("_MinX", minX);
                r.material.SetFloat("_MinY", minY);
                r.material.SetFloat("_MaxX", maxX);
                r.material.SetFloat("_MaxY", maxY);
            }
        }
    }


    void Update()
    {
        waitFrameCount++;
        if (waitFrameCount > updateRectClipFrame)
        {
            UpdateClipRect();
            waitFrameCount = 0;
        }
    }

    protected override void OnDestroy()
    {
    }

    void InitMaskImag()
    {
        MaskableGraphic maskImag = GetComponent<MaskableGraphic>();
        if (maskImag != null)
        {
            DestroyImmediate(maskImag);
        }

        if (maskImag == null)
        {
            maskImag = gameObject.AddComponent<Image>();
        }

        if (maskImag != null)
        {
            Color color = maskImag.color;
            color.a = 1f / 255f;
            maskImag.color = color;
        }
    }

    #endregion
}