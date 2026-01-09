using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;
using Random = System.Random;

public static class Util
{
    /// <summary>
    /// 固定协议头CMD,默认全部用这个，后面如果有其它扩展，在做处理。   注：常量暂进放到这里，后面会把https和tcp合并，然后在做移动处理
    /// </summary>
    public const int MESSAGE_CMD = 1;

    public const string ANIMEKEY = "ANIMEKEY";

    public const string GUIDEKEY = "GUIDEKEY";

    public static bool CheckAndLogError(object o, string error)
    {
        if (o == null || (o is bool && !(bool)o))
        {
            Debug.LogError(error);
            return false;
        }

        return true;
    }

    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (!comp)
            comp = go.AddComponent<T>();
        return comp;
    }

    static public Transform FindChild(Transform parent, string childName)
    {
        for (int i = 0; i < parent.childCount; ++i)
        {
            Transform tran = parent.GetChild(i);
            if (tran.name == childName)
            {
                return tran;
            }
        }

        for (int i = 0; i < parent.childCount; ++i)
        {
            Transform tran = parent.GetChild(i);
            tran = FindChild(tran, childName);
            if (tran != null)
            {
                return tran;
            }
        }

        return null;
    }

    /// <summary>
    /// 返回1h11m格式字符串
    /// </summary>
    /// <returns></returns>
    public static string GetTimeString(long endTime)
    {
        // 计算剩余时间
        var timeLeft = endTime - HeartBeatManager.GetServerTime();
        if (timeLeft <= 0)
        {
            return "Ended";
        }

        var hour = (int)timeLeft / 3600;
        var min = (int)(timeLeft % 3600) / 60;
        // 显示剩余时间字符串
        return $"{hour}h{min}m";
    }

    public static string FormatTime2Second(long endTime)
    {
        // 计算剩余时间
        var timeLeft = endTime - HeartBeatManager.GetServerTime();
        if (timeLeft <= 0)
        {
            return "";
        }

        var min = (int)(timeLeft % 3600) / 60;
        var seconds = (int)(timeLeft % 3600) % 60;
        // 显示剩余时间字符串
        return $"{min}m{seconds}s";
    }

    public static void SetParentEx(this GameObject s, Transform parent)
    {
        if (s == null || parent == null)
        {
            return;
        }

        s.transform.SetParent(parent);
    }

    public static void SetActiveEx(this GameObject s, bool active)
    {
        if (ReferenceEquals(s, null))
        {
            Debug.LogError("游戏物体为空，请查找！");
            return;
        }

        if (s.activeSelf == active) return;
        s.SetActive(active);
    }

    public static void SetUIDepth(this Transform s)
    {
        if (ReferenceEquals(s, null))
        {
            Debug.LogError("游戏物体为空，请查证！！");
            return;
        }

        var compontArray = s.GetComponentsInChildren<UIDepth>();
        if (ReferenceEquals(compontArray, null) || compontArray.Length == 0)
            return;
        foreach (var compont in compontArray)
        {
            compont.SetOrder();
        }
    }

    /// <summary>
    /// 获取当前的进度数据
    /// </summary>
    /// <param name="value">实际进度</param>
    /// <param name="total">总进度</param>
    /// <param name="mTable">进度数组</param>
    /// <param name="curUiProgress">进度数组</param>
    /// <returns></returns>
    public static float GetShowProgressValue(float value, int total, List<int> mTable, List<float> curUiProgress)
    {
        if (value >= total)
        {
            return 1;
        }

        if (value == 0 || ReferenceEquals(mTable, null) || mTable.Count == 0)
        {
            return 0;
        }

        var index = 0;
        for (var i = 0; i < mTable.Count; i++)
        {
            var table = mTable[i];
            if (value <= mTable[i])
            {
                index = i;
                break;
            }
        }

        var target = curUiProgress[index];
        if (index > 0)
        {
            var mlast = curUiProgress[index - 1];
            return (value - mTable[index - 1]) / (mTable[index] - mTable[index - 1]) * (target - mlast) + mlast;
        }
        else
        {
            if (!ReferenceEquals(mTable[index], null) && mTable[index] > 0)
            {
                return value / mTable[index] * target;
            }

            return 0;
        }
    }

    /// <summary>
    /// 返回格式00:01
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static string FormatTime(int time)
    {
        int minute = (int)Mathf.Floor(time / 60);
        var seconds = time % 60;
        return $"{minute:D2}:{seconds:D2}";
    }

    public static List<ItemConfig> GetRewardConfig(string rewardConfig)
    {
        if (string.IsNullOrEmpty(rewardConfig))
        {
            return null;
        }

        var stringSplit = rewardConfig.Split(';');
        var Length = stringSplit.Length;
        var rewardList = new List<ItemConfig>(Length);
        for (var i = 0; i < Length; i++)
        {
            var signSplit = stringSplit[i].Split('#');
            if (signSplit.Length >= 2)
            {
                var rewardClass = new ItemConfig();
                rewardClass.Id = int.Parse(signSplit[0]);
                rewardClass.Number = int.Parse(signSplit[1]);
                rewardList.Add(rewardClass);
            }
        }

        return rewardList;
    }

    /// <summary>
    /// 设置品质图片
    /// </summary>
    /// <param name="quality"></param>
    /// <param name="_qualityImage"></param>
    public static void SetQualityImage(int quality, Image _imgQuality)
    {
        var qualityPath = string.Empty;
        switch (quality)
        {
            case 1:
                qualityPath = "UI_ty_img_huise";
                break;
            case 2:
                qualityPath = "UI_ty_img_lvdi";
                break;
            case 3:
                qualityPath = "UI_ty_img_landi";
                break;
            case 4:
                qualityPath = "UI_ty_img_zise";
                break;
            case 5:
                qualityPath = "UI_ty_img_chengse";
                break;
            case 6:
                qualityPath = "UI_ty_img_hongse";
                break;
            default:
                qualityPath = "UI_ty_img_huise";
                break;
        }

        ResourceManagerNew.instance.LoadSpriteAsset(qualityPath, _imgQuality);
    }

    /// <summary>
    /// 计算MD5哈希
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string CalculateMD5Hash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }


    //寻找所有节点
    public static Transform FindDeepChild(this Transform parent, string name)
    {
        if (parent.name == name) return parent;

        foreach (Transform child in parent)
        {
            var result = child.FindDeepChild(name);
            if (result != null) return result;
        }

        return null;
    }

    public static AnimationClip GetAnimClipByName(this Animator animator, string name)
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == name)
            {
                return clip;
            }
        }

        Debug.LogError($"动画片段未找到: {name}");
        return null;
    }

    public static string GetStringByNum(int number)
    {
        if (number >= 10000000)
        {
            return (number / 1000000f).ToString("F1") + "m";
        }
        else if (number >= 1000)
        {
            return (number / 1000f).ToString("F1") + "k";
        }
        else
        {
            return number.ToString();
        }
    }

    public static string FormatNumber(int number)
    {
        if (number < 1000)
            return number.ToString();
        int integerPart = number / 1000;
        int tenths = (number % 1000) / 100;
        return tenths != 0 ? $"{integerPart}.{tenths}K" : $"{integerPart}K";
    }

    public static Vector2Int AnalysisItem(string strReword)
    {
        var tmps = strReword.Split("#");
        Vector2Int ret = new();
        ret.x = int.Parse(tmps[0]);
        ret.y = int.Parse(tmps[1]);
        return ret;
    }

    /// <summary>
    /// 传过来的时间戳和服务器时间是否同一天
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static bool IsSameDay(long timestamp)
    {
        if (timestamp == 0)
        {
            return false;
        }

        DateTime now = DateTimeOffset.FromUnixTimeSeconds(HeartBeatManager.GetServerTime()).UtcDateTime;
        // 将时间戳转换为UTC DateTime
        DateTime targetTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;

        // 只比较日期部分
        return targetTime.Date == now.Date;
    }

    /// <summary>
    /// 获取错误信息
    /// </summary>
    /// <param name="errorCode">错误码</param>
    /// <returns></returns>
    public static string GetMessage(int errorCode)
    {
        var config = Config.GetConfig<Config_DefineError>().GetConfigById(errorCode);
        if (!ReferenceEquals(config, null))
        {
            return config.ErrorDes;
        }

        return string.Empty;
    }

    /// <summary>
    /// 手机刘海设置
    /// </summary>
    public static CutoutRect GetPhoneCutout()
    {
        CutoutRect _phoneCutout = null;
        if (Application.platform == RuntimePlatform.Android)
        {
            // TODO 安卓刘海数据最好通过安卓原生获取
            _phoneCutout = new CutoutRect();
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            // TODO IPhone刘海数据通过读表获取
            _phoneCutout = new CutoutRect();
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            // TODO 编辑器刘海数据最好通过编辑器面板调整
            _phoneCutout = new CutoutRect();
        }

        return _phoneCutout;
    }

    private static readonly Random random = new Random();
    private static KeyValuePair<TKey, TValue> GetRandomItem<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary)
    {
        if (dictionary == null || dictionary.Count == 0)
            throw new ArgumentException("字典不能为空");

        int randomIndex = random.Next(0, dictionary.Count);
        return dictionary.ElementAt(randomIndex);
    }

    // 只获取随机值
    public static TValue GetRandomValue<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary)
    {
        return dictionary.GetRandomItem().Value;
    }

    // 只获取随机键
    public static TKey GetRandomKey<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary)
    {
        return dictionary.GetRandomItem().Key;
    }

    #region 二维数组
    /// <summary>
    /// 深拷贝二维数组
    /// </summary>
    public static T[,] Copy2DArray<T>(T[,] source, Func<T, T> copyFunc)
    {
        if (source == null) return null;

        int width = source.GetLength(0);
        int height = source.GetLength(1);

        T[,] result = new T[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                result[x, y] = source[x, y] != null ? copyFunc(source[x, y]) : default;
            }
        }

        return result;
    }
    
    // 获取行扩展方法
    public static T[] GetRow<T>(this T[,] array, int rowIndex)
    {
        int columns = array.GetLength(1);
        T[] row = new T[columns];
        
        for (int j = 0; j < columns; j++)
        {
            row[j] = array[rowIndex, j];
        }
        
        return row;
    }
    
    // 获取列扩展方法
    public static T[] GetColumn<T>(this T[,] array, int columnIndex)
    {
        int rows = array.GetLength(0);
        T[] column = new T[rows];
        
        for (int i = 0; i < rows; i++)
        {
            column[i] = array[i, columnIndex];
        }
        
        return column;
    }
    
    #endregion

    /// <summary>
    /// 随机打乱列表并取前n个元素
    /// </summary>
    /// <param name="originalList"></param>
    /// <param name="n"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static List<T> ShuffleAndTakeCopy<T>(List<T> originalList, int n)
    {
        if (originalList == null || originalList.Count == 0)
            return new List<T>();

        if (n > originalList.Count)
            n = originalList.Count;

        // 创建列表副本
        var list = new List<T>(originalList);
        System.Random random = new System.Random();

        for (int i = 0; i < n; i++)
        {
            var randomIndex = random.Next(i, list.Count);
            // 交换元素
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }

        // 返回前n个元素
        return list.GetRange(0, n);
    }

    #region 时间相关

    private static readonly TimeSpan East8Offset = TimeSpan.FromHours(8);
    public static DateTimeOffset GetUTC8Time(this long timestamp)
    {
        // 将时间戳转换为DateTimeOffset（UTC时间）
        DateTimeOffset utcTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        // 转换为东八区时间
        DateTimeOffset east8Time = utcTime.ToOffset(East8Offset);
        return east8Time;
    }

    public static DateTimeOffset GetCurrentEast8Time()
    {
        return DateTimeOffset.UtcNow.ToOffset(East8Offset);
    }

    /// <summary>
    /// 判断两个 时间戳 在东8区是否跨天
    /// </summary>
    public static bool IsCrossDayInEast8(long time1, long time2)
    {
        DateTimeOffset east8Time1 = GetUTC8Time(time1);
        DateTimeOffset east8Time2 = GetUTC8Time(time2);

        return east8Time1.Date != east8Time2.Date;
    }

    /// <summary>
    /// 计算到东8区下一次跨天的剩余时间
    /// </summary>
    public static TimeSpan GetTimeUntilNextCrossDay(long? time)
    {
        DateTimeOffset east8Now = time?.GetUTC8Time() ??  GetCurrentEast8Time();

        // 计算下一个午夜（东8区）
        DateTimeOffset nextMidnight = new DateTimeOffset(
            east8Now.Year, east8Now.Month, east8Now.Day,
            0, 0, 0, TimeSpan.FromHours(8)).AddDays(1);

        return nextMidnight - east8Now;
    }

    public static TimeSpan ConvertSecondsToTimeFormatSimple(long totalSeconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);
        return timeSpan;
    }

    #endregion
}
