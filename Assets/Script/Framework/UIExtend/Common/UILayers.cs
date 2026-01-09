using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UILayers
{
    public UILayer Layer_1;
    public UILayer Layer_2;
    public UILayer Layer_3;
    public UILayer Layer_4;
    public UILayer Layer_5;
    public UILayer Layer_6;

    public class UILayer
    {
        public const int mPerCanvasAddValue = 500;
        public const int mPerChileCanvasAddValue = 20;
        public const int mUpperLimit = mPerCanvasAddValue / mPerChileCanvasAddValue - 1;
        public Canvas mCanvas;
        public Transform mTran;
        public int mBaseSortingOrder;
        public bool mIsUpdate;
        public UILayer(GameObject gamObj, int sortingOrder)
        {
            mTran = gamObj.transform;
            mCanvas = gamObj.GetComponent<Canvas>();
            mBaseSortingOrder = sortingOrder;
            mCanvas.sortingOrder = sortingOrder;
            mIsUpdate = false;
            mTranChilds = new Transform[mUpperLimit];
            mChildCavansSortingOrders = new int[mUpperLimit];
            for (var i = 0; i < mUpperLimit; i++)
            {
                mChildCavansSortingOrders[i] = sortingOrder + (i + 1) * mPerChileCanvasAddValue;
            }
            var UIDepthDataEntry = mTran.GetOrAddComponent<UIDepthDataEntry>();
            UIDepthDataEntry.mBaseSortingOrder = sortingOrder;
            UIDepthDataEntry.mMaxSortingOrder = sortingOrder + mPerCanvasAddValue - mPerChileCanvasAddValue;
        }
        private Transform[] mTranChilds;
        private int[] mChildCavansSortingOrders;

        public void UpdateSorting()
        {
            if (!mIsUpdate)
                return;
            mIsUpdate = false;
            var curCount = mTran.childCount;
            if (curCount > mUpperLimit)
            {
                Debug.LogErrorFormat("===UILayer Child Exceed Upper Limit===\nCurCount: {0}, Upper Limit: {1}",
                    curCount.ToString(), mUpperLimit.ToString());
                curCount = mUpperLimit;
            }
            for (var i = 0; i < curCount; i++)
            {
                var tranChild = mTran.GetChild(i);
                var gamObjChild = tranChild.gameObject;
                var uibase = tranChild.GetComponent<UIBase>();
                if (ReferenceEquals(uibase, null))
                {
                    continue;
                }
                if (ReferenceEquals(tranChild, mTranChilds[i]))
                {
                    SetGroundGlassOrder(gamObjChild, uibase);
                    continue;
                }
                mTranChilds[i] = tranChild;
                if (!uibase.mHasCanvas)
                {
                    var canvas = gamObjChild.GetOrAddComponent<Canvas>();
                    gamObjChild.GetOrAddComponent<GraphicRaycaster>();
                    canvas.overrideSorting = true;
                    uibase.mCanvas = canvas;
                    uibase.mHasCanvas = true;
                }
                uibase.mBaseSortingOrder = mChildCavansSortingOrders[i];
                uibase.mMaxSortingOrder = uibase.mBaseSortingOrder + mPerChileCanvasAddValue - 1;
                uibase.mCanvas.sortingOrder = uibase.mBaseSortingOrder;
                SetGroundGlassOrder(gamObjChild, uibase);
                tranChild.SetUIDepth();
            }
        }

        /// <summary>
        /// 设置GroundGlass的层级
        /// </summary>
        /// <param name="go"></param>
        /// <param name="uibase"></param>
        private void SetGroundGlassOrder(GameObject go, UIBase uibase)
        {
            if (go.activeSelf && uibase.IsOpenBlur)
            {
                var order = uibase.mCanvas.sortingOrder;
                UIManager.Instance.groundGlass.SetCanvasSortOrder(order - 1);
            }
        }
    }
}

