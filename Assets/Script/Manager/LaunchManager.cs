using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Framework;
using UnityEngine;
using YooAsset;
using DG;
using DG.Tweening;

public class LaunchManager : MonoSingleton<LaunchManager>
{
    [Header("编辑器调试模式将选择WebPlayMode")] public bool isDebug = false;
    [Header("资源加载预制体")] public LaunchWindow launchWindow;

    /// <summary>
    /// 资源系统运行模式
    /// </summary>
    private EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

    private const string PACKAGENAME = "DefaultPackage";

    private async void Start()
    {
        isNotDestory = false;

        EPlayMode PlayMode = EPlayMode.EditorSimulateMode;
#if UNITY_WEBGL && !UNITY_EDITOR
    PlayMode = EPlayMode.WebPlayMode;
#elif UNITY_ANDROID && !UNITY_EDITOR
    PlayMode = EPlayMode.OfflinePlayMode;
#endif
        
        await PlatformManager.Instance.InitializePlatform();
        
        Application.targetFrameRate = 60;
        if (!YooAssets.Initialized)
        {
            YooAssets.Initialize();
            launchWindow?.gameObject.SetActiveEx(true);

            var initializationOperation = YooAssetManager.Initialize(PlayMode);
            await initializationOperation.ToUniTask();
            
            if (initializationOperation.Status != EOperationStatus.Succeed)
            {
                throw new Exception("资源系统初始化失败");
            }
            
            var requestPackageVersionOperation = YooAssetManager.RequestPackageVersionAsync();
            await requestPackageVersionOperation.ToUniTask();
            
            if (requestPackageVersionOperation.Status != EOperationStatus.Succeed)
            {
                throw new Exception("获取包版本失败");
            }
        
            YooAssetManager.PackageVersion = requestPackageVersionOperation.PackageVersion;
            var operation = YooAssetManager.UpdatePackageManifestAsync(requestPackageVersionOperation.PackageVersion);

            var indexValue = 0f;
            launchWindow?.SetSliderValue(0);
            launchWindow?.SetVersion(YooAssetManager.PackageVersion);
            while (!operation.IsDone)
            {
                indexValue = Mathf.Min(indexValue + 1.3f, 99f);
                launchWindow.SetSliderValue(indexValue);
                await UniTask.Yield();
            }
        }
        
        var gamePackage = YooAssets.GetPackage(PACKAGENAME);
        YooAssets.SetDefaultPackage(gamePackage);

        // 加载配置表
        await GameManager.Instance.LoadConfigAsync();
    
        // 不连接网络直接开始(单机模式)
        CallbackLogin();
    }

    /// <summary>
    /// 登录回调
    /// </summary>
    /// <param name="code"></param>
    /// <param name="data"></param>
    private void CallbackLogin()
    {
        UIManager.Instance.ShowUI("UILoading");
        GameManager.Instance.Launch();
        PlayerDataManager.instance.LoadData();
        StartCoroutine(GameStart());
    }


    private int _temp = 0;
    private int _delayTimes = 180;
    private IEnumerator GameStart()
    {
        _temp = 0;
        while (_readyCount < _totalSystemsCount || _temp < _delayTimes)
        {
            _temp++;
            yield return null;
        }
        
        GameManager.Instance.LogManager.Log_Login();
        var sceneHandle = YooAssets.LoadSceneAsync("Scene_Main");
        if (sceneHandle != null)
        {
            sceneHandle.Completed += delegate(SceneHandle sceneHandle)
            {
                // 切换到主页面场景
                if (PlayerPrefs.GetInt(Util.ANIMEKEY, 0) == 0)
                {
                    PlayerPrefs.SetInt(Util.ANIMEKEY, 1);
                    GameManager.Instance.PlayerControl.StartFight();
                }
                else
                {
                    UIManager.Instance.PreLoadUI("MainWindow");
                    UIManager.CutToScene(() =>
                    {
                        UIManager.Instance.ShowUI("MainWindow");
                        UIManager.Instance.HideUI("UILoading");
                    });
                }
            };
        }
        
        yield return null;
    }
    
    
    private  int _totalSystemsCount = 0; 
    private int _readyCount = 0;
    
    public void RegisterSystemWaitForInit()
    {
        _totalSystemsCount++;
    }
    
    public void MaskSystemReady()
    {
        _readyCount++;
    }
    
}