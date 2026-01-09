using AntNet;
using System;
using UnityEngine;

public class HeartBeatManager : MonoBehaviour
{
    /// <summary>
    /// 上一次 ping心跳的时间
    /// </summary>
    private float _lastPingTime = 0;

    /// <summary>
    /// 上一次服务器返回的时间
    /// </summary>
    private static float _lastServerTimeGameTime = 0;

    /// <summary>
    /// 服务器时间
    /// </summary>
    private static long _serverTime = 0;

    /// <summary>
    /// 开始心跳
    /// </summary>
    private bool _startHeadBeat = false;

    public void Update()
    {
        if (_startHeadBeat)
        {
            if (Time.unscaledTime - _lastPingTime >= 5)
            {
                _lastPingTime = Time.unscaledTime;
                HeartBeat();
            }
        }
    }

    public void ResetHeart()
    {
        _lastPingTime = 0;
        _lastServerTimeGameTime = 0;
        _serverTime = 0;
        _startHeadBeat = true;
    }

    public void StartHeadBeat()
    {
        _startHeadBeat = true;
    }

    public void StopHeadBeat()
    {
        _startHeadBeat = false;
    }

    /// <summary>
    /// 获取服务器时间
    /// </summary>
    /// <returns></returns>
    public static long GetServerTime()
    {
        return _serverTime + Convert.ToInt64((Time.unscaledTime - _lastServerTimeGameTime));
    }

    public void HeartBeat()
    {
    }
}