using AntNet;
using System;
using System.Collections.Generic;

/// <summary>
/// 消息总线，负责注册，分发事件
/// </summary>
public static class LTEventCenter
{
    /// <summary>
    /// 在update内队列执行
    /// </summary>
    private static Dictionary<string, List<Action<object>>> _queneEventMap = new Dictionary<string, List<Action<object>>>();
    /// <summary>
    /// 立即执行
    /// </summary>
    private static Dictionary<string, List<Action<object>>> _eventMap = new Dictionary<string, List<Action<object>>>();

    /// <summary>
    /// 全局事件不会被清理
    /// </summary>
    private static List<string> _globalNofityIds = new List<string>();
    /// <summary>
    /// 注册事件
    /// </summary>
    /// <param name="notifyID"></param>
    /// <param name="action"></param>
    /// <param name="inQuene"></param>
    public static void Regist(string notifyID, Action<object> action, bool inQuene, bool isGlobal = false)
    {
        if (isGlobal && !_globalNofityIds.Contains(notifyID))
            _globalNofityIds.Add(notifyID);
        var searchDict = inQuene ? _queneEventMap : _eventMap;
        if (searchDict.TryGetValue(notifyID, out var findList))
        {
            if (findList.Contains(action))
            {
                SDDebug.LogWarningFormat("{0}已存在同样事件注册{1},本次注册取消", notifyID, action);
            }
            else
            {
                findList.Add(action);
            }
        }
        else
        {
            findList = new()
                {
                    action,
                };
            searchDict.Add(notifyID, findList);
        }
    }

    /// <summary>
    /// 取消注册事件
    /// </summary>
    /// <param name="notifyID"></param>
    /// <param name="action"></param>
    /// <param name="inQuene"></param>
    public static void UnRegist(string notifyID, Action<object> action, bool inQuene)
    {
        if (_globalNofityIds.Contains(notifyID))
            return;
        var searchDict = inQuene ? _queneEventMap : _eventMap;
        if (searchDict.TryGetValue(notifyID, out var findList))
        {
            if (findList.Contains(action))
            {
                findList.Remove(action);
            }
            if (findList.Count == 0)
            {
                searchDict.Remove(notifyID);
            }
        }
    }

    /// <summary>
    /// 取消注册事件,删除所有找到事件
    /// </summary>
    /// <param name="notifyID"></param>
    /// <param name="action"></param>
    public static void UnRegist(string notifyID, Action<object> action)
    {
        UnRegist(notifyID, action, true);
        UnRegist(notifyID, action, false);
    }

    /// <summary>
    /// 直接发送回调
    /// </summary>
    /// <param name="action"></param>
    /// <param name="obj"></param>
    private static void DoActionImd(Action<object> action, object obj)
    {
        try
        {
            action(obj);
        }
        catch (Exception e)
        {
            SDDebug.LogError_Force("LTEventCenter.DoActionImd Error: " + e);
        }
    }

    /// <summary>
    /// 在队列中执行回调
    /// </summary>
    /// <param name="action"></param>
    /// <param name="obj"></param>
    private static void DoActionInQuene(Action<object> action, object obj)
    {
        Loom.QueueOnMainThread(action, obj);
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="notifyID"></param>
    /// <param name="obj"></param>
    public static void SendNotify(string notifyID, object obj, bool ENABLE_THREAD = true)
    {
        bool bCall = false;
        List<Action<object>> findList = null;
        if (_eventMap.TryGetValue(notifyID, out findList))
        {
            for (int i = 0; i < findList.Count; ++i)
            {
                DoActionImd(findList[i], obj);
                bCall = true;
            }
        }
        if (_queneEventMap.TryGetValue(notifyID, out findList))
        {
            for (int i = 0; i < findList.Count; ++i)
            {
                if (ENABLE_THREAD)
                {
                    bCall = true;
                    DoActionInQuene(findList[i], obj);
                }
                else
                {
                    bCall = true;
                    DoActionImd(findList[i], obj);
                }
            }
        }
#if UNITY_EDITOR
        if (!bCall)
        {
            SDDebug.LogWarning("---------------------------------------------can't find then callback notifyID = " + notifyID);
        }
#endif
    }

    /// <summary>
    /// 清除注册事件
    /// </summary>
    /// <param name="notifyID"></param>
    /// <param name="inQuene"></param>
    public static void ClearNotify(string notifyID, bool inQuene)
    {
        if (_globalNofityIds.Contains(notifyID))
            return;
        var searchDict = inQuene ? _queneEventMap : _eventMap;
        if (searchDict.ContainsKey(notifyID))
        {
            searchDict.Remove(notifyID);
        }
    }

    /// <summary>
    /// 清除注册事件
    /// </summary>
    /// <param name="notifyID"></param>
    public static void ClearNotify(string notifyID)
    {
        ClearNotify(notifyID, true);
        ClearNotify(notifyID, false);
    }

    /// <summary>
    /// 清除所有注册事件
    /// </summary>
    public static void ClearAll(bool isClearAll = false)
    {
        List<string> ids = new List<string>(_queneEventMap.Keys);
        string id = null;
        for (int i = 0, iLen = ids.Count; i < iLen; i++)
        {
            id = ids[i];

            if (isClearAll || !_globalNofityIds.Contains(id))
            {
                _queneEventMap[id].ForEach(act =>
                {
                    act = null;
                });
                _queneEventMap.Remove(id);
            }
        }

        ids = new List<string>(_eventMap.Keys);
        for (int i = 0, iLen = ids.Count; i < iLen; i++)
        {
            id = ids[i];
            if (isClearAll || !_globalNofityIds.Contains(id))
            {
                _eventMap[id].ForEach(act =>
                {
                    act = null;
                });
                _eventMap.Remove(id);
            }
        }
        ids = null;
        if (isClearAll)
        {
            _eventMap.Clear();
            _queneEventMap.Clear();
        }
        GC.Collect();
    }
}
