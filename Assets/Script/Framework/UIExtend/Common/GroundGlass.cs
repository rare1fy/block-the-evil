using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(RawImage))]
public class GroundGlass : MonoBehaviour
{
    [Header("UI材质球")]
    public Material mMatCreateShinerMask = null;//创建
    public RawImage mRawImage = null;
    public GameObject mMask = null;

    [HideInInspector]
    public int mInterations = 3;
    [HideInInspector]
    public int mBlurSize = 2;

    /// <summary>
    /// 渲染尺寸
    /// </summary>
    [HideInInspector]
    public Vector2Int mRendererSize = new (1280, 720);

    /// <summary>
    /// 降采样次数
    /// </summary>
    [HideInInspector]
    public int mDownSample = 2;

    /// <summary>
    /// 渲染的纹理对象
    /// </summary>
    private RenderTexture mRenderTex = null;

    /// <summary>
    /// 虚化的图片
    /// </summary>
    private RenderTexture mRenderBlur = null;

    public void Init(Vector2Int realSize)
    {
        mRawImage = GetComponent<RawImage>();
        mRendererSize = realSize;
        CreateMaterial();
        RecreateRenderTexture();
    }

    public void CreateMaterial()
    {
#if UNITY_EDITOR
       mMatCreateShinerMask = new Material(Shader.Find("2D/CreateShinerMask"));
#endif
    }

    /// <summary>
    /// 创建虚化遮罩
    /// </summary>
    public void CreateBlurMask()
    {
        var camera = Camera.main;
        if (!ReferenceEquals(camera, null))
        {
            camera.targetTexture = mRenderTex;
            camera.Render();
            camera.targetTexture = null;
        }
        var worldCamera = mRawImage.canvas.worldCamera;
        if (!ReferenceEquals(worldCamera, null))
        {
            worldCamera.targetTexture = mRenderTex;
            worldCamera.Render();
            worldCamera.targetTexture = null;
        }
        mMatCreateShinerMask.SetFloat("_BlurSize", mBlurSize);
        for (var i = 0; i < mInterations; i++)
        {
            Graphics.Blit(mRenderTex, mRenderBlur, mMatCreateShinerMask, 4);
            Graphics.Blit(mRenderBlur, mRenderTex, mMatCreateShinerMask, 4);
        }
    }

    /// <summary>
    /// 重新创建纹理
    /// </summary>
    public void RecreateRenderTexture()
    {
        if (!ReferenceEquals(mRenderTex,null))
            DestroyImmediate(mRenderTex);
        mRenderTex = new (mRendererSize.x / mDownSample, mRendererSize.y 
            / mDownSample, 1, RenderTextureFormat.ARGB32);

        mRenderTex.name = "RenderTex";
        if (!ReferenceEquals(mRenderBlur,null))
            DestroyImmediate(mRenderBlur);
        mRenderBlur = new (mRendererSize.x / mDownSample, mRendererSize.y 
            / mDownSample, 0, RenderTextureFormat.ARGB32);

        mRenderBlur.name = "RenderBlur";
        mRawImage.texture = mRenderTex;
    }

    /// <summary>
    /// 设置层级
    /// </summary>
    /// <param name="order"></param>
    public void SetCanvasSortOrder(int order)
    {
        if (mRawImage.canvas.sortingOrder != order)
            mRawImage.canvas.sortingOrder = order;
    }

    public void SetCanvasDisplay(bool state)
    {
        mRawImage.gameObject.SetActiveEx(state);
        if (!ReferenceEquals(mMask,null))
        {
            mMask.SetActiveEx(state);
        }
    }

    private void OnDestroy()
    {
        mRawImage.texture = null;
        mRenderTex = null;
        mRenderBlur = null;
        mMatCreateShinerMask = null;
    }
}