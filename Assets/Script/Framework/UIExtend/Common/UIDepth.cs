using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UIDepth : MonoBehaviour
{
    [Header("偏移层级")]
    public int order;
    [Header("是否是UI")]
    public bool isUI = false;
    [Header("当前层级")]
    public int mCurOrder = 0;

    public void SetOrder()
    {
        _UpdateDepth();
    }

    /// <summary>
    /// 编辑器设置层级
    /// </summary>
    [ContextMenu("SetOrder")]
    void _SetOrder()
    {
        _UpdateDepth();
    }

    /// <summary>
    /// 更新层级
    /// </summary>
    private void _UpdateDepth()
    {
        var sortingOrder = order;
        _GetParentUIBaseSortingOrder(transform, ref sortingOrder);
        mCurOrder = sortingOrder;
        if (isUI)
        {
            var canvas = gameObject.GetOrAddComponent<Canvas>();
            gameObject.GetOrAddComponent<GraphicRaycaster>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
        }
        else
        {
            //设置UIDepth本身 render
            var render = GetComponent<Renderer>();
            if (render)
            {
                render.sortingOrder = sortingOrder;
            }
            //设置UIDepth 管理的子物体rander
            _SetChildUIDepth(this.transform, sortingOrder, true);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(gameObject.scene.name))
            return;
        if (!Application.isPlaying)
            _UpdateDepth();
    }
    private void OnTransformChildrenChanged()
    {
        if (string.IsNullOrEmpty(gameObject.scene.name))
            return;
        if (!Application.isPlaying)
            _UpdateDepth();
    }
    private void OnTransformParentChanged()
    {
        if (string.IsNullOrEmpty(gameObject.scene.name))
            return;
        if (!Application.isPlaying)
            _UpdateDepth();
    }
#endif

    /// <summary>
    /// 获取父物体层级
    /// </summary>
    /// <param name="tran"></param>
    /// <param name="sortingOrder"></param>
    /// <returns></returns>
    private bool _GetParentUIBaseSortingOrder(Transform tran, ref int sortingOrder)
    {
        if (ReferenceEquals(tran,null))
            return false;
        var result = tran.GetComponent<IUIDepthDataEntry>();
        if (!ReferenceEquals(result,null) && result.mBaseSortingOrder > 0)
        {
            var canvas = tran.GetComponent<Canvas>();
            if (ReferenceEquals(canvas, null))
            {
                sortingOrder = order;
            }
            else
            {
                sortingOrder = canvas.sortingOrder + order;
            }
            if (sortingOrder > result.mMaxSortingOrder)
                sortingOrder = result.mMaxSortingOrder;
            return true;
        }
        return _GetParentUIBaseSortingOrder(tran.parent, ref sortingOrder);
    }

    /// <summary>
    /// 设置子物体层级
    /// </summary>
    /// <param name="tran"></param>
    /// <param name="sortingOrder"></param>
    /// <param name="isRoot"></param>
    private void _SetChildUIDepth(Transform tran, int sortingOrder, bool isRoot)
    {
        if (!isRoot)
        {
            var render = tran.GetComponent<Renderer>();
            if (render != null)
            {
                var uiDepth = tran.GetComponent<UIDepth>();
                if (uiDepth != null && !uiDepth.isUI)
                {
                    return;
                }
                render.sortingOrder = sortingOrder;
            }
        }

        for (int i = 0; i < tran.childCount; i++)
        {
            _SetChildUIDepth(tran.GetChild(i), sortingOrder, false);
        }
    }
}
