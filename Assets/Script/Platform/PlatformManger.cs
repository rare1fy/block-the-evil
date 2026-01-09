using System;
using Framework;
using System.Threading.Tasks;

public class PlatformManager : MonoSingleton<PlatformManager>
{
    private IPlatformAdapter currentPlatform;

    public async Task InitializePlatform()
    {
#if UNITY_EDITOR
        currentPlatform = new EditorPlatformAdapter(); // 编辑器适配器
#elif WEIXINMINIGAME
        currentPlatform = new WeChatAdapter();
#elif OTHER_PLATFORM
        currentPlatform = new OtherPlatformAdapter();
#endif
        
        await currentPlatform.Initialize();
        currentPlatform.RegisterEvents();
    }

    public void LoadData(Action<PlayerData> onComplete)
    {
        currentPlatform.LoadData(onComplete);
    }

    public void SaveData(PlayerData data)
    {
        currentPlatform.SaveData(data);
    }

    public void ShareMessage(string title, string imageUrl, Action<Object> onFinish)
    {
        currentPlatform?.ShareMessage(title, imageUrl, onFinish);
    }
    
    public void ShortVibration(int level)
    {
        currentPlatform?.ShortVibration(level);
    }

    // 统一的广告接口
    public void ShowRewardedVideoAd(int adType, Action<bool> onCloseResponse, Action<string> onFailed = null)
    {
        currentPlatform?.ShowRewardedVideoAd(adType, onCloseResponse, onFailed);
    }

    public void ShowInterstitialAd(Action onCloseResponse, int adType, Action<string> onFailed = null)
    {
        currentPlatform?.ShowInterstitialAd(onCloseResponse, adType, onFailed);
    }

    // 获取当前平台实例（用于平台特定功能）
    public T GetPlatformAdapter<T>() where T : class, IPlatformAdapter
    {
        return currentPlatform as T;
    }
}