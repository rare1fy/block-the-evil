using System.Collections.Generic;
using UnityEngine;
using Pb;
using System.IO;
using System;
using Framework;
using System.Text.RegularExpressions;

[DisallowMultipleComponent]
public class LanguageManager : MonoSingleton<LanguageManager>
{
    public const string LanguageSavaKey = "A3AcurRegionStyle";

    public Dictionary<int, LanguageConfig> m_LanguageConfigDic = new();

    [HideInInspector]
    public LanguageStyle CurLanguageStyle = LanguageStyle.English;

    /// <summary>
    /// 缓存语言翻译，只保存包含LID或FID的
    /// </summary>
    private Dictionary<string, string> mDicLanguage = new();

    public string GetConfigById(int Id)
    {
        if (m_LanguageConfigDic.Count == 0)
        {
            LoadLanguageConfig();
        }
        if (m_LanguageConfigDic.TryGetValue(Id, out var config))
        {
            switch (CurLanguageStyle)
            {
                case LanguageStyle.ComplexFont:
                    return config.ComplexFont;
                case LanguageStyle.English:
                    return config.English;
                default:
                    return config.English;
            }
        }
        return null;
    }

    /// <summary>
    /// 加载多语言配置
    /// </summary>
    /// <param name="callBack"></param>
    private void LoadLanguageConfig(Action callBack = null)
    {
        // 加载.bytes文件
        var textAsset = Resources.Load<TextAsset>("LanguageConfig");
        if (!ReferenceEquals(textAsset, null))
        {
            // 获取字节数组
            byte[] byteData = textAsset.bytes;
            using MemoryStream stream = new(byteData);
            ReadConfig(stream);
        }
        else
        {
            Debug.LogError("Failed to load the LanguageConfig.bytes file!");
        }

        if (!ReferenceEquals(callBack, null))
        {
            callBack();
        }
    }

    /// <summary>
    /// 读取多语言配置
    /// </summary>
    /// <param name="data"></param>
    private void ReadConfig(Stream data)
    {
        var tempConfig = LanguageConfigConfig.Descriptor.Parser.ParseFrom(data) as LanguageConfigConfig;
        if (!ReferenceEquals(tempConfig, null))
        {
            var dataList = tempConfig.Data;
            if (ReferenceEquals(m_LanguageConfigDic, null))
            {
                m_LanguageConfigDic = new();
            }

            for (int i = 0, iLength = dataList.Count; i < iLength; i++)
            {
                var item = dataList[i];
                m_LanguageConfigDic.TryAdd(item.Id, item);
            }
        }
    }

    /// <summary>
    /// 翻译多语言,CustomText上文本调用
    /// </summary>
    /// <param name="strText">原始文本</param>
    ///  <param name="IsCapital">是否全部转换为大写</param>
    /// <returns></returns>
    public string AnalysiseLanguageText(string strText,bool IsCapital = false)
    {
       var LIDResult = AnalysiseLanguageLID(strText);
       var FIDResult = AnalysiseLanguageFID(LIDResult);
        var upperStr = IsCapital && CurLanguageStyle == LanguageStyle.English ? FIDResult.ToUpper() : FIDResult;
        return upperStr;
    }

    /// <summary>
    /// 解析LID
    /// </summary>
    /// <param name="strText">原始文本</param>
    /// <returns></returns>
    public string AnalysiseLanguageLID(string strText)
    {
        if (string.IsNullOrEmpty(strText) || strText.Length <= 6)
            return strText;
        if (mDicLanguage.TryGetValue(strText, out var result))
            return result;
        result = strText;
        if (string.IsNullOrEmpty(result))
        {
            Debug.LogError(strText + "  解析失败：");
            return strText;
        }
        bool isBuffer = false;
        if (_analysisLID(ref result))
            isBuffer = true;
        if (isBuffer)//如果解析成功则缓存
            mDicLanguage.Add(strText, result);
        return result;
    }

    /// <summary>
    /// 解析FID
    /// </summary>
    /// <param name="strContent"></param>
    /// <returns></returns>
    private string AnalysiseLanguageFID(string strContent)
    {
        if (strContent.Length <= 6)
            return strContent;
        string pattern = @"\[(?:FID):\d+\]";
        MatchCollection matches = Regex.Matches(strContent, pattern);
        bool _isMatch = false;
        foreach (Match match in matches)
        {
            _isMatch = true;
            var replaceStr = _ReplaceLanguage(match.Value);
            if (!string.IsNullOrEmpty(replaceStr))
                strContent = strContent.Replace(match.Value, replaceStr);
        }
        if (_isMatch)
        {
            string[] parts = strContent.Split('|', 2); // 按第一个竖线分割成两部分
            if (parts.Length > 1)
            {
                var tempDes = parts[0];
                strContent = string.Format(tempDes, parts[1].Split('|'));
            }
        }
        return strContent;
    }

    #region 多语言翻译相关

    private bool _analysisLID(ref string strContent)
    {
        if (strContent.Length <= 6)
            return false;
        string pattern = @"\[(?:LID):\d+\]";
        MatchCollection matches = Regex.Matches(strContent, pattern);
        bool _isMatch = false;
        foreach (Match match in matches)
        {
            _isMatch = true;
            var replaceStr = _ReplaceLanguage(match.Value);
            if (!string.IsNullOrEmpty(replaceStr))
                strContent = strContent.Replace(match.Value, replaceStr);
        }
        return _isMatch;
    }

    /// <summary>
    /// 替换多语言文本
    /// </summary>
    /// <param name="strContent"></param>
    /// <returns></returns>
    private string _ReplaceLanguage(string strContent)
    {
        if (strContent.Length <= 6)
            return null;

        int startIndex = 5; // 5是"[LID:"的长度
        int endIndex = strContent.IndexOf("]", startIndex);

        if (endIndex > startIndex)
        {
            var languageKey = strContent.Substring(startIndex, endIndex - startIndex);
            if (int.TryParse(languageKey, out var id))
            {
                //这里读表拿数据
                return GetConfigById(id);
            }
        }
        return null;
    }

    /// <summary>
    /// 字符串拼接
    /// </summary>
    /// <param name="strText"></param>
    /// <param name="strArray"></param>
    /// <returns></returns>
    public string Format(string strText, params object[] strArray)
    {
        var fidStr = string.Empty;
        for (int i = 0; i < strArray.Length; i++)
        {
            fidStr += strArray[i].ToString();
            if (i < strArray.Length - 1)
            {
                fidStr += "|";
            }
        }
        strText = strText + "|" + fidStr;
        var text = AnalysiseLanguageText(strText);
        return text;
    }

    /// <summary>
    /// 刷新多语言显示
    /// </summary>
    public void LangInterfaceRefresh()
    {
        GameObject[] allGamObj;
        if (!Application.isPlaying)
            allGamObj = FindObjectsOfType<GameObject>();
        else
            allGamObj = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var obj in allGamObj) 
        {
            var iLanguageRefresh = obj.GetComponent<ILanguageRefresh>();
            iLanguageRefresh?.LanguageRefresh();
        }
    }

    #endregion

    /// <summary>
    /// 清除数据
    /// </summary>
    public void ClearData() 
    {
        if (mDicLanguage != null)
            mDicLanguage.Clear();
        if (m_LanguageConfigDic != null)
            m_LanguageConfigDic.Clear();
    }

    private void OnDestroy()
    {
        ClearData();
        m_LanguageConfigDic = null;
        mDicLanguage = null;
    }

    public override void createInit() 
    {
        var curRegionStyle = (LanguageStyle)PlayerPrefs.GetInt(LanguageSavaKey, 0);
        CurLanguageStyle = curRegionStyle;
    }

    /// <summary>
    /// 查找并替换字符串
    /// </summary>
    /// <param name="strText"></param>
    /// <returns></returns>
    public bool FindAndReplaceString(ref string strText)
    {
        if (m_LanguageConfigDic.Count == 0)
        {
            LoadLanguageConfig();
        }
        var lowerStr = strText.ToLower();
        var upperStr = strText.ToUpper();
        foreach (var item in m_LanguageConfigDic.Values)
        {
            if (strText == item.English || strText == item.ComplexFont || lowerStr == item.English ||
                lowerStr == item.ComplexFont || upperStr == item.English || upperStr == item.ComplexFont)
            {
                strText = $"[LID:{item.Id}]";
                return true;
            }
        }
        return false;
    }
}

public enum LanguageStyle
{
    /// <summary>
    /// 英文
    /// </summary>
    English = 0,
    /// <summary>
    /// 繁体
    /// </summary>
    ComplexFont = 1,
}

public interface ILanguageRefresh
{
    void LanguageRefresh();
}