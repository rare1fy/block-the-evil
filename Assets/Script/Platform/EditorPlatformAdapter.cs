using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Object = System.Object;

public class EditorPlatformAdapter : IPlatformAdapter
{
    public void LoadData(Action<PlayerData> onComplete)
    {
        var filePath = Path.Combine(Application.dataPath, "data");
        
        var dir = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dir) && dir != null)
            Directory.CreateDirectory(dir);
        
        var path = Path.Combine(Application.dataPath, filePath + ".json");
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                if (!string.IsNullOrEmpty(json))
                {
                    var obj = JsonConvert.DeserializeObject<PlayerData>(json);
                    onComplete?.Invoke(obj ?? new PlayerData());
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("解析存档失败: " + e.Message);
            }
        }
        onComplete?.Invoke(new PlayerData());
    }

    public void SaveData(PlayerData data)
    {
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        
        var filePath = Path.Combine(Application.dataPath, "data");
        var path = Path.Combine(Application.dataPath, filePath + ".json");
        var dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir) && dir != null)
            Directory.CreateDirectory(dir);
        
        File.WriteAllText(path, json);
    }

    public void ShortVibration(int level)
    {
        
    }

    public void ShowRewardedVideoAd(int adType, Action<bool> onCloseResponse, Action<string> onFailed = null)
    {
        onCloseResponse?.Invoke(true);
    }

    public void ShowInterstitialAd(Action onCloseResponse, int adType, Action<string> onFailed = null)
    {
        onCloseResponse?.Invoke();
    }

    public Task Initialize()
    {
        return Task.CompletedTask;
    }

    public void RegisterEvents()
    {
        
    }

    public void ShareMessage(string title, string imageUrl, Action<Object> onFinish)
    {
        onFinish?.Invoke(null);
    }
}