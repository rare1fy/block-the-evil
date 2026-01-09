using UnityEngine;
using System;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using System.Collections.Generic;
using Google.Protobuf;
using UnityWebSocket;
using ErrorEventArgs = UnityWebSocket.ErrorEventArgs;
using MessageEventArgs = UnityWebSocket.MessageEventArgs;

namespace AntNet
{
    public partial class WebSocketObject : NetObject
    {
        //读取数据大小
        //连接
        protected WebSocket _webSocket = null;                       
        //连接回调
        protected Action<NetObject> connCallback = (obj) => { };   
        //连接断开回调
        protected Action<NetObject> disconnCallback = (obj) => { };            
        
        protected byte[] headData = new byte[MessageHead.Length];
        protected byte[] unCompData = new byte[1 << 24];

        #region

        /// <summary>
        /// 发送的数据
        /// </summary>
        private NetBuffer m_SendBuffer = null;

        /// <summary>
        /// 接收的数据块
        /// </summary>
        protected byte[] m_RecvBuffer = new byte[MessageHead.MaxRecvLength];

        protected int m_nRecvBufferSize = 0;

        protected readonly object m_RecvQueueLocker = new();
        protected readonly object m_ActionLock = new();
        protected readonly object m_HeadDataLock = new();

        protected Queue<Action> m_ActionQueue = new();
        protected Queue<RecvData> m_RecvQueue = new();
        #endregion
        
        void OnDestroy()
        {
            Dispose();
            NetBuffer.Dispose();
            Stop(false);
        }

        public override void Connect(string ip, uint port, string net, Action<NetObject> onConnectFinish,
            Action<NetObject> onDisConnect, float timeout = 0)
        {
            base.Connect(ip, port, net, onConnectFinish, onDisConnect, timeout);
            curPort = port;
            _net = net;
            if (Available)
            {
                if (onConnectFinish != null)
                    onConnectFinish(this);
                return;
            }
            Stop(false);
            IPV6Help.GetIPType(ip, (int)port, out curIp, out var ipv6);
            var address = net + "s://" + curIp + ":" + port + "/" + net;
            _webSocket = new WebSocket(address);
            connCallback = onConnectFinish;
            disconnCallback = onDisConnect;
            _webSocket.OnOpen += Socket_OnOpen;
            _webSocket.OnMessage += Socket_OnMessage;
            _webSocket.OnClose += Socket_OnClose;
            _webSocket.OnError += Socket_OnError;
            _webSocket.ConnectAsync();
            if (timeout > 0)
            {
                connCoroutine = StartCoroutine(OnConnectTimeout(timeout));
            }
        }

        private void Socket_OnOpen(object sender, OpenEventArgs e)
        {
            if (connCallback!=null) { 
                connCallback(this);
            }
        }

        private void Socket_OnClose(object sender, CloseEventArgs e)
        {
            if (disconnCallback != null) {
                disconnCallback(this);
            }    
            disconnCallback = null;
        }

        private void Socket_OnError(object sender, ErrorEventArgs e)
        {
            if (_webSocket != null && _webSocket.ReadyState != WebSocketState.Closed)
            {
                _webSocket.CloseAsync();
            }
            if (disconnCallback != null)
            {
                disconnCallback(this);
            }
            disconnCallback = null;
            _webSocket = null;
        }
        
        private void Socket_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.IsBinary)
            {
                ProcessRecv(e);
            }
            else if (e.IsText)
            {
            }
        }

        public override void Stop(bool destroy)
        {
            if (_webSocket != null)
            {
                _webSocket.CloseAsync();
                _webSocket = null;
            }
            if (connCoroutine != null)
            {
                StopCoroutine(connCoroutine);
                connCoroutine = null;
            }
            base.Stop(destroy);
            m_RecvQueue.Clear();
            m_ActionQueue.Clear();
            sendque.Clear();
            m_nRecvBufferSize = 0;
            Array.Clear(m_RecvBuffer, 0, m_RecvBuffer.Length);
            if (destroy)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 发送消息 有回调
        /// </summary>
        /// <typeparam name="T"></typeparam> 回调消息类型
        /// <param name="id"></param> 协议id
        /// <param name="msg"></param> 协议内容
        /// <param name="callback"></param> 回调
        public void SendMsg<T>(int id,IMessage msg, Action<int, T> callback = null) where T : IMessage
        {
            var array = msg.ToByteArray();
            SendData sendData = SendData.GetData(id, array, 5);
            SendMsg(sendData,(errorCode, obj) => {
                if (errorCode == 0)
                {
                    if (callback != null)
                    {
                        var _obj = Activator.CreateInstance(typeof(T));
                        var data = (_obj as IMessage).Descriptor.Parser.ParseFrom(obj.data);
                        callback(0, (T)data);
                    }
                }
                else
                {
                    if (callback != null)
                    {
                        callback(errorCode, default(T));
                    }
                }
            });
        }

        /// <summary>
        /// 发送消息 无回调
        /// </summary>
        /// <param name="id"></param> 协议id
        /// <param name="msg"></param> 协议内容
        public void SendMsg(int id, IMessage msg)
        {
            var array = msg.ToByteArray();
            var sendData = SendData.GetData(id, array, 5);
            SendMsg(sendData);
        }

        public override void SendMsg(SendData send, Action<ushort, RecvData> callback = null)
        {
            if (!Available)
            {
                Debug.LogError("===WebSocket SendMsg Error, WebSocket = null===");
                OnTcpDisconnectCallback();
                send.head.error = Error.ErrNetTimeout;
                CallBack(send.head, null);
            }
            else
            {
                if (send.endTime == 0 && callback != null)
                {
                    send.endTime = Time.realtimeSinceStartup + 5;
                }
                base.SendMsg(send, callback);
                if (Available)
                    ProcessSend();
            }
        }

        public override bool Available => _webSocket != null && _webSocket.ReadyState != WebSocketState.Closed;

        /// <summary>
        /// 发送数据
        /// </summary>
        private void ProcessSend()
        {
            if (!Available)
            {
                return;
            }
            try
            {
                var m_SendData = DequeueSendData();
                if (!ReferenceEquals(m_SendData, null))
                {
                    var packet = m_SendData.GetData();
                    if (!ReferenceEquals(packet, null))
                    {
                        m_SendBuffer = NetBuffer.Alloc(packet.data.Length);
                        Buffer.BlockCopy(packet.data, 0, m_SendBuffer.data, 0, packet.Size);
                        if (!ReferenceEquals(m_SendBuffer, null) && m_SendBuffer.data.Length > 0)
                        {
                            _webSocket.SendAsync(m_SendBuffer.data);
                            ClearSendCache(m_SendData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("===TCP ProcessSend Exception===\nException: " + ex.ToString());
                OnWebSocketDisconnect();
            }
        }

        /// <summary>
        /// 获取发送队列头数据
        /// </summary>
        /// <returns></returns>
        private SendData DequeueSendData()
        {
            return sendque.Count > 0 ? sendque.Dequeue() : null;
        }

        /// <summary>
        /// 清空发送缓存
        /// </summary>
        private void ClearSendCache(SendData sendData)
        {
            if (!ReferenceEquals(sendData, null))
            {
                SendData.Free(sendData);
            }
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        private void ProcessRecv(MessageEventArgs e)
        {
            lock (m_RecvQueueLocker)
            {
                SplitPackets(e);
            }

            if (Available)
            {
                lock (m_RecvQueueLocker)
                {
                    while (m_RecvQueue.Count > 0)
                    {
                        var recvData = m_RecvQueue.Dequeue();
                        HandlePackageError(recvData.head, recvData.data);//保留以前逻辑
                        Loom.QueueOnMainThread((obj) => CallBack(recvData.head, recvData), null);
                    }
                }
            }
        }

        /// <summary>
        /// 拆包
        /// </summary>
        /// <param name="e"></param>
        private void SplitPackets(MessageEventArgs e)
        {
            lock (m_RecvQueueLocker)
            {
                int bytesRead = 0;
                try
                {
                    bytesRead = e.RawData.Length;
                }
                catch (Exception ex)
                {
                    Debug.LogError("===TCP tcpClient.GetStream().EndRead(asyncReader) Exception===\nException: " + ex.ToString());
                }

                if (bytesRead == 0)
                {
                    Debug.LogError("===TCP bytesRead = 0 : ,  TCP Available : " + Available);
                    RunAction(OnWebSocketDisconnect);
                    return;
                }
                Array.Copy(e.RawData, m_RecvBuffer, bytesRead);
                m_nRecvBufferSize += bytesRead;
                try
                {
                    int offset = 0;
                    while (m_nRecvBufferSize >= MessageHead.Length)
                    {
                        MessageHead head = null;
                        lock (m_HeadDataLock)
                        {
                            Buffer.BlockCopy(m_RecvBuffer, offset, headData, 0, MessageHead.Length);
                            head = MessageHead.Parse(headData);
                        }

                        int nLength = (int)head.len + MessageHead.Length;
                        Debug.Log("消息头字节数长度 ===>>>> " + (int)head.len + "-----接收到的总字节数 ===>>>> " + nLength
                            + "-----消息号 ===>>>> " + head.act + "-----消息下标====" + head.index);
                        if (m_nRecvBufferSize >= nLength)
                        {
                            byte[] bodyData = new byte[head.len];
                            Buffer.BlockCopy(m_RecvBuffer, offset + MessageHead.Length, bodyData, 0, (int)head.len);
                            RecvData threadRecvData = new ();
                            threadRecvData.head = head;
                            int _cmd = EncryptUtil.UintToInt(head.cmd);
                            int _act = EncryptUtil.UintToInt(head.act);
                            bodyData = EncryptUtil.Decrypt(_cmd, _act, bodyData);
                            if ((head.flags & MessageHead.Flags.FlagCompress) > 0)
                            {
                                try
                                {
                                    GZipInputStream gzi = new(new MemoryStream(bodyData));
                                    var len = gzi.Read(unCompData, 0, unCompData.Length);
                                    var xdata = new byte[len];
                                    Array.Copy(unCompData, xdata, len);
                                    threadRecvData.data = xdata;
                                }
                                catch (Exception)
                                {
                                    threadRecvData.data = bodyData;
                                }
                            }
                            else
                            {
                                threadRecvData.data = bodyData;
                            }
                            m_RecvQueue.Enqueue(threadRecvData);
                            m_nRecvBufferSize -= nLength;
                            offset += nLength;
                        }
                        else
                        {
                            break;
                        }
                    }
                    Buffer.BlockCopy(m_RecvBuffer, offset, m_RecvBuffer, 0, m_nRecvBufferSize);
                }
                catch (Exception ex)
                {
                    Debug.Log("===TCP SplitPackets Exception===\nException: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// 处理包头异常
        /// </summary>
        /// <param name="head"></param>
        /// <param name="data"></param>
        private void HandlePackageError(MessageHead head, byte[] data)
        {
            if (head.error > 0)
            {
                if (head.error != Error.ErrOk)
                {
                    Debug.LogErrorFormat("===TCP Server Error Code===\nError Code: {0}, cmd : {1} , act : {2}   cmd : ", head.error , head.cmd, head.act);
                }
            }
            if (head.len > MessageHead.MaxRecvLength)
            {
                Debug.LogFormat("===TCP Receive Lengh > Max Length===, Head.Length: {0}, Max Length: {1}", head.len.ToString(), MessageHead.MaxRecvLength.ToString());
                OnWebSocketDisconnect();
            }

            if (head.len > 0 && (data == null || data.Length == 0))
            {
                Debug.Log("===TCP Head.length > 0 But Data = null Or Data.Length = 0===" + 0);
                OnWebSocketDisconnect();
            }
        }

        /// <summary>
        /// 添加Action
        /// </summary>
        /// <param name="action"></param>
        private void RunAction(Action action)
        {
            if (action == null)
            {
                return;
            }

            lock (m_ActionLock)
            {
                m_ActionQueue.Enqueue(action);
            }
        }

        /// <summary>
        /// 断开网络连接
        /// </summary>
        private void OnWebSocketDisconnect()
        {
            IsDelayTimeOut = false;
            Stop(false);
            if (!ReferenceEquals(disconnCallback, null))
            {
                disconnCallback(this);
            }
            disconnCallback = null;
        }

        public void Disconnect()
        {
            IsDelayTimeOut = false;
            Stop(false);
            disconnCallback = null;
        }

        public override void ClearAllCallBack()
        {
            base.ClearAllCallBack();
            connCallback = null;
        }
    }
}