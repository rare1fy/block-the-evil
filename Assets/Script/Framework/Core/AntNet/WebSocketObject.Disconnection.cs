using Framework;
using System.Collections;
using UnityEngine;

namespace AntNet
{
    public partial class WebSocketObject
    {
        /// <summary>
        /// ip
        /// </summary>
        private string curIp = string.Empty;

        /// <summary>
        /// 端口号
        /// </summary>
        private uint curPort = 0;

        /// <summary>
        /// 登录的_net信息
        /// </summary>
        private string _net = string.Empty;

        /// <summary>
        /// 首次登录协成
        /// </summary>
        protected Coroutine connCoroutine = null;

        /// <summary>
        /// 最大重试次数
        /// </summary>
        private const int maxRetries = 5;

        /// <summary>
        /// 当前重试次数
        /// </summary>
        private int currentRetryCount = 0;

        /// <summary>
        /// 协成等待时间
        /// </summary>
        private WaitForSecondsRealtime _waitForTime = new(WaitTime);

        /// <summary>
        /// 等待时间
        /// </summary>
        public const float WaitTime = 3f;

        /// <summary>
        /// 重连消息发送次数
        /// </summary>
        private int _connectTime = 0;

        /// <summary>
        /// 最大重连消息发送次数
        /// </summary>
        private const int maxConnectTime = 3;

        /// <summary>
        /// 首次连接超时处理
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        protected IEnumerator OnConnectTimeout(float timeout)
        {
            yield return new WaitForSeconds(timeout);
            if (_webSocket != null)
            {
                Debug.LogError("===WebSocket Connect Time Out===\nTimeOut: " + timeout);
                OnWebSocketDisconnect();
                UIManager.Instance.ShowCommonTip(Util.GetMessage(200), delegate () 
                {

                });
            }
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            if (!Available || ReferenceEquals(connCoroutine, null))
            {
                return;
            }
            StopCoroutine(connCoroutine);
            connCoroutine = null;
        }

        public void OnTcpDisconnectCallback()
        {
            if (currentRetryCount > 0) //正在重连中
            {
                return;
            }
            EventDispatchCenter.Instance.Dispatch(SDEvents.S2C_NETWORK_DISCONNECT);
            UIManager.Instance.ShowUI("UConnectingWindow");
            Stop(false);
            GameManager.Instance.GetHeartBeatManager().StopHeadBeat();
            StopCoroutine(CheckConnection());
            // 启动网络重连
            StartCoroutine(CheckConnection());
        }

        /// <summary>
        /// 检查重连
        /// </summary>
        /// <returns></returns>
        private IEnumerator CheckConnection()
        {
            while (!Available)
            {
                // 重连次数限制
                if (currentRetryCount > maxRetries)
                {
                    UIManager.Instance.HideUI("UConnectingWindow");
                    UIManager.Instance.ShowCommonTip("Loss of network connection!!!", OnBackToLogin);
                    yield break;
                }
                // 增加重试计数
                currentRetryCount++;
                EventDispatchCenter.Instance.Dispatch(SDEvents.S2C_NETWORK_DCONNECT_TIMES, currentRetryCount);
                if (Available)
                {
                    // 在这里恢复游戏状态
                    RestoreGameState();
                    yield break;
                }
                else
                {
                    Connect(curIp, curPort, _net, delegate (NetObject net)
                    {
                        OnFastReconnC2SCallBack();
                    }, null);
                    // 等待一定时间后重试
                    yield return _waitForTime;
                }
            }
        }

        /// <summary>
        /// 恢复游戏状态
        /// </summary>
        private void RestoreGameState()
        {
            StopCoroutine(CheckConnection());
            EventDispatchCenter.Instance.Dispatch(SDEvents.S2C_NETWORK_RECONNECTION);
            // 连接成功
            UIManager.Instance.HideUI("UConnectingWindow");
            // 重置重试次数
            currentRetryCount = 0;
            _connectTime = 0;
            GameManager.Instance.GetHeartBeatManager().StartHeadBeat();
        }

        /// <summary>
        /// 连接成功请求快速重连
        /// </summary>
        private void OnFastReconnC2SCallBack()
        {
        }

        /// <summary>
        /// 返回登录
        /// </summary>
        private void OnBackToLogin()
        {
            _connectTime = 0;
            currentRetryCount = 0;
            AudioManagerNew.Instance.StopAllAudio(true);
#if !UNITY_EDITOR 
            var uIManager = UIManager.Instance;
            uIManager.CloseAll();
            uIManager.ShowUI("UILogin", delegate (GameObject ui)
            {
                uIManager.PreLoadUI("UILoading");
            });
#else
            var uIManager = UIManager.Instance;
            uIManager.CloseAll();
            uIManager.ShowUI("UILogin", delegate (GameObject ui)
            {
                uIManager.PreLoadUI("UILoading");
            });
#endif
            StopAllCoroutines();
            Time.timeScale = 1;
        }
    }
}
