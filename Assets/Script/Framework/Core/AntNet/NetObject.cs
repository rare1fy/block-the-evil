using UnityEngine;
using System;
using CustomDataStruct;
using System.Collections.Generic;
using Google.Protobuf;

namespace AntNet
{
    public class NetObject : MonoBehaviour
    {
        /// <summary>
        /// 是否延迟超时
        /// </summary>
        public bool IsDelayTimeOut = false;

        /// <summary>
        /// 发送队列
        /// </summary>
        protected Queue<SendData> sendque = new ();

        protected Dictionary<int, Action<ushort, RecvData>> onceCallbackDict = new ();          //一次回调，cs模式交互

        protected Dictionary<int, Action<ushort, RecvData>> foreverCallbackDict = new ();      //永远回调，用于服务器通知等

        /// <summary>
        /// 超时处理,用于删除
        /// </summary>
        protected Dictionary<int, MessageHead> indexDict = new ();

        protected UnOrderMultiMap<float, int> timeoutDict = new (); //用于计算

        protected Dictionary<float, float> timeoutTimeDict = new (); //用于计算

        /// <summary>
        /// 清空超时消息
        /// </summary>
        private bool m_clearTimeOutMsg = false;

        private bool isDestoryNet = false;
 
        private void SetForeverCallBack(int cmd, Action<ushort, RecvData> callback)
        {
            if (isDestoryNet)
                return;
            if (callback == null)
            {
                if (foreverCallbackDict.ContainsKey(cmd))
                {
                    foreverCallbackDict[cmd] = null;
                    foreverCallbackDict.Remove(cmd);
                }
            }
            else
            {
                foreverCallbackDict[cmd] = callback;
            }
        }

        /// <summary>
        /// 协议请求返回事件
        /// </summary>
        /// <typeparam name="T"></typeparam> 协议返回数据类型
        /// <param name="act"></param> 协议ID
        /// <param name="callback"></param> 回调函数
        public void SetMessageCallBack<T>(int act, Action<int, T> callback) where T :IMessage
        {
            Action<ushort,RecvData> _act = (errorCode, obj) => {
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
                };
            };
            SetMessageCallBack(act, _act);
        }

        /// <summary>
        /// 协议请求返回事件
        /// </summary>
        /// <param name="act"></param> 协议id
        /// <param name="callback"></param> 回调函数
        private void SetMessageCallBack(int act, Action<ushort, RecvData> callback) {
            SetForeverCallBack(Util.MESSAGE_CMD, act, callback);
        }

        /// <summary>
        /// 移除协议请求返回事件
        /// </summary>
        /// <param name="act"></param> 协议id
        public void MoveMessageCallBack(int act) {
            SetForeverCallBack(Util.MESSAGE_CMD, act, null);
        }
        
        private void SetForeverCallBack(int cmd, int act, Action<ushort, RecvData> callback)
        {
            int key = cmd * 1024 + act;
            SetForeverCallBack(key, callback);
        }

        public virtual void ClearAllCallBack()
        {
            isDestoryNet = true;
            var intList= ListPool<int>.Get();
            intList.AddRange(foreverCallbackDict.Keys);
            foreach (var foreverCall in intList)
            {
                foreverCallbackDict[foreverCall] = null;
            }
            ListPool<int>.Release(intList);
            intList = ListPool<int>.Get();
            intList.AddRange(onceCallbackDict.Keys);
            foreach (var onceCall in intList)
            {
                foreverCallbackDict[onceCall] = null;
            }
            ListPool<int>.Release(intList);
            foreverCallbackDict.Clear();
            onceCallbackDict.Clear();
        }

        public virtual void SendMsg(SendData send, Action<ushort, RecvData> callback = null)
        {
            sendque.Enqueue(send);
            if (callback != null && !isDestoryNet)
            {
                onceCallbackDict[send.head.index] = callback;
            }
            if (send.endTime > 0)
            {
                var headMsg= MessageHead.getHeadData(send.head);
                if (timeoutDict.ContainsKey(send.endTime))
                {
                    //send.head.nextTimeout = timeoutDict[send.endTime];
                    timeoutDict[send.endTime].Add(send.head.index);
                    timeoutTimeDict[send.endTime] = send.endTime;
                }
                else
                {
                    timeoutDict.Add(send.endTime, headMsg.index);
                }
                indexDict[send.head.index] = headMsg;
            }
        }

        protected void CallBack(MessageHead head, RecvData recv)
        {
            int key = head.CmdAct;
            if (foreverCallbackDict.ContainsKey(key))
            {
                foreverCallbackDict[key](head.error, recv);
            }

            if (foreverCallbackDict.ContainsKey(head.cmd))
            {
                foreverCallbackDict[head.cmd](head.error, recv);
            }

            if (onceCallbackDict.ContainsKey(head.index))
            {
                if (head.error > 0)
                    Debug.LogError("---------------------------------错误码---------------------" + head.error);
                if (head.error != Error.ErrNetTimeout)
                {
                    onceCallbackDict[head.index](head.error, recv);
                }
                onceCallbackDict.Remove(head.index);
            }

            var index = head.index;
            if (indexDict.ContainsKey(index))
            {
                MessageHead.Free(indexDict[index]);
                indexDict.Remove(index);
            }
        }

        public virtual void Stop(bool destroy) 
        {
            m_clearTimeOutMsg = true;
        }

        List<float> delMsgKeys = new List<float>();
        protected virtual void LateUpdate()
        {
            if (m_clearTimeOutMsg)
            {
                timeoutTimeDict.Clear();
                timeoutDict.Clear();
                m_clearTimeOutMsg = false;
            }
            delMsgKeys.Clear();
            var keys = ListPool<float>.Get();
            keys.AddRange(timeoutDict.Keys);
            for (int j = 0; j < keys.Count; j++)
            {
                // 超时时间点
                float key = keys[j];
                float time = 0;
                if (!timeoutTimeDict.TryGetValue(key, out time))
                    time = key;

                if (Time.realtimeSinceStartup < time)
                {
                    continue;
                }
                var headIndexs = timeoutDict[key];
                delMsgKeys.Add(key);
                for (int i = 0; i < headIndexs.Count; i++)
                {
                    if (indexDict.ContainsKey(headIndexs[i]))
                    {
                        var head = indexDict[headIndexs[i]];
                        head.error = Error.ErrNetTimeout;
                        Debug.LogError(string.Format("网络超时-----------index={0},    cmd={1},     act={2}",
                            head.index, head.cmd, head.act));
                        CallBack(head, null);
                    }
                }
                if (timeoutTimeDict.ContainsKey(key))
                {
                    timeoutTimeDict.Remove(key);
                }
            }
            for (int i = 0; i < delMsgKeys.Count; i++)
            {
                timeoutDict.Remove(delMsgKeys[i]);
            }
        }

        virtual public bool Available
        { 
            get
            {
                return false;
            }
        }

        public virtual void Connect(string ip, uint port,string net,Action<NetObject> onConnectFinish, Action<NetObject> onDisConnect, float timeout = 0)
        {
            m_clearTimeOutMsg = true;
        }

        public void Dispose()
        {
            SendData.Dispose();
            sendque?.Clear();
            onceCallbackDict?.Clear();
            foreverCallbackDict?.Clear();
            indexDict?.Clear();
            timeoutDict?.Dispose();
            timeoutTimeDict?.Clear();
        }
    }
}