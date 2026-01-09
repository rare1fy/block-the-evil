using System;
using System.Collections;
using System.Collections.Generic;
using AntNet;
using UnityEngine;
using Framework;
using UnityEngine.Networking;

public class WebRequestManager : MonoSingleton<WebRequestManager>
{
    private WebSocketObject _webSocketNet = null;

    private Action LoginCallback = null;

    public override void createInit()
    {
        //_webSocketNet = GameManager.Instance.GetWebSocketObjectManager();       
    }

    /// <summary>
    /// 登录请求
    /// </summary>
    /// <param name="playerName"></param>
    /// <param name="passord"></param>
    /// <param name="httpName"></param>
    /// <param name="callback"></param>
    public void RequestLogin(Action callback)
    {
        LoginCallback = callback;
        CreateSocket();
    }

    /// <summary>
    /// 错误返回
    /// </summary>
    /// <param name="errorCode"></param>
    private void ErrorCallBackFunction(int errorCode)
    {
        UIManager.Instance.HideUI("UILoading");
        UIManager.Instance.ShowCommonTip(Util.GetMessage(errorCode));
    }

    /// <summary>
    /// 连接 webscoket
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    private void CreateSocket()
    {
        Debug.LogError("开始连接的时间====" + Time.realtimeSinceStartup);
        _webSocketNet?.Connect("dev-api.lastone.games", 443, "ws", OnConnectFinishCallBack, 
            OnDisConnectCallBack, WebSocketObject.WaitTime);
    }

    /// <summary>
    /// 连接成功请求登录
    /// </summary>
    /// <param name="o"></param>
    private void OnConnectFinishCallBack(object o)
    {
        Debug.LogError("OnConnectFinishCallBack");
        var msg = new GamerLoginC2S
        {
            Id = 1,
        };

        _webSocketNet.SendMsg<GamerLoginS2C>((int)MESSAGE_ID.GAME_CMD_LOGIN_BYSESSION, msg,
            (errorCode, data) =>
        {
            if (errorCode == 0)
            {
                Debug.LogError("Gid:" + data.Id + " 收到GamerLoginS2C的时间====" + Time.realtimeSinceStartup);
                LoginCallback?.Invoke();
            }
            else
            {
                ErrorCallBackFunction(errorCode);
            }
        });
    }

    /// <summary>
    /// 连接失败回调
    /// </summary>
    /// <param name="o"></param>
    private void OnDisConnectCallBack(object o)
    {
        Debug.LogError("连接失败回调");
        UIManager.Instance.HideUI("UILoading");
    }
    
    
    /// <summary>
    /// POST JSON 数据到指定地址
    /// </summary>
    /// <param name="url">目标地址</param>
    /// <param name="jsonData">要发送的数据对象</param>
    /// <param name="onSuccess">成功回调，返回响应文本</param>
    /// <param name="onError">失败回调，返回错误信息</param>
    /// <param name="timeout">超时时间（秒）</param>
    /// <param name="headers">自定义请求头</param>
    public void PostJSON(string url, string jsonData, Action<string> onSuccess = null, Action<string> onError = null, int timeout = 30,
        Dictionary<string, string> headers = null)
    {
        StartCoroutine(PostJSONCoroutine(url, jsonData, onSuccess, onError, timeout, headers));
    }
    
    private IEnumerator PostJSONCoroutine(string url, string jsonData,
        Action<string> onSuccess, Action<string> onError,
        int timeout, Dictionary<string, string> headers)
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            // 设置处理器
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();

            // 设置请求头
            www.SetRequestHeader("Content-Type", "application/json");
            
            // 添加自定义请求头
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    www.SetRequestHeader(header.Key, header.Value);
                }
            }

            // 设置超时
            www.timeout = timeout;

            // 发送请求
            yield return www.SendWebRequest();

            // 处理响应
            if (www.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(www.downloadHandler.text);
            }
            else
            {
                string errorMsg = $"HTTP错误: {www.error}";
                if (www.responseCode != 0)
                {
                    errorMsg += $", 状态码: {www.responseCode}";
                }
                onError?.Invoke(errorMsg);
            }
        }
    }

    private void OnDestroy()
    {
        _webSocketNet?.Disconnect();
    }
}
