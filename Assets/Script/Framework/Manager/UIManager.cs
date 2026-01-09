using DG.Tweening;
using Framework;
using Google.Protobuf.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UILayers;

class UIManager : MonoSingleton<UIManager>
{
    public struct UIParam
    {
        public string uiName;
        public int layer;

        /// <summary>
        /// 界面类型
        /// </summary>
        public UIType uIType;

        public UIParam(string name, int layer, UIType uiType = UIType.None)
        {
            uiName = name;
            this.layer = layer;
            uIType = uiType;
        }
    }

    private Dictionary<string, UIParam> uiParam = new Dictionary<string, UIParam>(){
        {"MainWindow",new UIParam("MainWindow",1)},
        {"UILogin",new UIParam("UILogin",1)},

        {"UIFightMain", new UIParam("UIFightMain", 2)},                                  //主战斗
        {"UIFightChat", new UIParam("UIFightChat",4)},                                   //对局聊天
        {"UIFightStart", new UIParam("UIFightStart", 5)},                                //战斗开始
        {"UIFailContinue", new UIParam("UIFailContinue", 4)},                            //不可消除提示
        {"UIFightEnd", new UIParam("UIFightEnd", 4)},                                    //战斗结束
        {"UIFightEndlessEnd", new UIParam("UIFightEndlessEnd", 3)},                      //无尽战斗结束
        {"UIFightNewColor", new UIParam("UIFightNewColor", 3)},                          //新颜色
        {"UIFightNewStar", new UIParam("UIFightNewStar", 4)},                            //新角色
        {"UIMusicWindow", new UIParam("UIMusicWindow",3)},                               //音乐
        
        {"UILoading",new UIParam("UILoading",5,UIType.Static)},                           //过场动画
        {"UICommonTips",new UIParam("UICommonTips",6, UIType.Static)},                    //错误提示
        {"UPromptWindow",new UIParam("UPromptWindow",6,UIType.StaticNotInStack)},         //上浮提示
        {"UISecondConfirm",new UIParam("UISecondConfirm",6,UIType.StaticNotInStack)},     //选项提示
        {"UConnectingWindow",new UIParam("UConnectingWindow",6,UIType.StaticNotInStack)}, //短线重连

        { "UIDialogueWindow", new UIParam("UIDialogueWindow",3)},                           //外围对话框
        { "UIChatWindow", new UIParam("UIChatWindow",2)},                                   //聊天室
        { "UIContactsWindow", new UIParam("UIContactsWindow",2)},                           //npc列表
        { "UIBuildWindow", new UIParam("UIBuildWindow",2)},                    //建筑
        { "FeelWindow", new UIParam("FeelWindow",4) },                      //好感
        { "PerformWindow", new UIParam("PerformWindow",5)},                  //剧情
        { "UIPassConfirm", new UIParam("UIPassConfirm",6)},                  //通关弹窗
        { "UISuggest", new UIParam("UISuggest",6)},                     //开场提示
        { "UISetPopup", new UIParam("UISetPopup",3)},                        //设置
        { "UIFightNewSystem", new UIParam("UIFightNewSystem",4)},                          //新机制
        { "UIDdesktop", new UIParam("UIDdesktop",4)},
        { "ScreenAdsWindow", new UIParam("ScreenAdsWindow",6)},      //插屏广告
        { "UIAdsWindow", new UIParam("UIAdsWindow", 3)},    //主界面广告

    };

    private Dictionary<string, UIBase> uiName;

    private Dictionary<string, UIBase> uiStaticName;

    /// <summary>
    /// 打开的界面的列表，关闭后则移除它
    /// </summary>
    private List<UIBase> openUiBaseList = new();

    #region 层级管理

    public GameObject layer1;
    public GameObject layer2;
    public GameObject layer3;
    public GameObject layer4;
    public GameObject layer5;
    public GameObject layer6;
    public GroundGlass groundGlass;

    UILayers uiLayers = null;
    private const int mBaseSortingOrder = 500;
    private bool _isWait = false;
    #endregion

    void Awake()
    {
        uiName = new();
        uiStaticName = new();
        uiLayers = new();
        uiLayers.Layer_1 = new UILayer(layer1, mBaseSortingOrder + UILayer.mPerCanvasAddValue * 0);
        uiLayers.Layer_2 = new UILayer(layer2, mBaseSortingOrder + UILayer.mPerCanvasAddValue * 1);
        uiLayers.Layer_3 = new UILayer(layer3, mBaseSortingOrder + UILayer.mPerCanvasAddValue * 2);
        uiLayers.Layer_4 = new UILayer(layer4, mBaseSortingOrder + UILayer.mPerCanvasAddValue * 3);
        uiLayers.Layer_5 = new UILayer(layer5, mBaseSortingOrder + UILayer.mPerCanvasAddValue * 4);
        uiLayers.Layer_6 = new UILayer(layer6, mBaseSortingOrder + UILayer.mPerCanvasAddValue * 5);
        groundGlass.Init(GetResolutionSize());
    }

    /// <summary>
    /// 获取真实的屏幕尺寸
    /// </summary>
    /// <returns></returns>
    private Vector2Int GetResolutionSize()
    {
        var rectTransform = GetComponent<RectTransform>();
        return new Vector2Int((int)Mathf.Round(rectTransform.rect.width), (int)Mathf.Round(rectTransform.rect.height));
    }

    private void LateUpdate()
    {
        uiLayers.Layer_1.UpdateSorting();
        uiLayers.Layer_2.UpdateSorting();
        uiLayers.Layer_3.UpdateSorting();
        uiLayers.Layer_4.UpdateSorting();
        uiLayers.Layer_5.UpdateSorting();
        uiLayers.Layer_6.UpdateSorting();
    }

    private UILayer GetUILayerByName(int index)
    {
        UILayer layer = null;
        switch (index)
        {
            case 1:
                layer = uiLayers.Layer_1;
                break; 
            case 2:
                layer = uiLayers.Layer_2;
                break;
            case 3:
                layer = uiLayers.Layer_3;
                break;
            case 4:
                layer = uiLayers.Layer_4;
                break;
            case 5:
                layer = uiLayers.Layer_5;
                break;
            case 6:
                layer = uiLayers.Layer_6;
                break;
        }
        return layer;
    }

    public void PreLoadUI(string windowName)
    {
        StartCoroutine(RealOpenWindow(windowName, true));
    }

    public void ShowUI(string windowName, Action<GameObject> callBack = null,object param = null)
    {
        StartCoroutine(RealOpenWindow(windowName, false ,callBack, param));
    }

    private IEnumerator RealOpenWindow(string windowName, bool preLoad = false,
        Action<GameObject> callBack = null, object param = null)
    {
        while (_isWait)
        {
            yield return null;
        }
        GameObject obj = null;
        UIBase uiBase = null;

        if (uiParam.TryGetValue(windowName, out var uiStruct))
        {
            var index = uiStruct.layer;
            var curLayer = GetUILayerByName(index);
            if (uiName.TryGetValue(windowName, out uiBase) || uiStaticName.TryGetValue(windowName, out uiBase))
            {
                obj = uiBase.gameObject;
                CallBackFunction(uiBase, callBack, obj, param, preLoad, curLayer);
            }
            else
            {
                Transform parent = null;
                switch (index)
                {
                    case 1:
                        parent = layer1.transform;
                        break;
                    case 2:
                        parent = layer2.transform;
                        break;
                    case 3:
                        parent = layer3.transform;
                        break;
                    case 4:
                        parent = layer4.transform;
                        break;
                    case 5:
                        parent = layer5.transform;
                        break;
                    case 6:
                        parent = layer6.transform;
                        break;
                    default:
                        parent = layer1.transform;
                        break;
                }
                _isWait = true;
                ResourceManagerNew.instance.LoadAssetAsync(windowName, delegate (GameObject ui)
                {
                    obj = Instantiate(ui, parent);
                    uiBase = obj.GetComponent<UIBase>();
                    uiBase.BindItemNodeAndInit();
                    uiBase.UiName = windowName;
                    uiBase.CurUIType = uiStruct.uIType;
                    CallBackFunction(uiBase, callBack, obj, param, preLoad, curLayer);
                    if (uiStruct.uIType == UIType.None)
                        uiName.TryAdd(windowName, uiBase);
                    else
                        uiStaticName.TryAdd(windowName, uiBase);
                    _isWait = false;
                }, delegate ()
                {
                    _isWait = false;
                });
            }
        }
        else
        {
            Debug.LogError("UIManager.ShowUI: uiName not found:" + windowName);
        }
    }

    private void CallBackFunction(UIBase uiBase, Action<GameObject> callBack,
        GameObject obj, object param,bool preLoad, UILayer uILayer)
    {
        
        obj.SetActiveEx(!preLoad);
        if (!preLoad)
        {
            if (uiBase.CurUIType != UIType.StaticNotInStack)
            {
                if (!openUiBaseList.Contains(uiBase))
                {
                    openUiBaseList.Add(uiBase);
                }
            }
            if (uiBase.IsOpenBlur)
            {
                groundGlass.SetCanvasDisplay(false);
                groundGlass.SetCanvasSortOrder(0);
                obj.transform.localScale = Vector3.zero;
                groundGlass.CreateBlurMask();
                groundGlass.SetCanvasDisplay(true);
            }
            uiBase.SetUILayer(uILayer);
        }

        obj.SetActiveEx(!preLoad);
        if (preLoad)
        {
            uiBase.OnPreload(param);
        }
        else
        {
            uiBase.OnOpen(param);
        }

        if (!ReferenceEquals(callBack,null))
        {
            callBack(obj);
        }
    }

    public void HideUI(string windowName,bool isDestory = false)
    {
        UIBase uiBase = null;
        if (uiName.TryGetValue(windowName, out uiBase) || uiStaticName.TryGetValue(windowName, out uiBase))
        {
            var curGo = uiBase.gameObject;
            if (!isDestory)
            {
                curGo.SetActiveEx(false);
                uiBase.OnClose();
            }
            else
            {
                DestroyImmediate(curGo);
                uiName.Remove(windowName);
            }
            
            RemoveUiBaseStack(uiBase, isDestory);
            SetRawImageTexture();
        }
    }

    public void HideUI(UIBase uiBase, bool isDestory = false)
    {
        if (ReferenceEquals(uiBase, null))
            return;
        if (uiName.ContainsValue(uiBase))
        {
            var curGo = uiBase.gameObject;
            if (!isDestory)
            {
                curGo.SetActiveEx(false);
                uiBase.OnClose();
            }
            else
            {
                DestroyImmediate(curGo);
                uiName.Remove(uiBase.UiName);
            }
        }
        else if (uiStaticName.ContainsValue(uiBase))
        {
            var curGo = uiBase.gameObject;
            curGo.SetActiveEx(false);
            uiBase.OnClose();
        }
        
        
        RemoveUiBaseStack(uiBase, isDestory);
        SetRawImageTexture();
    }

    public void CloseAll(bool isDestory = false,bool isContainStatic = false)
    {
        foreach (var item in uiName)
        {
            var uiBase = item.Value;
            RemoveUiBaseStack(uiBase, isDestory);
            var curGo = uiBase.gameObject;
            if (!isDestory)
            {
                curGo.SetActiveEx(false);
                uiBase.OnClose();
            }
            else
            {
                DestroyImmediate(curGo);
            }
        }

        if (isDestory)
        {
            uiName.Clear();
        }
        
        if (isContainStatic)
        {
            foreach (var item in uiStaticName)
            {
                var uiBase = item.Value;
                RemoveUiBaseStack(uiBase, isDestory);
                uiBase.gameObject.SetActiveEx(false);
                uiBase.OnClose();
            }
        }
        SetRawImageTexture();
    }

    /// <summary>
    /// 从栈中移除
    /// </summary>
    /// <param name="uIBase"></param>
    private void RemoveUiBaseStack(UIBase uIBase, bool isDestory)
    {
        if (openUiBaseList.Count > 0 && uIBase.CurUIType != UIType.StaticNotInStack && openUiBaseList.Contains(uIBase))
        {
            openUiBaseList.Remove(uIBase);
        }
    }

    /// <summary>
    /// 关闭虚化
    /// </summary>
    /// <param name="uiBase"></param>
    private void CloseBlur()
    {
        groundGlass?.SetCanvasSortOrder(0);
        groundGlass?.SetCanvasDisplay(false);
    }

    /// <summary>
    /// 设置渲染RawImage的纹理
    /// </summary>
    private void SetRawImageTexture()
    {
        if (openUiBaseList.Count > 0)
        {
            var topUi = openUiBaseList.Last();
            if (!ReferenceEquals(topUi, null) && topUi.IsOpenBlur)
            {
                groundGlass.SetCanvasDisplay(false);
                groundGlass.SetCanvasSortOrder(0);
                topUi.transform.localScale = Vector3.zero;
                groundGlass.CreateBlurMask();
                groundGlass.SetCanvasDisplay(true);
                topUi.transform.localScale = Vector3.one;
                if (!ReferenceEquals(topUi.mCanvas, null))
                {
                    groundGlass.SetCanvasSortOrder(topUi.mCanvas.sortingOrder - 1);
                }
            }
            else
            {
                CloseBlur();
            }
        }
        else
        {
            CloseBlur();
        }
    }

    public UIBase GetTopUI()
    {
        return openUiBaseList.Count > 0 ? openUiBaseList.Last() : null;
    }

    public UIBase GetUI(string windowName)
    {
        if (uiName.TryGetValue(windowName, out var uiBase))
        {
            return uiBase;
        }
        Debug.LogError($"未找到界面{windowName}");
        return null;
    }

    public void ShowCommonTip(string errorMsg, Action action = null)
    {
        var msg = new Tuple<string, Action>(errorMsg, action);
        ShowUI("UICommonTips", param: msg);
    }

    /// <summary>
    /// 上浮提示
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="playAudio"></param>
    public void ShowPromptWindow(string msg,bool playAudio = true)
    {
        var tuple = new Tuple<int, int, int, string, bool>(1, 0, 0, msg, playAudio);
        ShowUI("UPromptWindow", null, tuple);
    }

    /// <summary>
    /// 二次确认
    /// </summary>
    /// <param name="des">描述</param>
    /// <param name="sure">确认回调</param>
    /// <param name="cancle">取消回调</param>
    public void ShowSecondConfirm(string des, Action cancle = null, Action sure = null, bool isAd = false)
    {
        var msg = new Tuple<string, Action, Action, bool>(des, sure, cancle, isAd);
        ShowUI("UISecondConfirm", param: msg);
    }

    /// <summary>
    /// 展示通用获得奖励弹窗
    /// </summary>
    /// <param name="type">类型1是道具，2是装备,3是挂机结算</param>
    /// <param name="itemConfigs">道具信息</param>
    /// <param name="equipmentInfos">装备信息</param>
    public void ShowUGlobalReward(RepeatedField<ItemConfig> itemConfigs = null, int type = 1,
        RepeatedField<EquipmentInfo> equipmentInfos = null, Action<GameObject> callBack = null,int curHitNum = 0)
    {
        var curItemConfig = new List<ItemConfig>();
        if (type == 2)
        {
            if (equipmentInfos.Count <= 0) return;
            foreach (var item in equipmentInfos)
            {
                curItemConfig.Add(new ItemConfig()
                {
                    Id = item.Id,
                    Number = 1
                });
            }
        }
        else
        {
            if (itemConfigs.Count <= 0)
            {
                return;
            }
            curItemConfig = itemConfigs.ToList();
        }

        var msg = new Tuple<int, List<ItemConfig>, int>(type, curItemConfig, curHitNum);
        ShowUI("UGlobalRewardWindow", callBack, msg);
    }



    #region ui相机震动

    public void CameraShake()
    {
        if (mCameraShake == null)
        {
            InitCamera();
        }
        if (mCameraShake != null)
        {
            StartCoroutine(mCameraShake.DoShake());
        }
    }
    
    private UICameraShake mCameraShake;
    private void InitCamera()
    {
        mCameraShake = new UICameraShake(GameObject.Find("shakeCamera"));
    }

    #endregion
    

    private static GameObject SceneChange;
    /// <summary>
    /// UI场景切换
    /// </summary>
    public static void CutToScene(Action startCall = null,Action endCall = null)
    {
        if (SceneChange == null)
        {
            ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_C_Zhuanchang001", (obj) =>
            {
                SceneChange = Instantiate(obj,_Instance.layer6.transform);
                SceneChange.SetActive(true);
                AudioManagerNew.Instance.PlayAudio("UI_Switch_Wave.ogg");
                SceneChange.SetActiveEx(true);
                DOVirtual.DelayedCall(1f, () =>
                {
                    startCall?.Invoke();
                });
                DOVirtual.DelayedCall(2f, () =>
                {
                    endCall?.Invoke();
                    SceneChange.SetActiveEx(false);
                });
            });
        }
        else
        {
            SceneChange.SetActiveEx(true);
            AudioManagerNew.Instance.PlayAudio("UI_Switch_Wave.ogg");
            DOVirtual.DelayedCall(1f, () =>
            {
                startCall?.Invoke();
            });
            DOVirtual.DelayedCall(2f, () =>
            {
                endCall?.Invoke();
                SceneChange.SetActiveEx(false);
            });
        }
    }

}

public enum UIType
{
    /// <summary>
    /// 普通UI
    /// </summary>
    None,
    /// <summary>
    /// 静态UI
    /// </summary>
    Static,
    /// <summary>
    /// 静态UI,不在栈中(没有虚化效果)
    /// </summary>
    StaticNotInStack,
}