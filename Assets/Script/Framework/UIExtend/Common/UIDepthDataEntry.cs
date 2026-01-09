using UnityEngine;

    [ExecuteInEditMode]
    public class UIDepthDataEntry : MonoBehaviour, IUIDepthDataEntry
    {
        public int mBaseSortingOrder { get; set; }
        public int mMaxSortingOrder { get; set; }

        private void Awake()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return;
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                mBaseSortingOrder = canvas.sortingOrder;
                mMaxSortingOrder = mBaseSortingOrder + UILayers.UILayer.mPerCanvasAddValue - UILayers.UILayer.mPerChileCanvasAddValue;
            }
#endif
        }
    }
interface IUIDepthDataEntry
{
    /// <summary>
    /// 当前界面基础层级
    /// </summary>
    int mBaseSortingOrder { get; set; }
    /// <summary>
    /// 当前界面最大层级
    /// </summary>
    int mMaxSortingOrder { get; set; }
}

