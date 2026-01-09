using System;
using System.Threading.Tasks;

public interface IPlatformAdapter
{
    Task Initialize();
    
    void RegisterEvents();
    
    void LoadData(Action<PlayerData> onComplete);
    
    void SaveData(PlayerData data);
    
    void ShareMessage(string title, string imageUrl, Action<Object> onFinish);

    void ShortVibration(int level);
    
    void ShowRewardedVideoAd(int adType, Action<bool> onCloseResponse, Action<string> onFailed = null);
    
    void ShowInterstitialAd(Action onCloseResponse, int adType, Action<string> onFailed = null);
}